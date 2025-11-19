
using System.Drawing;
using GlmSharp;

using panpan;
using panpan.Scene;
using panpan.Rendering;
using panpanExample;
using panpan.Util;

namespace panpanExample
{
    public class TestScene : Scene
    {
        private TestPlayer player = null!;
        private readonly List<Tile> walls = new List<Tile>();
        private Editor? editor = null;

        public TestScene() : base("test") { }

        public override void Init()
        {
            // base.Init should usually be called last
            for (var i = 0; i < 40; i++)
            {
                walls.Add((Tile)AddChild(new Tile(i * 8, 8)));
            }
            walls.Add((Tile)AddChild(new Tile(-64, 8)));

            player = (TestPlayer)AddChild(new TestPlayer(32, 9));

            base.Init();

            // Camera is setup in base.Init so it must be adjusted after.
            Camera.SetBounds(320, 180);
            Camera.Position.x = 320 / 2;
            Camera.Position.y = 180 / 2;
            App.SetBGColor(panpan.Rendering.Color.Black);
#if DEBUG
            editor = new Editor();
            editor?.Init();
#endif
        }

        public override void Render()
        {
            base.Render();
            editor?.ShowEditor();
        }
    }
}