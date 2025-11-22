
namespace panpan.Scene
{
    public class SceneManager
    {
        private Scene activeScene = null!;
        public Scene ActiveScene => activeScene;
        
        public SceneManager()
        {
        }

        public void SwapScene(Scene scene)
        {
            this.activeScene = scene;
            scene.Init();
        }

    }
}