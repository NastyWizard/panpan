
namespace panpan.Scene
{
    public abstract class Component
    {
        public Entity parent;
        public virtual void Init() { }
        public virtual void Update() { }
        public virtual void Render(nint renderPass) { }

    }
}