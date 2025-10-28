
using GlmSharp;
using panpan.Rendering;
using panpan.Scene;

namespace panpan.Collision
{
    public abstract class Collider: Component
    {
        protected vec4 debugColor = Color.Green;
        public Collider() { }
        public virtual bool CollidesWith(Collider other) { return false; }
        public virtual void DrawDebug() {}
    }
}