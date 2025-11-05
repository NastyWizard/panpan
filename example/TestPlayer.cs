
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
    public class TestPlayer : Entity
    {
        SpriteRenderer renderer;
        BoxCollider collider;
        vec2 speed;
        float grav = 0.3f;
        public override void Init()
        {
            speed = vec2.Zero;
            
            renderer = (SpriteRenderer)AddComponent(new SpriteRenderer(new Texture(Sprites.test, 16, 16)));
            //renderer.RegisterSetUniforms(SetUniforms);
            renderer.Origin = new vec2(8f/16f, -15f/16f);

            collider = (BoxCollider)AddComponent(new BoxCollider(16, 16));
            collider.SetOffset(-8,0);

            Input.RegisterOnKeyHeld(OnKeyHeld);
            Input.RegisterOnKeyDown(OnKeyDown);
            base.Init();
        }

        public override void Update()
        {
            // TODO replace with move / colision func

            if (Position.y + speed.y < 0)
            {
                Position.y = 0;
                speed.y = 0;
            }

            Position.xy += speed;
            
            // Need to clear speed after, as input happens bedfore update
            speed.x = 0;
            speed.y -= grav;

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
            if (App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy))
            {
                collider.DrawDebug();
            }

            vec4 col = Color.Hex("#000000ff");

            Draw.Dot(Position.xy, Color.Hex("#ff00ffff"));
        }

        private void OnKeyHeld(SDL.Keycode? k)
        {
            if (k == SDL.Keycode.Left || k == SDL.Keycode.A)
            {
                speed.x = -2f;
                Scale.x = -1;
            }
            if (k == SDL.Keycode.Right || k == SDL.Keycode.D)
            {
                speed.x = 2f;
                Scale.x = 1;
            }
        }

        private void OnKeyDown(SDL.Keycode? k)
        {
            if (k == SDL.Keycode.Z || k == SDL.Keycode.Space)
            {
                speed.y = 5;
            }
        }
    }
}