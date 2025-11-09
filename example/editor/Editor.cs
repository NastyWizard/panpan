
using panpan;
using panpan.Rendering;

namespace panpanExample
{
    public class Editor
    {
        public void ShowEditor()
        {
            ShowBasicDebugInfo();
            ShowTileEditor();
        }

        public void ShowTileEditor()
        {

        }

        public void ShowBasicDebugInfo()
        {
            bool showing = false;
            bool visible = ImGui.Begin("Scene Debug", ref showing);

            if (visible)
            {
                ImGui.Text($"FPS: {App.GetFPS():F2}");
                ImGui.Text($"Mouse Pos: {Input.MousePosition.x:F2}, {Input.MousePosition.y:F2}");
            }

            ImGui.End();
        }
    }
}