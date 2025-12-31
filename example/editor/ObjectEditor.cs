
using System.Reflection;
using GlmSharp;
using ImGuiNET.Backend.SDLGPU;
using panpan;
using panpan.Assets;
using panpan.Rendering;
using panpan.Scene;
using panpan.Util;

namespace panpanExample
{
    public class ObjectEditor
    {
        enum BrushType
        {
            BRUSH,
            RECT,
            LINE
        }

        private bool editing = false;
        private bool snap = true;
        private ivec2 snapSize = new ivec2(8, 8);

        private BrushType brushType = BrushType.BRUSH;
        private Texture? brushTex;
        private ivec2 brushPos;
        private Entity brushEntity;
        private Type brushObjectType;
        private vec2 brushOffset = vec2.Zero;
        private Rect? brushClipRect;

        private IEnumerable<Type> availableObjectTypes;

        public bool Visible = false;


        public void Init()
        {
            ChangeBrush(typeof(Capybara), 0, 0);
            Input.RegisterOnMouseDown(OnMouseDown);
            availableObjectTypes = Utility.GetAllSubTypes(typeof(GameObject));
        }

        private void OnMouseDown(byte btn)
        {
            if (btn == 1 && editing && !ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow) && !App.GetCollisionManager().IntersectsPosition(Input.MousePosition, typeof(Tile)))
            {
                PlaceObject(brushPos.x - (int)brushOffset.x, brushPos.y - (int)brushOffset.y);
            }
        }

        public void Show()
        {
            if (!Visible)
            {
                return;
            }

            if (editing)
            {
                DrawBrush();
            }

            bool showing = false;
            if (ImGui.Begin("Object Editor", ref showing))
            {
                ImGui.Checkbox("Edit", ref editing);

                ImGui.Separator();
                ImGui.Checkbox("Snap", ref snap);
                ImGui.InputInt("Snap X", ref snapSize.x);
                ImGui.InputInt("Snap Y", ref snapSize.y);
                
                ShowObjectSelect();
                ShowBrushSelect();
                ImGui.Separator();
                ShowTileSelect();
            }
            ImGui.End();
        }

        private void PlaceObject(int x, int y)
        {

            var paramArray = brushObjectType.GetConstructors().Single().GetParameters();
            object?[] objectParams = new object[paramArray.Length];

            var k = 0;
            foreach (var param in paramArray)
            {
                objectParams[k] = Utility.CreateDefaultFromType(paramArray[k].ParameterType);
                k++;
            }
            objectParams[0] = x;
            objectParams[1] = y;

            Entity? obj = (Entity)Activator.CreateInstance(brushObjectType, objectParams);
            if (obj != null)
            {
                var tile = App.GetSceneManager().ActiveScene.AddChild(obj);
                tile.Init();
            }
        }


        private void ShowObjectSelect()
        {
            if (brushObjectType == null)
            {
                brushObjectType = availableObjectTypes.ElementAt(0);
            }

            string label = brushObjectType.Name;

            if (ImGui.BeginCombo("object", label))
            {
                var i = 0;
                foreach (var type in availableObjectTypes)
                {
                    bool isSelected = ReferenceEquals(type, brushObjectType);
                    if (ImGui.Selectable(type.Name + $" [{i++}]", ref isSelected))
                    {
                        if (!ReferenceEquals(brushObjectType, type))
                        {
                            var paramArray = type.GetConstructors().Single().GetParameters();
                            object?[] objectParams = new object[paramArray.Length];

                            var k = 0;
                            foreach (var param in paramArray)
                            {
                                objectParams[k] = Utility.CreateDefaultFromType(paramArray[k].ParameterType);
                                k++;
                            }
                            
                            ChangeBrush(type, objectParams);
                        }
                    }
                }
                ImGui.EndCombo();
            }
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
        }

        private void ShowTileSelect()
        {
            if (ImGui.BeginCombo("test", "test"))
            {
                bool b = false;
                if (ImGui.Selectable("test2", ref b)) { /* handle selection */ }
                ImGui.EndCombo();
            }
        }

        private void DrawBrush()
        {
            brushPos = (ivec2)Input.MousePosition;
            if (snap)
            {
                brushPos.x = (int)MathF.Floor(brushPos.x / snapSize.x) * snapSize.x + (int)brushOffset.x;
                brushPos.y = (int)MathF.Ceiling(brushPos.y / snapSize.y) * snapSize.y + (int)brushOffset.y;
            }
            vec2 scale = new vec2(brushEntity.Transform.Scale.x, brushEntity.Transform.Scale.y);
            Draw.Sprite(brushTex, brushPos, scale, brushClipRect);
        }

        private void ChangeBrush(Type type, params object?[]? args)
        {
            brushObjectType = type;
            
            brushEntity = (Entity)Activator.CreateInstance(type, args);
            brushEntity!.Init();

            var renderer = brushEntity.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                brushTex = Debug.cursorTex;
                brushClipRect = new Rect(0, 0, 7, 7);
                brushOffset = new vec2(3, 3);
            }
            else
            {
                brushTex = renderer!.Texture;
                brushClipRect = renderer.ClipRect;
                vec2 scale = new vec2(brushEntity.Transform.Scale.x, brushEntity.Transform.Scale.y);
                brushOffset = -renderer!.Origin * new vec2(brushClipRect?.Width ?? (float)brushTex!.Width, brushClipRect?.Height ?? (float)brushTex!.Height) * scale;   
            }
        }
    }
}