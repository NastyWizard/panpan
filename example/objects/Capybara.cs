
using panpan.Rendering;
using panpan.Scene;
using panpan.Assets;
using panpan.Util;
using SDL3;
using panpan;
using GlmSharp;
using panpan.Collision;

namespace panpanExample
{
    public class Capybara : Entity
    {
        private SpriteRenderer renderer = null!;
        private BoxCollider collider = null!;

        public Capybara(int x, int y)
        {
            Position.x = x;
            Position.y = y;
        }

        public override void Init()
        {
            renderer = (SpriteRenderer)AddComponent(new SpriteRenderer(new Texture(Sprites.capybara, 22, 18)));
            renderer.Origin = new vec2(11f / 22f, -17f / 18f);

            collider = (BoxCollider)AddComponent(new BoxCollider(22, 18));
            collider.SetOffset(-4, 0);

            base.Init();
        }
    }
}