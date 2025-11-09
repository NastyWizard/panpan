
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
        private SpriteRenderer renderer = null!;
        private BoxCollider collider = null!;
        private vec2 speed = vec2.Zero;
        private readonly float grav = 0.2f;
        public override void Init()
        {
            renderer = (SpriteRenderer)AddComponent(new SpriteRenderer(new Texture(Sprites.test, 16, 16)));
            renderer.Clip(new Rect(0, 0, 8, 8));
            //renderer.RegisterSetUniforms(SetUniforms);
            //renderer.Origin = new vec2(8f/16f, -15f/16f);

            collider = (BoxCollider)AddComponent(new BoxCollider(16, 16));
            collider.SetOffset(-8,0);

            Input.RegisterOnKeyHeld(OnKeyHeld);
            Input.RegisterOnKeyDown(OnKeyDown);
            base.Init();
        }

        public override void Update()
        {
            Move(ref speed);
            // Need to clear speed after, as input happens before update
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
            
            //collider.DrawDebug();

            vec4 col = Color.Hex("#000000ff");

            //Draw.Dot(Position.xy, Color.Hex("#ff00ffff"));
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
                speed.y = 3;
            }

            if (k == SDL.Keycode.R)
            {
                renderer.Clip(8, 8,8,8);
            }
        }

        private void Move(ref vec2 speed)
        {
            ivec2 sign = new ivec2(MathF.Sign(speed.x), MathF.Sign(speed.y));
            ivec2 move = new ivec2((int)MathF.Round(speed.x), (int)MathF.Round(speed.y));
            vec2 r = speed - move;

            // Horizontal
            while (move.x != 0)
            {
                if (!App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(sign.x, 0.0f)))
                {
                    Position.x += sign.x;
                    move.x -= sign.x;
                }
                else
                {
                    speed.x = 0;
                    break;
                }
            }

            // Vertical
            while (move.y != 0)
            {
                if (!App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(0.0f, sign.y)))
                {
                    Position.y += sign.y;
                    move.y -= sign.y;
                }
                else
                {
                    speed.y = 0;
                    break;
                }
            }
            
            //Position.xy += r;
        }
    }
}