
using panpan.Rendering;

namespace panpan.Scene
{
    public abstract class Scene
    {
        private List<Entity> entities;
        private List<Entity> markedChildrenForRemoval;
        private List<Entity> markedChildrenForAddition;

        private Camera camera;
        public Camera Camera => camera;

        public Camera ActiveCamera;

        private string name;
        public string Name => name;

        public List<Entity> Children => entities;

        public float TimeScale = 1.0f;

        public Scene(string name)
        {
            this.name = name;
            entities = new List<Entity>();
            markedChildrenForRemoval = new List<Entity>();
            markedChildrenForAddition = new List<Entity>();
            camera = new Camera(0,0,800, 600);
            ActiveCamera = camera;
        }

        public virtual void Init()
        {
            camera.Init();
            camera.Scene = this;
            

            foreach (Entity ent in markedChildrenForAddition)
            {
                entities.Add(ent);
                ent.OnAdd();
            }
            markedChildrenForAddition.Clear();

            foreach (Entity ent in entities)
            {
                ent.Init();
            }
        }

        public void RestartScene()
        {
            entities.Clear();
            Init();
        }

        public virtual void FixedUpdate()
        {
            camera.FixedUpdate();
            foreach (Entity ent in entities)
            {
                ent.FixedUpdate();
            }
        }

        public virtual void Update()
        {   
            camera.Update();
            foreach (Entity ent in entities)
            {
                ent.Update();
            }
            
            foreach (Entity ent in markedChildrenForRemoval)
            {
                entities.Remove(ent);
                ent.OnRemove();
                ent.Scene = null;
            }
            markedChildrenForRemoval.Clear();

            foreach (Entity ent in markedChildrenForAddition)
            {
                entities.Add(ent);
            }
            markedChildrenForAddition.Clear();
        }

        public virtual void Render()
        {
            ActiveCamera.PushUniformData();
            DrawBatch.BeginFrame();
            foreach (Entity ent in entities)
            {
                ent.Render();
            }
            ActiveCamera.Render();
            DrawBatch.Flush();
        }

        public virtual void OnFSToggle(){}

        public virtual Entity AddChild(Entity entity)
        {
            markedChildrenForAddition.Add(entity);
            entity.Scene = this;
            return entity;
        }

        public virtual bool RemoveChild(Entity entity)
        {
            if (entity == null)
                return false;
            
            markedChildrenForRemoval.Add(entity);
            return true;
        }

    }
}