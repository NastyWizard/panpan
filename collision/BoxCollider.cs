
using System.Drawing;
using GlmSharp;
using panpan.Rendering;

namespace panpan.Collision
{
    public class BoxCollider : Collider
    {
        public Rectangle bounds;
        private ivec2 offset = ivec2.Zero;
        public BoxCollider(int width, int height)
        {
            this.bounds = new Rectangle(0, 0, width, height);
        }

        public override void Update()
        {
            bounds.X = (int)Parent.Position.x + offset.x;
            bounds.Y = (int)Parent.Position.y + offset.y;
            base.Update();
        }

        public override bool CollidesWith(Collider other)
        {
            var colType = other.GetType();
            if (colType == typeof(BoxCollider))
            {
                var otherBox = (BoxCollider)other;
                if (!Rectangle.Intersect(bounds, otherBox.bounds).IsEmpty)
                {
                    return true;
                }
            }
            return false;
        }

        public override void DrawDebug()
        {
            base.DrawDebug();
            Draw.Rect(bounds, debugColor);
        }

        public void SetOffset(int x, int y)
        {
            offset.x = x;
            offset.y = y;
        }
    }
}
