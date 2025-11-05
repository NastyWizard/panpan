
using GlmSharp;
using panpan.Rendering;
using panpan.Scene;

namespace panpan.Collision
{
    public delegate void CollisionDelegate(Collider other);
    public abstract class Collider: Component
    {
        protected vec4 debugColor = Color.Green;
        public CollisionDelegate OnCollisionEnter;
        public CollisionDelegate OnCollisionExit;
        public CollisionDelegate OnCollisionUpdate;
        
        public Collider() { }
        public void RegisterOnCollisionEnter(CollisionDelegate func) { OnCollisionEnter = func; }
        public void RegisterOnCollisionExit(CollisionDelegate func) { OnCollisionExit = func; }
        public void RegisterOnCollisionUpdate(CollisionDelegate func) { OnCollisionUpdate = func; }
        public virtual void DrawDebug() { }
        public virtual vec2 CenterPoint() { throw new Exception("Unimplemented CenterPoint"); }
        public virtual bool Intersects(Collider other) { throw new Exception("Unimplemented Intersects"); }
    }
}