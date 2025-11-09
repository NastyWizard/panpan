
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
    public class TestWall : Entity
    {
        private SpriteRenderer renderer = null!;
        private BoxCollider collider = null!;

        public TestWall(int x, int y)
        {
            Position.xy = new vec2(x, y);
        }
        public override void Init()
        {
            renderer = (SpriteRenderer)AddComponent(new SpriteRenderer(new Texture(Sprites.tile, 8, 8)));
            renderer.RegisterSetUniforms(SetUniforms);

            collider = (BoxCollider)AddComponent(new BoxCollider(8, 8));
            collider.SetOffset(0,-7);

            base.Init();
        }

        public override void Update()
        {
            base.Update();
        }

        public void SetUniforms()
        {
            float[] uniforms = new float[8] {
                // time
                SDL.GetTicks() / 1000.0f,
                0.0f, 0.0f, 0.0f, // padding
                // color
                1.0f, 1.0f, 1.0f, 1.0f
            };
            renderer.SetUniformFloat(uniforms);
        }
        public override void Render()
        {
            base.Render();
            //collider.DrawDebug();
        }
    }
}