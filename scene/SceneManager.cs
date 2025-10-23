
namespace panpan.Scene
{
    public class SceneManager
    {
        private Scene activeScene;
        public Scene ActiveScene => activeScene;
        
        public SceneManager(Scene startScene)
        {
            SwapScene(startScene);
        }

        public void SwapScene(Scene scene)
        {
            this.activeScene = scene;
            scene.Init();
        }

    }
}