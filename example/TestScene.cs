
using System.Drawing;
using GlmSharp;

using panpan;
using panpan.Scene;
using panpan.Rendering;
using panpanExample;
using panpan.Util;
using panpan.Rendering;

public class TestScene : Scene
{
    public TestScene() : base("test")
    {
    }

    TestPlayer player;
    List<TestWall> walls = new List<TestWall>();
    PTimer wallTimer;
    bool showDebugWindow = true;

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
    }

    public override void Render()
    {
        base.Render();
        DrawImGui();
    }

    private void DrawImGui()
    {
        if (!showDebugWindow)
        {
            return;
        }

        bool open = showDebugWindow;
        bool visible = ImGui.Begin("Scene Debug", ref open);
        showDebugWindow = open;

        if (visible)
        {
            ImGui.Text($"FPS: {App.GetFPS():F2}");
            ImGui.Text($"Player Pos: {player.Position.x:F2}, {player.Position.y:F2}");
            ImGui.Text($"Camera Pos: {Camera.Position.x:F2}, {Camera.Position.y:F2}");
            ImGui.Text($"Mouse Pos: {Input.MousePosition.x:F2}, {Input.MousePosition.y:F2}");
            ImGui.Text($"Walls: {walls.Count}");

            if (ImGui.Button("Reset Player"))
            {
                player.Position.x = 0;
                player.Position.y = 1;
            }
        }

        ImGui.End();
    }
}