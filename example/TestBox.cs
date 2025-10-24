
using panpan.Rendering;
using panpan.Scene;
using panpan.Assets;
using SDL3;
using panpan;

namespace panpanExample
{
    public class TestBox : Entity
    {
        SpriteRenderer renderer;
        public override void Init()
        {
            renderer = (SpriteRenderer)AddComponent(new SpriteRenderer(new Texture(Sprites.test, 72, 72)));
            Input.RegisterOnKeyHeld(OnKeyHeld);
            base.Init();
        }

        public override void Update()
        {
            base.Update();
        }

        private void OnKeyHeld(SDL.Keycode? e)
        {
            if (e == SDL.Keycode.Left)
            {
                Position.x -= 5;
                Scale.x = 1;
            }
            if (e == SDL.Keycode.Right)
            {
                Position.x += 5;    
                Scale.x = -1;
            }
        }
    }
}