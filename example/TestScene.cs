
using System.Drawing;
using GlmSharp;

using panpan;
using panpan.Scene;
using panpan.Rendering;
using panpan.Util;
using panpan.Assets;

namespace panpanExample
{
    public class TestScene : Scene
    {
        public TestPlayer player = null!;
        private Editor? editor = null;

        private vec2? prevMousePos = null;
        private vec3 targetCameraPos;

        private RenderTarget gameTarget;
        private RenderTarget lightTarget;
        private Material lightingMat;

        // used for editor
        public bool FreeCamera = false;

        public TestScene() : base("test") { }

        public TileMap[,] TileMaps = new TileMap[16,16];

        public bool DebugLights = false;

        public Texture ActivePalette;

        public override void Init()
        {
            GameTextures.Init();
            ActivePalette = GameTextures.palette_3;
            lightingMat = new Material(Shaders.bbLighting_frag_hlsl, Shaders.backbuffer_vert_hlsl);

            player = (TestPlayer)AddChild(new TestPlayer(64 + 320*6, 33 + 176*6));

            TilePool.FillPool(3000);

            for (var x = 0; x < 16; x++)
            {
                for(var y = 0; y < 16; y++)
                {
                    var tileMap = new TileMap(x,y, 320/8, 180/8);
                    TileMaps[x,y] = tileMap;
                    AddChild(tileMap);
                }
            }

            base.Init();

            //LoadMapData();

            Input.RegisterOnMouseHeld(OnMouseHeld);
            Input.RegisterOnMouseReleased(OnMouseUp);

            // Camera is setup in base.Init so it must be adjusted after.
            Camera.SetBounds(320, 180);
            Camera.Position.x = 320 / 2;
            Camera.Position.y = 180 / 2;
            targetCameraPos = Camera.Position;
            App.SetBGColor(panpan.Rendering.Color.SkyBlue);
            gameTarget = new RenderTarget((uint)App.GetGameSize().x,(uint)App.GetGameSize().y);
            gameTarget.SetClearColor(panpan.Rendering.Color.Hex("3c6a20"));
            lightTarget = new RenderTarget((uint)App.GetGameSize().x,(uint)App.GetGameSize().y);
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
            // Base game render
            App.EndRenderPass();
            App.SetRenderTarget(gameTarget);
            base.Render();
            
            editor?.ShowEditor();
            // Flush again after editor draws to ensure DrawBounds() calls are rendered
            DrawBatch.Flush();

            // Draw to lighting mask
            App.EndRenderPass();
            App.SetRenderTarget(lightTarget);
            DrawLights();
            // Render game with lighting
            App.ResetRenderTarget();
            Draw.SetRTMaterial(lightingMat);
            Draw.SetRTUniformFloats([DebugLights ? 1.0f : 0.0f, 0.0f, 0.0f, 0.0f]);
            Draw.SetRTAdditionalTextures([lightTarget.GetTexture(), GameTextures.defaultPalette, ActivePalette]);

            Draw.RenderTarget(gameTarget,vec2.Zero);

            Draw.ClearRTAdditionalTextures();
            Draw.ResetRTMaterial();
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

        private void DrawLights()
        {
            
            for (var x = 0; x < 16; x++)
            {
                for(var y = 0; y < 16; y++)
                {
                    TileMaps[x,y].DrawLights();
                }
            }
            
            //Draw.Sprite(GameTextures.lightTex64, Input.MousePosition + new vec2(-32,32));
            Draw.Sprite(GameTextures.lightTex64, player.Position.xy + new vec2(-32,32));
            float t = Time.Elapsed();
            Draw.Sprite(GameTextures.lightTex64, player.Position.xy + new vec2(-32,32) + new vec2(MathF.Sin(t), MathF.Cos(t))*4);
            Draw.Sprite(GameTextures.lightTex64, player.Position.xy + new vec2(-32,32) + new vec2(MathF.Sin(t*-1), MathF.Cos(t*1))*6);

        }
    }
}