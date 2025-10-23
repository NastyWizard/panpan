
using GlmSharp;

namespace panpan.Scene
{
    public abstract class Entity
    {
        public Scene Scene;

        private List<Component> components = new List<Component>();

        public vec3 Position;
        
        public virtual void Init()
        {
            foreach (Component comp in components)
            {
                comp.Init();
            }
        }
        public virtual void Update()
        {
            foreach (Component comp in components)
            {
                comp.Update();
            }
        }
        public virtual void Render(nint renderPass)
        {
            foreach (Component comp in components)
            {
                comp.Render(renderPass);
            }
        }

        public Component AddComponent(in Component comp)
        {
            components.Add(comp);
            return comp;
        }

    }
}