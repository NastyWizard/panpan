
using System.Drawing;
using panpan;
using panpan.Scene;
using panpan.Rendering;
using panpanExample;
using GlmSharp;

public class TestScene : Scene
{
    public TestScene() : base("test") { } // Init should handle most setup
    TestPlayer player;
    List<TestWall> walls = new List<TestWall>();
    public override void Init()
    {
        // base.Init should usually be called last
        for (var i = 0; i < 40; i++)
        {
            walls.Add((TestWall)AddChild(new TestWall(-320/2 + i * 8, 0)));
        }

        player = (TestPlayer)AddChild(new TestPlayer());

        base.Init();

        // Camera is setup in base.Init so it must be adjusted after.
        Camera.SetBounds(320, 180);
    }

    public override void Render()
    {
        base.Render();
    }
}