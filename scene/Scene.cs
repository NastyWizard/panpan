
using panpan.Rendering;
using panpan.Collision;
using panpanExample;

namespace panpan.Scene
{
    public abstract class Scene
    {
        private List<Entity> entities;

        private Camera camera;
        public Camera Camera => camera;

        private string name;
        public string Name => name;

        public List<Entity> Children => entities;
        private TileMap tileMap;
        public TileMap TileMap => tileMap;

        public float TimeScale = 1.0f;

        public Scene(string name)
        {
            this.name = name;
            entities = new List<Entity>();
            camera = new Camera(0,0,800, 600);
            tileMap = new TileMap(0,0,320,180);
        }

        public virtual void Init()
        {
            camera.Init();
            tileMap.Init();
            foreach (Entity ent in entities)
            {
                ent.Init();
            }
        }

        public virtual void Update()
        {
            camera.Update();
            tileMap.Update();
            foreach (Entity ent in entities)
            {
                ent.Update();
            }
        }
        public virtual void Render()
        {
            camera.PushUniformData();
            ColliderDebugBatch.BeginFrame();
            tileMap.Render();
            foreach (Entity ent in entities)
            {
                ent.Render();
            }
            camera.Render();
            ColliderDebugBatch.Flush();
        }

        public Entity AddChild(Entity entity)
        {
            entities.Add(entity);
            entity.Scene = this;
            return entity;
        }

    }
}