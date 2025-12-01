
using System.Reflection;
using System.Runtime.CompilerServices;
using GlmSharp;
using ImGuiNET.Backend.SDLGPU;
using panpan;
using panpan.Rendering;
using panpan.Scene;
using panpan.Util;

namespace panpanExample
{
    public class TileEditor
    {
        enum BrushType
        {
            BRUSH,
            RECT,
            LINE
        }

        private bool editing = false;
        private bool snap = true;
        private bool canPlace = true;
        private bool canOverwrite = true;
        private bool drawingRect = false;
        private bool removingRect = false;
        private Rect drawRect; 
        private bool fill = true;
        private int hoverTimer = 6;
        private ivec2 snapSize = new ivec2(8, 8);

        private BrushType brushType = BrushType.BRUSH;
        private Texture? brushTex;
        private ivec2 brushPos;
        private int selectedTileType = 0;
        private TileSet tileSet = TileSets.Dirt;
        private TileMap[,] tilemaps = ((TestScene)App.GetSceneManager().ActiveScene).TileMaps;
        private TileMap? activeTileMap;

        public bool Visible = false;

        public void Init()
        {
            activeTileMap = activeTileMap = tilemaps[0,0];
            brushTex = new Texture(panpan.Assets.Sprites.tile, 8, 8);
            brushTex.CopyPass();
            Input.RegisterOnMouseHeld(OnMouseHeld);
            Input.RegisterOnMouseDown(OnMouseDown);
            Input.RegisterOnMouseReleased(OnMouseUp);
        }

        private void OnMouseHeld(byte btn)
        {
            if(canPlace && editing)
            {
                if(brushType == BrushType.BRUSH)
                {
                    if (btn == 1 && !App.GetCollisionManager().IntersectsPosition(Input.MousePosition, typeof(Tile)))
                    {
                        PlaceTile((uint)brushPos.x, (uint)brushPos.y);
                    }
                    if(btn == 3 && App.GetCollisionManager().IntersectsPosition(Input.MousePosition, typeof(Tile)))
                    {
                        RemoveTile((uint)brushPos.x, (uint)brushPos.y);
                        activeTileMap!.UpdateAutoTiles();
                    }
                }
            }
        }

        private void OnMouseDown(byte btn)
        {
            if(canPlace && editing)
            {
                if(brushType == BrushType.RECT)
                {
                    if(btn == 1)
                        drawingRect = true;
                    if(btn == 3)
                        removingRect = true;
                    drawRect.X = brushPos.x;
                    drawRect.Y = brushPos.y;
                }
            }
        }

