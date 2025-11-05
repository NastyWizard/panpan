
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
            this.bounds = new Rect(0, 0, width, height);
        }

        public override void Update()
        {
            bounds.X = (int)Parent.Position.x + offset.x;
            bounds.Y = (int)Parent.Position.y + offset.y;
            base.Update();
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
                    bnds.X = (int)pos.Value.x + offset.x;
                    bnds.Y = (int)pos.Value.y + offset.y;
                }
                
                if (bnds.Intersects(otherBox.bounds))
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
            Draw.Dot(new vec2(bounds.X, bounds.Y), Rendering.Color.SkyBlue);
        }

        public void SetOffset(int x, int y)
        {
            offset.x = x;
            offset.y = y;
        }
        public override vec2 CenterPoint()
        {
            return new vec2(bounds.X + bounds.Width/2, bounds.Y + bounds.Height/2);
        }
    }
}
