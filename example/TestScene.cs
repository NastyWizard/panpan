
using panpan.Scene;
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
    }
}