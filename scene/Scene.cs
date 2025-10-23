
using panpan.Rendering;

namespace panpan.Scene
{
    public abstract class Scene
    {
        private List<Entity> entities;

        private Camera camera;
        public Camera Camera => camera;

        private string name;
        public string Name => name;

        public Scene(string name)
        {
            this.name = name;
            entities = new List<Entity>();
            camera = new Camera(800, 600);
        }

        public virtual void Init()
        {
            foreach (Entity ent in entities)
            {
                ent.Init();
            }
        }

        public virtual void Update()
        {
            foreach (Entity ent in entities)
            {
                ent.Update();
            }
        }
        public virtual void Render(nint renderPass)
        {
            //camera.PushUniformData();
            foreach (Entity ent in entities)
            {
                ent.Render(renderPass);
            }
        }

        public Entity AddChild(Entity entity)
        {
            entities.Add(entity);
            entity.Scene = this;
            return entity;
        }

    }
}