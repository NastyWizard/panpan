
using System.Drawing;
using panpan;
using panpan.Scene;
using panpan.Rendering;
using panpanExample;

public class TestScene : Scene
{
    public TestScene() : base("test") { } // Init should handle most setup
    TestBox player;
    public override void Init()
    {
        // base.Init should usually be called last
        player = (TestBox)AddChild(new TestBox());
        base.Init();
        Camera.SetBounds(320, 180);
        Camera.Position.x = 320 / 2;
        Camera.Position.y = 180 / 2;
    }
}