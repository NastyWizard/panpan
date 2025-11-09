
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

        TestPlayer player;
        List<TestWall> walls = new List<TestWall>();
        PTimer wallTimer;
        bool showDebugWindow = true;

        Texture textureTest;
        ImGuiTextureRef textureRef;

        Editor? editor;

        public TestScene() : base("test") { }

        public override void Init()
        {
            // base.Init should usually be called last
            for (var i = 0; i < 20; i++)
            {
                walls.Add((TestWall)AddChild(new TestWall(-320 / 2 + i * 8, 0)));
            }
            walls.Add((TestWall)AddChild(new TestWall(-64, 8)));

            player = (TestPlayer)AddChild(new TestPlayer());
            player.Position.y = 1;

            base.Init();

            // Camera is setup in base.Init so it must be adjusted after.
            Camera.SetBounds(320, 180);
            App.SetBGColor(panpan.Rendering.Color.Black);

            textureTest = new Texture(panpan.Assets.Sprites.tile, 8, 8);
            textureRef = ImGuiTextureRef.FromTexture(textureTest);
#if DEBUG
            editor = new Editor();
#endif
        }

        public override void Render()
        {
            base.Render();
            editor?.ShowEditor();
        }
    }
}