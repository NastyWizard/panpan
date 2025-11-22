
using panpan;
using panpan.Rendering;
using panpan.Util;

namespace panpanExample
{
    public class Editor
    {

        TileEditor tileEditor = new TileEditor();
        SpriteEditor spriteEditor = new SpriteEditor();
        ObjectEditor objectEditor = new ObjectEditor();

        #region Debugger
        bool debuggerVisible = false;
        #endregion Debugger

        public void Init()
        {
            tileEditor.Init();
            objectEditor.Init();
        }

        public void ShowEditor()
        {
            ShowMenuBar();
            ShowBasicDebugInfo();
            tileEditor.Show();
            spriteEditor.Show();
            objectEditor.Show();
        }

        public void ShowMenuBar()
        {
            ImGui.BeginMainMenuBar();
            if (ImGui.BeginMenu("window"))
            {
                ImGui.Checkbox("Tile Editor", ref tileEditor.Visible);
                ImGui.Checkbox("Sprite Editor", ref spriteEditor.Visible);
                ImGui.Checkbox("Object Editor", ref objectEditor.Visible);
                ImGui.Checkbox("Debug", ref debuggerVisible);
                ImGui.EndMenu();
            }
            ImGui.Text($"FPS: {App.GetFPS():F2}");
            ImGui.EndMainMenuBar();
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
                ImGui.SliderFloat("Time Scale", ref App.GetSceneManager().ActiveScene.TimeScale, 0.1f, 2.0f);
                if(ImGui.Button("Reset Time Scale"))
                {
                    App.GetSceneManager().ActiveScene.TimeScale = 1.0f;
                }
                ImGui.Checkbox("Show colliders", ref App.GetCollisionManager().ShowColliderDebug);
                ImGui.Checkbox("Show invisible objects", ref panpan.Util.Debug.showObjectsWithoutRenderer);
                ImGui.Checkbox("Free camera", ref ((TestScene)App.GetSceneManager().ActiveScene).FreeCamera);
            }


            ImGui.End();
        }
    }
}