        private void OnMouseUp(byte btn)
        {
            if(canPlace && editing)
            {
                if(brushType == BrushType.RECT && (drawingRect || removingRect))
                {
                    drawRect.Width = brushPos.x - drawRect.X;
                    drawRect.Height = brushPos.y - drawRect.Y;

                    if(!fill) // outline
                    {
                        for(var w = 0; w < Math.Abs(drawRect.Width)+1; w++)
                        {
                            var sign = Math.Sign(drawRect.Width);
                            var ws = w*sign;
                            if(drawingRect)
                            {
                                int xx = (drawRect.X + ws)/320;
                                int yy = drawRect.Y/176;
                                SetActiveTileMap(xx, yy);
                                PlaceTile((uint)(drawRect.X + ws), (uint)(drawRect.Y), false);

                                xx = (drawRect.X + ws)/320;
                                yy = (drawRect.Y + drawRect.Height)/176;
                                SetActiveTileMap(xx, yy);
                                PlaceTile((uint)(drawRect.X + ws), (uint)(drawRect.Y + drawRect.Height), false);
                            }
                            else if(removingRect)
                            {
                                int xx = (drawRect.X + ws)/320;
                                int yy = drawRect.Y/176;
                                SetActiveTileMap(xx, yy);
                                RemoveTile((uint)(drawRect.X + ws), (uint)(drawRect.Y));
                                
                                xx = (drawRect.X + ws)/320;
                                yy = (drawRect.Y + drawRect.Height)/176;
                                SetActiveTileMap(xx, yy);
                                RemoveTile((uint)(drawRect.X + ws), (uint)(drawRect.Y + drawRect.Height));
                            }
                        }

                        for(var h = 0; h < Math.Abs(drawRect.Height)+1; h++)
                        {
                            var sign = Math.Sign(drawRect.Height);
                            var hs = h*sign;

                            if(drawingRect)
                            {
                                int xx = drawRect.X/320;
                                int yy = drawRect.Y + hs/176;
                                SetActiveTileMap(xx, yy);
                                PlaceTile((uint)(drawRect.X),                   (uint)(drawRect.Y + hs), false);
                                
                                xx = (drawRect.X + drawRect.Width)/320;
                                yy = (drawRect.Y + hs)/176;
                                SetActiveTileMap(xx, yy);
                                PlaceTile((uint)(drawRect.X + drawRect.Width),  (uint)(drawRect.Y + hs), false);
                            }
                            else if(removingRect)
                            {
                                int xx = drawRect.X/320;
                                int yy = drawRect.Y + hs/176;
                                SetActiveTileMap(xx, yy);
                                RemoveTile((uint)(drawRect.X),                   (uint)(drawRect.Y + hs));

                                xx = (drawRect.X + drawRect.Width)/320;
                                yy = (drawRect.Y + hs)/176;
                                SetActiveTileMap(xx, yy);
                                RemoveTile((uint)(drawRect.X + drawRect.Width),  (uint)(drawRect.Y + hs));
                            }
                        }
                    }
                    else // fill
                    {
                        for(var w = 0; w < Math.Abs(drawRect.Width)+1; w++)
                        {
                            for(var h = 0; h < Math.Abs(drawRect.Height)+1; h++)
                            {
                                var signw = Math.Sign(drawRect.Width);
                                var ws = w*signw;
                                var signh = Math.Sign(drawRect.Height);
                                var hs = h*signh;

                                int xx = (drawRect.X + ws)/320;
                                int yy = (drawRect.Y + hs)/176;
                                SetActiveTileMap(xx, yy);
                                
                                if(drawingRect)
                                    PlaceTile((uint)(drawRect.X + ws), (uint)(drawRect.Y + hs), false);
                                else if(removingRect)
                                    RemoveTile((uint)(drawRect.X + ws), (uint)(drawRect.Y + hs));
                            }
                        }
                    }
                    activeTileMap!.UpdateAutoTiles();
                }
            }
            drawingRect = false;
            removingRect = false;
        }

        public void Show()
        {
            if (!Visible)
            {
                return;
            }

            DrawInactiveTileGrid();
            activeTileMap!.DrawBounds(Color.SkyBlue);
            if (editing)
            {
                DrawBrush();
            }

            bool showing = false;
            if (ImGui.Begin($"Tile Editor", ref showing))
            {
                ImGui.Text($"room: {activeTileMap.X}, {activeTileMap.Y}");
                ImGui.Checkbox("Edit", ref editing);
                ImGui.SameLine();
                ImGui.Checkbox("Overwrite", ref canOverwrite);

                ImGui.Separator();
                ImGui.Checkbox("Snap", ref snap);
                ImGui.InputInt("Snap X", ref snapSize.x);
                ImGui.InputInt("Snap Y", ref snapSize.y);

                ShowBrushSelect();
                ImGui.Separator();
                ShowTileSelect();
                if(ImGui.Button("save all"))
                {
                    SaveAll();
                }

                // if(ImGui.Button("save active"))
                // {
                //     activeTileMap.Save();
                // }
            }
            ImGui.End();
        }

        private void SaveAll([CallerFilePath] string? filePath = "")
        {
            var t = Time.Elapsed();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("public static string[][][][] mapData = new string[][][][] {");
            
            for(var x = 0; x < 16; x++)
            {
                sb.AppendLine("  new string[][][] {");
                for(var y = 0; y < 16; y++)
                {
                    var tilemap = tilemaps[x, y];
                    sb.AppendLine("    new string[][] {");
                    // LoadFromData expects data[column][row], so we need columns first, then rows
                    for(uint col = 0; col < tilemap.Width; col++)
                    {
                        sb.Append("      new string[] { ");
                        for(int row = 0; row < tilemap.Height; row++)
                        {
                            var tile = tilemap.GetTile(col, (uint)row);
                            if(tile == null)
                            {
                                sb.Append("\" \"");
                            }
                            else
                            {
                                sb.Append($"\"{tile.tileSet.index}\"");
                            }
                            if(row < tilemap.Height - 1)
                                sb.Append(", ");
                        }
                        sb.Append(" }");
                        if(col < tilemap.Width - 1)
                            sb.Append(",");
                        sb.AppendLine();
                    }
                    sb.Append("    }");
                    if(y < 15)
                        sb.Append(",");
                    sb.AppendLine();
                }
                sb.Append("  }");
                if(x < 15)
                    sb.Append(",");
                sb.AppendLine();
            }
            sb.AppendLine("};");

            filePath = Path.GetDirectoryName(Path.GetDirectoryName(filePath));

            string outputPath = Path.Join(filePath, "TileMapData.cs");
            string template = File.ReadAllText(Path.Join(filePath, "TileMapData.cs.in"));

            template = template.Replace("@TILEMAP_DATA@", sb.ToString());
            File.WriteAllText(outputPath, template);
            var et = Time.Elapsed();
            Console.WriteLine($"Saved: {et - t}s");
        }

