
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

        bool editing = false;
        BrushType brushType = BrushType.BRUSH;

        public bool Visible = false;

        public void Show()
        {
            if (!Visible)
            {
                return;
            }

            bool showing = false;
            if (ImGui.Begin("Tile Editor", ref showing))
            {
                ImGui.Checkbox("Edit", ref editing);
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
    }
}