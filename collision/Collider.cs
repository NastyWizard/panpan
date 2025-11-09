
using GlmSharp;
using panpan.Rendering;
using panpan.Scene;

namespace panpan.Collision
{
    public delegate void CollisionDelegate(Collider other);
    public abstract class Collider: Component
    {
        protected vec4 debugColor = Color.Green;
        public CollisionDelegate OnCollisionEnter = null!;
        public CollisionDelegate OnCollisionExit  = null!;
        public CollisionDelegate OnCollisionUpdate  = null!;

        public Collider() { }

        public override void Init()
        {
            UpdateBounds();
            App.GetCollisionManager().AddCollider(this);
            base.Init();
        }

        public override void Update()
        {
            UpdateBounds();
            base.Update();
        }

        public void RegisterOnCollisionEnter(CollisionDelegate func) { OnCollisionEnter = func; }
        public void RegisterOnCollisionExit(CollisionDelegate func) { OnCollisionExit = func; }
        public void RegisterOnCollisionUpdate(CollisionDelegate func) { OnCollisionUpdate = func; }
        public virtual void DrawDebug() { }
        public virtual vec2 CenterPoint() { throw new Exception("Unimplemented CenterPoint"); }
        public virtual bool Intersects(Collider other, vec2? pos = null) { throw new Exception("Unimplemented Intersects"); }
        public virtual void UpdateBounds() { throw new Exception("Unimplemented UpdateBounds"); }
    }
}