        private void PlaceTile(uint x, uint y, bool autoUpdate = true)
        {
            x = (uint)(x/snapSize.x) % 40;
            y = (uint)(y/snapSize.y) % 22;
            var tile = activeTileMap!.GetTile((uint)(x),(uint)(y));

            if(tile == null)
            {
                activeTileMap.AddTile(x,y,tileSet);
            }
            else if(canOverwrite)
            {
                tile.SetTileSet(tileSet);
            }
            if(autoUpdate)
            {
                activeTileMap.UpdateAutoTiles();
            }
        }
        private void RemoveTile(uint x, uint y)
        {
            x = (uint)(x/snapSize.x) % 40;
            y = (uint)(y/snapSize.y) % 22;
            activeTileMap!.RemoveTile(x, y);
        }

        private void ShowBrushSelect()
        {
            ImGui.Text("Brush type");

            bool temp = brushType == BrushType.BRUSH;
            if (ImGui.Checkbox("brush", ref temp))
            {
                brushType = BrushType.BRUSH;
            }
            ImGui.SameLine();
            temp = brushType == BrushType.RECT;
            if (ImGui.Checkbox("rect", ref temp))
            {
                brushType = BrushType.RECT;
            }
            ImGui.SameLine();
            temp = brushType == BrushType.LINE;
            if (ImGui.Checkbox("line", ref temp))
            {
                brushType = BrushType.LINE;
            }

            if(brushType == BrushType.RECT)
            {
                ImGui.Checkbox("fill", ref fill);
            }

        }

        private void ShowTileSelect()
        {
            var tsetType = typeof(TileSets);
            FieldInfo[] fields = tsetType.GetFields(BindingFlags.Static | BindingFlags.Public);
            List<FieldInfo> staticVariables = fields.Where(field => field.FieldType == typeof(TileSet)).ToList();
            if (ImGui.BeginCombo("Tile Set", staticVariables[selectedTileType].Name))
            {
                bool b = false;
                var i = 0;
                foreach(var field in staticVariables)
                {
                    if(field.FieldType == typeof(TileSet))
                    {
                        if (ImGui.Selectable(field.Name, ref b))
                        {
                            selectedTileType = i;
                            tileSet = (TileSet)field.GetValue(null);
                        }
                    }
                    i++;
                }
                ImGui.EndCombo();
            }
        }

        private void DrawBrush()
        {
            brushPos = (ivec2)Input.MousePosition;
            if (snap)
            {
                brushPos.x = (int)MathF.Floor(brushPos.x / snapSize.x) * snapSize.x;
                brushPos.y = (int)MathF.Ceiling(brushPos.y / snapSize.y) * snapSize.y;

                if(Input.MousePosition.y < 0)
                    brushPos.y-=8;
                if(Input.MousePosition.x < 0)
                    brushPos.x-=8;
            }

            canPlace = false;

            int xx = brushPos.x/320;
            int yy = brushPos.y/176;
            SetActiveTileMap(xx, yy);

            canPlace = canPlace && hoverTimer <= 0;
            Draw.Rect(new Rect(brushPos.x-1,brushPos.y, 9,9), !canPlace ? Color.Red : Color.SkyBlue);

            if(!ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow) && !ImGui.IsAnyItemFocused())
            {
                hoverTimer--;
            }
            else
            {
                hoverTimer = 6;
            }
        }

        private void DrawInactiveTileGrid()
        {
            for(var x = 0; x < 16; x++)
            {
                for(var y = 0; y < 16; y++)
                {
                    if(tilemaps[x,y].InView())
                    {
                        tilemaps[x,y].DrawBounds(Color.DarkBlue);
                    }
                }   
            }
        }

        private void SetActiveTileMap(int x, int y)
        {
            if(x >= 0 && x < 16 && y >= 0 && y < 16)
            {
                activeTileMap = tilemaps[x,y];
                canPlace = true;
            }
        }
    }
}