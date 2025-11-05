
using GlmSharp;
using panpan.Rendering;
using SDL3;

namespace panpan.Scene
{
    public abstract class Entity
    {
        public Scene Scene;

        private List<Component> components = new List<Component>();

        public Transform Transform = new Transform();
        public ref vec3 Position => ref Transform.Position;
        public ref vec3 Scale => ref Transform.Scale;
        public ref float Angle => ref Transform.Angle;
        
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
        public virtual void Render()
        {
            foreach (Component comp in components)
            {
                comp.Render();
            }
        }

        public Component AddComponent(in Component comp)
        {
            components.Add(comp);
            comp.Parent = this;
            return comp;
        }

    }
}