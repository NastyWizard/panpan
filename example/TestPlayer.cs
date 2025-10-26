
using panpan.Rendering;
using panpan.Scene;
using panpan.Assets;
using panpan.Util;
using SDL3;
using panpan;
using GlmSharp;

namespace panpanExample
{
    public class TestPlayer : Entity
    {
        SpriteRenderer renderer;
        vec2 speed;
        float grav = 0.3f;
        public override void Init()
        {
            Position.x = 320 / 2;
            speed = vec2.Zero;
            renderer = (SpriteRenderer)AddComponent(new SpriteRenderer(new Texture(Sprites.test, 16, 16)));
            renderer.RegisterSetUniforms(SetUniforms);
            renderer.Origin = new vec2(8f/16f, 0f);
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
        public override void Render(nint renderPass)
        {
            base.Render(renderPass);

            vec4 col = Color.Hex("#88ff33ff");

            //Draw.Rect(Position.xy - vec2.UnitX * 8, new vec2(15f, 15f));
            Draw.Line(Position.xy, Input.MousePosition, Color.Black);

            Draw.Dot(Position.xy - vec2.UnitX*8, col);
            Draw.Dot(Position.xy + vec2.UnitX*7, col);
            Draw.Dot(Position.xy + vec2.UnitY*15 - vec2.UnitX*8, col);
            Draw.Dot(Position.xy + vec2.UnitY * 15 + vec2.UnitX * 7, col);
            
            Draw.Dot(Input.MousePosition, Color.Hex("#ff00ffff"));
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