
using GlmSharp;
using ImGuiNET.Backend.SDLGPU;
using panpan;
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

        public bool Visible = false;

        public void Init()
        {
            brushTex = new Texture(panpan.Assets.Sprites.capybara, 22, 18);
            brushTex.CopyPass();
            Input.RegisterOnMouseDown(OnMouseDown);
        }

        private void OnMouseDown(byte btn)
        {
            if (btn == 1 && editing && !ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow) && !App.GetCollisionManager().IntersectsPosition(Input.MousePosition, typeof(TestWall)))
            {
                PlaceTile(brushPos.x, brushPos.y);
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

                ShowBrushSelect();
                ImGui.Separator();
                ShowTileSelect();
            }
            ImGui.End();
        }

        private void PlaceTile(int x, int y)
        {
            var tile = App.GetSceneManager().ActiveScene.AddChild(new Capybara(x, y));
            tile.Init();
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
                brushPos.x = (int)MathF.Floor(brushPos.x / snapSize.x) * snapSize.x;
                brushPos.y = (int)MathF.Ceiling(brushPos.y / snapSize.y) * snapSize.y + 8;
            }
            Draw.Sprite(brushTex, brushPos);
            Draw.Rect(new Rect(brushPos.x-1,brushPos.y-8, snapSize.x+1,snapSize.y+1), Color.SkyBlue);
        }
    }
}