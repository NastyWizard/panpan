
namespace panpan.Scene
{
    public abstract class Component
    {
        public Entity Parent { get; internal set; } = null!;
        public virtual void Init() { }
        public virtual void Destroy() { }
        public virtual void Update() { }
        public virtual void Render() { }

    }
}