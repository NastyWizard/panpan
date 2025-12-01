
using System.Drawing;
using GlmSharp;

using panpan;
using panpan.Scene;
using panpan.Rendering;
using panpan.Util;

namespace panpanExample
{
    public class TestScene : Scene
    {
        public TestPlayer player = null!;
        private Editor? editor = null;

        private vec2? prevMousePos = null;
        private vec3 targetCameraPos;

        // used for editor
        public bool FreeCamera = false;

        public TestScene() : base("test") { }

        public TileMap[,] TileMaps = new TileMap[16,16];

        public override void Init()
        {

            player = (TestPlayer)AddChild(new TestPlayer(64 + 320*6, 33 + 176*6));

            base.Init();

            for (var x = 0; x < 16; x++)
            {
                for(var y = 0; y < 16; y++)
                {
                    var tileMap = new TileMap(x,y, 320/8, 180/8);
                    TileMaps[x,y] = tileMap;
                    AddChild(tileMap);
                }
            }
            //LoadMapData();

            Input.RegisterOnMouseHeld(OnMouseHeld);
            Input.RegisterOnMouseReleased(OnMouseUp);

            // Camera is setup in base.Init so it must be adjusted after.
            Camera.SetBounds(320, 180);
            Camera.Position.x = 320 / 2;
            Camera.Position.y = 180 / 2;
            targetCameraPos = Camera.Position;
            App.SetBGColor(panpan.Rendering.Color.Black);
#if DEBUG
            editor = new Editor();
            editor?.Init();
#endif
        }

        private void OnMouseHeld(byte btn)
        {
            if(btn == 2)
            {
                if(prevMousePos != null)
                {
                    vec2 delta = prevMousePos.Value - Input.MousePositionWindow;
                    targetCameraPos.xy += Camera.Zoom * (new vec2(delta.x,-delta.y) / 4);
                }
                prevMousePos = Input.MousePositionWindow;
            }
        }
        private void OnMouseUp(byte btn)
        {
            if(btn == 2)
            {
                prevMousePos = null;
            }
        }

        public override void Update()
        {
            base.Update();
            if(FreeCamera)
            {
                Camera.Position.x = float.Lerp(Camera.Position.x, targetCameraPos.x, 0.1f);
                Camera.Position.y = float.Lerp(Camera.Position.y, targetCameraPos.y, 0.1f);
            }
            else
            {
                vec2 size = new vec2(320,176);
                vec2 center = size/2;
                vec2 p = player.Position.xy / size;
                p.x = MathF.Floor(p.x);
                p.y = MathF.Floor(p.y);
                Camera.Position.xy = size * p + center;
                targetCameraPos = Camera.Position;
            }
        }

        public override void Render()
        {
            base.Render();
            editor?.ShowEditor();
            // Flush again after editor draws to ensure DrawBounds() calls are rendered
            DrawBatch.Flush();
        }

        private void LoadMapData()
        {
            var t = Time.Elapsed();
            for (var x = 0; x < 16; x++)
            {
                for(var y = 0; y < 16; y++)
                {
                    TileMaps[x,y].LoadFromData(TileMapData.mapData[x][y]);
                }
            }
            var et = Time.Elapsed();
            Console.WriteLine($"Loaded: {et - t}s");
        }
    }
}