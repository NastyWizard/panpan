
using GlmSharp;
using ImGuiNET.Backend.SDLGPU;
using panpan;
using panpan.Rendering;

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
        private ivec2 snapSize = new ivec2(8, 8);

        private BrushType brushType = BrushType.BRUSH;
        private Texture? brushTex;
        private ivec2 brushPos;

        public bool Visible = false;

        public void Init()
        {
            brushTex = new Texture(panpan.Assets.Sprites.tile, 8, 8);
            brushTex.CopyPass();
            Input.RegisterOnMouseDown(OnMouseDown);
        }

        private void OnMouseDown(byte btn)
        {
            if (btn == 1 && editing && !ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow))
            {
                var tile = App.GetSceneManager().ActiveScene.AddChild(new TestWall(brushPos.x, brushPos.y));
                tile.Init();
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
            if (ImGui.Begin("Tile Editor", ref showing))
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
        }
    }
}