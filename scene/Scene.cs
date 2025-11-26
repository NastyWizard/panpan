
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

        public float TimeScale = 1.0f;

        public Scene(string name)
        {
            this.name = name;
            entities = new List<Entity>();
            camera = new Camera(0,0,800, 600);
        }

        public virtual void Init()
        {
            camera.Init();
            camera.Scene = this;
            
            foreach (Entity ent in entities)
            {
                ent.Init();
            }
        }

        public virtual void Update()
        {
            camera.Update();
            foreach (Entity ent in entities)
            {
                ent.Update();
            }
        }
        public virtual void Render()
        {
            camera.PushUniformData();
            ColliderDebugBatch.BeginFrame();
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

        public bool RemoveChild(Entity entity)
        {
            if (entity == null)
                return false;
            
            bool removed = entities.Remove(entity);
            
            if (removed)
            {
                entity.Scene = null;
                return true;
            }
            return false;
        }

    }
}