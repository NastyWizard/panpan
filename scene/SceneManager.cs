
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
            Log.Write("Swapping scenes");
            this.activeScene = scene;
            scene.Init();
            Log.Write("Scene swap complete");
        }

    }
}