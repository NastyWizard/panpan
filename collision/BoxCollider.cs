
using System.Drawing;
using GlmSharp;
using panpan.Rendering;
using panpan.Util;

namespace panpan.Collision
{
    public class BoxCollider : Collider
    {
        public Rect bounds;
        private ivec2 offset = ivec2.Zero;
        public BoxCollider(int width, int height)
        {
            SetBounds(new Rect(0, 0, width, height));
        }

        public override void Render()
        {
            base.Render();
            if (App.GetCollisionManager().ShowColliderDebug)
            {
                DrawDebug();
            }
        }

        public override void UpdateBounds()
        {
            // Use Floor(x + 0.5) for consistent rounding away from zero (matches MeshRenderer)
            bounds.X = (int)MathF.Floor(Parent.Position.x + offset.x + 0.5f);
            bounds.Y = (int)MathF.Floor(Parent.Position.y + offset.y + 0.5f);
        }

        public override bool Intersects(Collider other, vec2? pos = null)
        {
            var colType = other.GetType();
            if (colType == typeof(BoxCollider))
            {
                var otherBox = (BoxCollider)other;

                var bnds = bounds;

                if (pos != null)
                {
                    // Use Floor(x + 0.5) for consistent rounding away from zero (matches MeshRenderer)
                    bnds.X = (int)MathF.Floor(pos.Value.x + offset.x + 0.5f);
                    bnds.Y = (int)MathF.Floor(pos.Value.y + offset.y + 0.5f);
                }

                if (bnds.Intersects(otherBox.bounds))
                {
                    return true;
                }
            }
            return false;
        }
        public override bool IntersectsPosition(vec2 pos) 
        {
            return bounds.IntersectsPosition(pos);
        }

        public override void DrawDebug()
        {
            base.DrawDebug();
            ColliderDebugBatch.SubmitRect(bounds, debugColor);
            ColliderDebugBatch.SubmitPixel(new vec2(bounds.X, bounds.Y), Rendering.Color.Red);
        }

        public void SetOffset(int x, int y)
        {
            offset.x = x;
            offset.y = y;
        }
        public void SetBounds(Rect bounds)
        {
            this.bounds = bounds;
            this.bounds.Width -= 1;
            this.bounds.Height -= 1;
        }

        public override vec2 CenterPoint()
        {
            return new vec2(bounds.X + bounds.Width/2, bounds.Y + bounds.Height/2);
        }
    }
}
