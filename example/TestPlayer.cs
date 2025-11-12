
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

        private int coyoteTimeMax = 6;
        private int coyoteTime = 0;

        #region  state
        private bool isMoving = false;

        #endregion state

        public TestPlayer(int x, int y)
        {
            Position.x = x;
            Position.y = y;
        }

        public override void Init()
        {
            renderer = (SpriteRenderer)AddComponent(new SpriteRenderer(new Texture(Sprites.player, 8, 8)));
            renderer.Clip(new Rect(0, 0, 8, 8));
            //renderer.RegisterSetUniforms(SetUniforms);
            renderer.Origin = new vec2(4f / 8f, -7f / 8f);

            collider = (BoxCollider)AddComponent(new BoxCollider(8, 8));
            collider.SetOffset(-4, 0);

            Input.RegisterOnKeyHeld(OnKeyHeld);
            Input.RegisterOnKeyDown(OnKeyDown);
            Input.RegisterOnKeyReleased(OnKeyReleased);

            base.Init();
        }

        public override void Update()
        {
            coyoteTime--;
            coyoteTime = Math.Max(coyoteTime, 0);
            // Grounded
            if (App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy - vec2.UnitY))
            {
                coyoteTime = coyoteTimeMax;
            }

            Move(ref speed);
            // Need to clear speed after, as input happens before update
            if(!isMoving)
                speed.x *= .5f * Scene.TimeScale;
            speed.y -= grav * Scene.TimeScale;

            speed.y = MathF.Max(speed.y, -8.0f);

            if (Position.y < -100)
                Position.y = 100;

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
        }

        private void OnKeyHeld(SDL.Keycode? k)
        {
            float moveSpd = 0.1f;
            if (k == SDL.Keycode.Left || k == SDL.Keycode.A)
            {
                speed.x += -moveSpd;
                speed.x = MathF.Max(speed.x, -1.0f);
                Scale.x = -1;
                isMoving = true;
            }
            if (k == SDL.Keycode.Right || k == SDL.Keycode.D)
            {
                speed.x += moveSpd;
                speed.x = MathF.Min(speed.x, 1.0f);
                Scale.x = 1;
                isMoving = true;
            }
        }

        private void OnKeyDown(SDL.Keycode? k)
        {
            if (coyoteTime > 0 && (k == SDL.Keycode.Z || k == SDL.Keycode.Space))
            {
                speed.y = 3;
            }

            if (k == SDL.Keycode.R)
            {
                renderer.Clip(8, 8, 8, 8);
            }
        }

        private void OnKeyReleased(SDL.Keycode? k)
        {
            if (k == SDL.Keycode.Left || k == SDL.Keycode.A || k == SDL.Keycode.Right || k == SDL.Keycode.D)
            {
                isMoving = false;
            }
        }

        private void Move(ref vec2 speed)
        {
            var spd = Scene.TimeScale * speed;
            ivec2 sign = new ivec2(MathF.Sign(spd.x), MathF.Sign(spd.y));
            ivec2 move = new ivec2((int)MathF.Round(spd.x), (int)MathF.Round(spd.y));
            vec2 r = spd - move;

            // Horizontal
            while (move.x != 0)
            {
                if (!App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(sign.x, 0.0f)))
                {
                    Position.x += sign.x;
                    move.x -= sign.x;
                    if (App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(0.0f, 1.0f)))
                    {
                        speed.y = MathF.Min(0f, speed.y);
                        spd.y = Scene.TimeScale * speed.y;
                        sign.y = MathF.Sign(spd.y);
                        move.y = (int)MathF.Round(spd.y);
                    }
                }
                else if (!App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(sign.x, 2.0f)) || !App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(sign.x, 1.0f)))
                {
                    Position.y++;
                    Position.y = MathF.Round(Position.y);
                }
                else
                {
                    speed.x = 0;
                    break;
                }
            }

            // Apply remainder horizontally if non-zero
            if (r.x != 0 && speed.x != 0)
            {
                if (!App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(r.x, 0.0f)))
                {
                    Position.x += r.x;
                    if (App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(0.0f, 1.0f)))
                    {
                        speed.y = MathF.Min(0f, speed.y);
                        spd.y = Scene.TimeScale * speed.y;
                        sign.y = MathF.Sign(spd.y);
                        move.y = (int)MathF.Round(spd.y);
                    }
                }
                else if (!App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(r.x, 2.0f)) || !App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(r.x, 1.0f)))
                {
                    Position.y++;
                }
                else
                {
                    speed.x = 0;
                }
            }

            // --------------------------------------------------------------------------
            // --------------------------------------------------------------------------

            // Vertical
            while (move.y != 0)
            {
                if (!App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(0.0f, sign.y)))
                {
                    Position.y += sign.y;
                    move.y -= sign.y;
                }
                else if (!App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(2.0f, sign.y)) || !App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(1.0f, sign.y)))
                {
                    Position.x++;
                }
                else if (!App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(-2.0f, sign.y)) || !App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(-1.0f, sign.y)))
                {
                    Position.x--;
                }
                else
                {
                    speed.y = 0;
                    break;
                }
            }

            // Apply remainder vertically if non-zero
            if (r.y != 0 && speed.y != 0)
            {
                if (!App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(0.0f, r.y)))
                {
                    Position.y += r.y;
                }
                else if (!App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(2.0f, r.y)) || !App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(1.0f, r.y)))
                {
                    Position.x++;
                }
                else if (!App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(-2.0f, r.y)) || !App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(-1.0f, r.y)))
                {
                    Position.x--;
                }
                else
                {
                    speed.y = 0;
                }
            }
        }
    }
}