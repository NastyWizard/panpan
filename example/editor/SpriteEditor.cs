
using ImGuiNET.Backend.SDLGPU;
using panpan;
using panpan.Rendering;
using panpan.Scene;

namespace panpanExample
{
    public class SpriteEditor
    {
        Entity? currentSelectedObject = null;
        public bool Visible = false;
        public void Show()
        {
            if (!Visible)
            {
                return;
            }

            bool showing = false;
            if (ImGui.Begin("Sprite Editor", ref showing))
            {
                ShowObjectSelect();
                ImGui.Separator();
            }
            ImGui.End();
        }

        private void ShowObjectSelect()
        {
            if (currentSelectedObject == null)
            {
                currentSelectedObject = App.GetSceneManager().ActiveScene.Children[0];
            }

            string label = currentSelectedObject.GetType().Name;

            if (ImGui.BeginCombo("object", label))
            {
                bool b = false;
                var i = 0;
                foreach (var child in App.GetSceneManager().ActiveScene.Children)
                {
                    Type t = child.GetType();
                    if (ImGui.Selectable(t.Name + $"##{i++}", ref b))
                    {
                        currentSelectedObject = child;
                    }
                }
                ImGui.EndCombo();
            }
        }

        private void ShowSpriteClipper()
        {
            
        }
    }
}