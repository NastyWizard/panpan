
using panpan.Rendering;
using panpan.Scene;
using panpan.Assets;
using GlmSharp;
using panpan.Collision;

namespace panpanExample
{
    public class Tile : Entity
    {
        private SpriteRenderer renderer = null!;
        public Tile(int x, int y)
        {
            Position.xy = new vec2(x, y);
        }

        public override void Init()
        {
            renderer = (SpriteRenderer)AddComponent(new SpriteRenderer(new Texture(Sprites.tile, 8, 8)));
            base.Init();
        }

        public override void Update()
        {
            base.Update();
        }
        public override void Render()
        {
            base.Render();
            //collider.DrawDebug();
        }
    }
}