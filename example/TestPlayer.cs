
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
        private Animator animator = null;
        private BoxCollider collider = null!;
        private vec2 speed = vec2.Zero;
        private int dir;

        //
        private int jumpQueMax = 4;
        private int jumpQue = 0;
        private readonly float grav = 0.2f;
        private readonly float jumpSpeed = 3.25f;
        private readonly float walkSpeed = 0.5f;
        private readonly float walkSpeedMax = 1f;

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
            renderer = (SpriteRenderer)AddComponent(new SpriteRenderer(new Texture(Sprites.player, 8*3, 8)));
            renderer.Clip(new Rect(0, 0, 8, 8));
            //renderer.RegisterSetUniforms(SetUniforms);
            renderer.Origin = new vec2(4f / 8f, -7f / 8f);

            animator = (Animator)AddComponent(new Animator(ref renderer, 8, 8));
            animator.AddAnimation("idle", [0]);
            animator.AddAnimation("walk", [1,2]);

            collider = (BoxCollider)AddComponent(new BoxCollider(8, 8));
            collider.SetOffset(-4, 0);

            Input.RegisterOnKeyHeld(OnKeyHeld);
            Input.RegisterOnKeyDown(OnKeyDown);
            Input.RegisterOnKeyReleased(OnKeyReleased);

            base.Init();
        }

        public override void Update()
        {
            var grounded = App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy - vec2.UnitY);

            if (coyoteTime > 0 && jumpQue > 0)
            {
                speed.y = jumpSpeed;
                coyoteTime = 0;
                jumpQue = 0;
            }

            jumpQue--;
            jumpQue = Math.Max(jumpQue, 0);
            coyoteTime--;
            coyoteTime = Math.Max(coyoteTime, 0);

            if (grounded)
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

            if (Math.Abs(speed.x) >= 0.1f && grounded)
            {
                animator.Play("walk");
            }
            else
            {
                animator.Play("idle");
            }

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
            var spd = walkSpeed;
            if (speed.x != 0)
            {
                spd = 0.1f;
            }
            if (k == SDL.Keycode.Left || k == SDL.Keycode.A)
            {
                speed.x += -spd;
                speed.x = MathF.Max(speed.x, -walkSpeedMax);
                Scale.x = -1;
                isMoving = true;
            }
            if (k == SDL.Keycode.Right || k == SDL.Keycode.D)
            {
                speed.x += spd;
                speed.x = MathF.Min(speed.x, walkSpeedMax);
                Scale.x = 1;
                isMoving = true;
            }
        }

        private void OnKeyDown(SDL.Keycode? k)
        {
            if (k == SDL.Keycode.Z || k == SDL.Keycode.Space)
            {
                jumpQue = jumpQueMax;
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

            // Horizontal movement
            while (move.x != 0)
            {
                if (!App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(sign.x, 0.0f)))
                {
                    Position.x += sign.x;
                    move.x -= sign.x;
                    // Check for ceiling after horizontal movement
                    if (App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(0.0f, 1.0f)))
                    {
                        speed.y = MathF.Min(0f, speed.y);
                        spd.y = Scene.TimeScale * speed.y;
                        sign.y = (int)MathF.Sign(spd.y);
                        move.y = (int)MathF.Round(spd.y);
                        r.y = spd.y - move.y;
                    }
                }
                // Try to climb up if blocked horizontally
                else if (!App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(sign.x, 1.0f)) && 
                         !App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(sign.x, 2.0f)))
                {
                    Position.y++;
                    Position.y = MathF.Round(Position.y);
                }
                else
                {
                    speed.x = 0;
                    move.x = 0;
                }
            }

            // Vertical movement
            while (move.y != 0)
            {
                if (!App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(0.0f, sign.y)))
                {
                    Position.y += sign.y;
                    move.y -= sign.y;
                }
                // Try to slide horizontally if blocked vertically (only when moving up)
                else if (sign.y == 1)
                {
                    // Try sliding right
                    if (!App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(1.0f, sign.y)) || 
                        !App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(2.0f, sign.y)))
                    {
                        Position.x++;
                    }
                    // Try sliding left
                    else if (!App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(-1.0f, sign.y)) || 
                             !App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(-2.0f, sign.y)))
                    {
                        Position.x--;
                    }
                    else
                    {
                        speed.y = 0;
                        move.y = 0;
                    }
                }
                else
                {
                    speed.y = 0;
                    move.y = 0;
                }
            }

            // Apply remainder horizontally if non-zero
            if (r.x != 0 && speed.x != 0)
            {
                float newX = Position.x + r.x;
                // Check if remainder would cross an integer boundary (collision system rounds to int)
                int currentCellX = (int)MathF.Round(Position.x);
                int newCellX = (int)MathF.Round(newX);
                
                vec2 checkPos = Position.xy;
                // If crossing a boundary, check collision at the boundary
                if (newCellX != currentCellX)
                {
                    checkPos.x = newCellX;
                }
                else
                {
                    checkPos.x = newX;
                }
                
                if (!App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), checkPos))
                {
                    Position.x = newX;
                    // Check for ceiling after horizontal remainder movement
                    if (App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), Position.xy + new vec2(0.0f, 1.0f)))
                    {
                        speed.y = MathF.Min(0f, speed.y);
                    }
                }
                // Try to climb up if blocked horizontally
                else if (!App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), checkPos + new vec2(0.0f, 1.0f)) && 
                         !App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), checkPos + new vec2(0.0f, 2.0f)))
                {
                    Position.y++;
                    Position.y = MathF.Round(Position.y);
                }
                else
                {
                    speed.x = 0;
                }
            }

            // Apply remainder vertically if non-zero
            if (r.y != 0 && speed.y != 0)
            {
                float newY = Position.y + r.y;
                // Check if remainder would cross an integer boundary (collision system rounds to int)
                int currentCellY = (int)MathF.Round(Position.y);
                int newCellY = (int)MathF.Round(newY);
                
                vec2 checkPos = Position.xy;
                // If crossing a boundary, check collision at the boundary
                if (newCellY != currentCellY)
                {
                    checkPos.y = newCellY;
                }
                else
                {
                    checkPos.y = newY;
                }
                
                if (!App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), checkPos))
                {
                    Position.y = newY;
                }
                // Try to slide horizontally if blocked vertically (only when moving up)
                else if (sign.y == 1)
                {
                    // Try sliding right
                    if (!App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), checkPos + new vec2(1.0f, 0.0f)) ||
                        !App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), checkPos + new vec2(2.0f, 0.0f)))
                    {
                        Position.x++;
                    }
                    // Try sliding left
                    else if (!App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), checkPos + new vec2(-1.0f, 0.0f)) || 
                             !App.GetCollisionManager().IntersectsWith(collider, typeof(TestWall), checkPos + new vec2(-2.0f, 0.0f)))
                    {
                        Position.x--;
                    }
                    else
                    {
                        speed.y = 0;
                    }
                }
                else
                {
                    speed.y = 0;
                }
            }
        }
    }
}