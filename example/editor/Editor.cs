
using panpan;
using panpan.Rendering;

namespace panpanExample
{
    public class Editor
    {

        #region Tile
        bool tileMenuVisible = false;
        TileEditor tileEditor = new TileEditor();
        #endregion Tile 

        #region Debugger
        bool debuggerVisible = false;
        #endregion

        public void ShowEditor()
        {
            ShowMenuBar();
            ShowBasicDebugInfo();
            ShowTileEditor();
        }

        public void ShowMenuBar()
        {
            ImGui.BeginMainMenuBar();
            if (ImGui.BeginMenu("window"))
            {
                ImGui.Checkbox("Tile Editor", ref tileMenuVisible);
                ImGui.Checkbox("Debug", ref debuggerVisible);
                ImGui.EndMenu();
            }
            ImGui.Text($"FPS: {App.GetFPS():F2}");
            ImGui.EndMainMenuBar();
        }

        public void ShowTileEditor()
        {
            if (tileMenuVisible)
            {
                tileEditor.Show();
            }
        }

        public void ShowBasicDebugInfo()
        {
            if (!debuggerVisible)
            {
                return;
            }

            bool showing = false;
            bool visible = ImGui.Begin("Scene Debug", ref showing);

            if (visible)
            {
                ImGui.Text($"FPS: {App.GetFPS():F2}");
                ImGui.Text($"Mouse Pos: {Input.MousePosition.x:F2}, {Input.MousePosition.y:F2}");
            }

            ImGui.Checkbox("Show colliders", ref App.GetCollisionManager().ShowColliderDebug);

            ImGui.End();
        }
    }
}