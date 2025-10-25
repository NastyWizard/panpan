
using panpan.Rendering;
using panpan.Scene;
using panpan.Assets;
using SDL3;
using panpan;
using GlmSharp;

namespace panpanExample
{
    public class TestBox : Entity
    {
        SpriteRenderer renderer;
        vec2 speed;
        float grav = 0.3f;
        public override void Init()
        {
            Position.x = 320 / 2;
            speed = vec2.Zero;
            renderer = (SpriteRenderer)AddComponent(new SpriteRenderer(new Texture(Sprites.test, 16, 16)));
            renderer.Origin = new vec2(0f, -1f);
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

            speed.x = 0;
            speed.y -= grav;
            Position.x = Input.MousePosition.x;
            Position.y = Input.MousePosition.y;

            base.Update();
        }

        public override void Render(nint renderPass)
        {
            base.Render(renderPass);

            Draw.Line(Position.xy, Input.MousePosition);
        }

        private void OnKeyHeld(SDL.Keycode? k)
        {
            if (k == SDL.Keycode.Left)
            {
                speed.x = -5;
                Scale.x = 1;
            }
            if (k == SDL.Keycode.Right)
            {
                speed.x = 5;
                Scale.x = -1;
            }

        }

        private void OnKeyDown(SDL.Keycode? k)
        {
            
            if (k == SDL.Keycode.Z)
            {
                speed.y = 5;
            }
        }
    }
}