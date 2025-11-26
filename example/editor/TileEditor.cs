
using System.Reflection;
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

        public bool Visible = false;

        public void Init()
        {
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
                        App.GetSceneManager().ActiveScene.TileMap.UpdateAutoTiles();
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
                                PlaceTile((uint)(drawRect.X + ws), (uint)(drawRect.Y), false);
                                PlaceTile((uint)(drawRect.X + ws), (uint)(drawRect.Y + drawRect.Height), false);
                            }
                            else if(removingRect)
                            {
                                RemoveTile((uint)(drawRect.X + ws), (uint)(drawRect.Y));
                                RemoveTile((uint)(drawRect.X + ws), (uint)(drawRect.Y + drawRect.Height));
                            }
                        }

                        for(var h = 0; h < Math.Abs(drawRect.Height)+1; h++)
                        {
                            var sign = Math.Sign(drawRect.Height);
                            var hs = h*sign;

                            if(drawingRect)
                            {
                                PlaceTile((uint)(drawRect.X),                   (uint)(drawRect.Y + hs), false);
                                PlaceTile((uint)(drawRect.X + drawRect.Width),  (uint)(drawRect.Y + hs), false);
                            }
                            else if(removingRect)
                            {
                                RemoveTile((uint)(drawRect.X),                   (uint)(drawRect.Y + hs));
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

                                if(drawingRect)
                                    PlaceTile((uint)(drawRect.X + ws), (uint)(drawRect.Y + hs), false);
                                else if(removingRect)
                                    RemoveTile((uint)(drawRect.X + ws), (uint)(drawRect.Y + hs));
                            }
                        }
                    }
                    App.GetSceneManager().ActiveScene.TileMap.UpdateAutoTiles();
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

            if (editing)
            {
                App.GetSceneManager().ActiveScene.TileMap.DrawBounds();
                DrawBrush();
            }

            bool showing = false;
            if (ImGui.Begin("Tile Editor", ref showing))
            {
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
            }
            ImGui.End();
        }

        private void PlaceTile(uint x, uint y, bool autoUpdate = true)
        {
            var tile = App.GetSceneManager().ActiveScene.TileMap.GetTile((uint)(x/snapSize.x),(uint)(y/snapSize.y));

            if(tile == null)
            {
                App.GetSceneManager().ActiveScene.TileMap.AddTile(x/8,y/8,tileSet);
            }
            else if(canOverwrite)
            {
                tile.SetTileSet(tileSet);
            }
            if(autoUpdate)
            {
                App.GetSceneManager().ActiveScene.TileMap.UpdateAutoTiles();
            }
        }
        private void RemoveTile(uint x, uint y)
        {
            App.GetSceneManager().ActiveScene.TileMap.RemoveTile(x/8, y/8);
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


            var tmap = App.GetSceneManager().ActiveScene.TileMap;
            canPlace = !(brushPos.x < 0 || brushPos.y < 0 || brushPos.x >= tmap.Width*tmap.TileSize || brushPos.y >= tmap.Height*tmap.TileSize);
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
    }
}