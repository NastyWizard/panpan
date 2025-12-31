
using panpan;
using panpan.Rendering;
using panpan.Util;
using SDL3;

namespace panpanExample
{
    public class Editor
    {

        TileEditor tileEditor = new TileEditor();
        SpriteEditor spriteEditor = new SpriteEditor();
        ObjectEditor objectEditor = new ObjectEditor();
        private bool visible = true;

        private int selectedPaletteIndex = 2;

        #region Debugger
        bool debuggerVisible = false;
        #endregion Debugger

        public void Init()
        {
            tileEditor.Init();
            objectEditor.Init();

            Input.RegisterOnKeyDown(OnKeyDown);
        }

        public void ShowEditor()
        {
            if(visible)
            {
                ShowMenuBar();
                ShowBasicDebugInfo();
                tileEditor.Show();
                spriteEditor.Show();
                objectEditor.Show();
            }
        }

        private void OnKeyDown(SDL.Keycode? key)
        {
            if(key == SDL.Keycode.Grave)
            {
                visible = !visible;
            }
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
                if(ImGui.Button("Reset Player"))
                {
                    ((GameScene)App.GetSceneManager().ActiveScene).player.Position.xy = new GlmSharp.vec2(64 + 320*6, 33 + 176*6);
                }
                ImGui.SliderFloat("Time Scale", ref App.GetSceneManager().ActiveScene.TimeScale, 0.1f, 2.0f);
                if(ImGui.Button("Reset Time Scale"))
                {
                    App.GetSceneManager().ActiveScene.TimeScale = 1.0f;
                }
                if(ImGui.SliderFloat("Zoom", ref App.GetSceneManager().ActiveScene.Camera.Zoom, 0.5f, 8.0f))
                {
                    App.GetSceneManager().ActiveScene.Camera.UpdateProjection();
                }
                if(ImGui.Button("Reset Zoom"))
                {
                    App.GetSceneManager().ActiveScene.Camera.Zoom = 1.0f;
                    App.GetSceneManager().ActiveScene.Camera.UpdateProjection();
                }
                ImGui.Checkbox("Show colliders", ref App.GetCollisionManager().ShowColliderDebug);
                ImGui.Checkbox("Show invisible objects", ref panpan.Util.Debug.showObjectsWithoutRenderer);
                ImGui.Checkbox("Free camera", ref ((GameScene)App.GetSceneManager().ActiveScene).FreeCamera);
                ImGui.Checkbox("Debug lights", ref ((GameScene)App.GetSceneManager().ActiveScene).DebugLights);

                Texture[] paletteOptions = [GameTextures.palette_1,GameTextures.palette_2,GameTextures.palette_3,GameTextures.palette_4];
                if (ImGui.BeginCombo("Palette", $"{selectedPaletteIndex}"))
                {
                    for(var i = 0; i < paletteOptions.Length; i++)
                    {
                        bool isSelected = false;
                        if (ImGui.Selectable($"{i}", ref isSelected))
                        {
                            selectedPaletteIndex = i;
                            ((GameScene)App.GetSceneManager().ActiveScene).ActivePalette = paletteOptions[i];
                        }
                    }
                    ImGui.EndCombo();
                }
            }


            ImGui.End();
        }
    }
}