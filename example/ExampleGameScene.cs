
using System.Drawing;
using GlmSharp;

using panpan;
using panpan.Scene;
using panpan.Rendering;
using panpan.Util;
using panpan.Assets;
using SDL3;

namespace panpanExample
{
    public class ExampleGameScene : Scene
    {
        public ExamplePlayer player = null!;
        private ExampleEditor? editor = null;

        private vec2? prevMousePos = null;
        private vec3 targetCameraPos;

        private RenderTarget gameTarget;
        private RenderTarget lightTarget;
        private Material lightingMat;

        // used for editor
        public bool FreeCamera = false;

        public ExampleGameScene() : base("test") { }

        public ExampleTileMap[,] TileMaps = new ExampleTileMap[16,16];
        public ExampleTileMap? ActiveTileMap;

        public bool DebugLights = false;

        public Texture ActivePalette;

        public override void Init()
        {
            ExampleGameTextures.Init();
            ActivePalette = ExampleGameTextures.palette_3;
            lightingMat = DefaultMaterials.BackbufferLighting;

            player = (ExamplePlayer)AddChild(new ExamplePlayer(64 + 320*6, 33 + 176*6));

            ExampleTilePool.FillPool(3000);

            for (var x = 0; x < 16; x++)
            {
                for(var y = 0; y < 16; y++)
                {
                    var tileMap = new ExampleTileMap(x,y, 320/8, 180/8);
                    TileMaps[x,y] = tileMap;
                    AddChild(tileMap);
                }
            }

            base.Init();

            ActiveTileMap = TileMaps[(int)player.Position.x / 320, (int)player.Position.y / 176];

            //LoadMapData();

            Input.RegisterOnMouseHeld(OnMouseHeld);
            Input.RegisterOnMouseReleased(OnMouseUp);

            // Camera is setup in base.Init so it must be adjusted after.
            Camera.SetBounds((uint)App.GetDisplayBounds().Width/8, (uint)App.GetDisplayBounds().Height/8);
            Camera.Position.x = 320 / 2;
            Camera.Position.y = 180 / 2;
            targetCameraPos = Camera.Position;
            App.SetBGColor(panpan.Rendering.Color.SkyBlue);
            gameTarget = new RenderTarget((uint)App.GetGameSize().x,(uint)App.GetGameSize().y);
            gameTarget.SetClearColor(panpan.Rendering.Color.Hex("3c6a20"));
            lightTarget = new RenderTarget((uint)App.GetGameSize().x,(uint)App.GetGameSize().y);
#if DEBUG
            editor = new ExampleEditor();
            editor?.Init();
#endif
            App.ToggleFullscreen();
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
            ActiveTileMap = TileMaps[(int)player.Position.x / 320, (int)player.Position.y / 176];
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
            Draw.SetRTAdditionalTextures([lightTarget.GetTexture(), ExampleGameTextures.defaultPalette, ActivePalette]);

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
                    TileMaps[x,y].LoadFromData(ExampleTileMapData.mapData[x][y]);
                }
            }
            var et = Time.Elapsed();
            Console.WriteLine($"Loaded: {et - t}s");
        }

        private void DrawLights()
        {
            if(FreeCamera)
            {
                for (var x = 0; x < 16; x++)
                {
                    for(var y = 0; y < 16; y++)
                    {
                        TileMaps[x,y].DrawLights();
                    }
                }
            }
            else
            {
                ActiveTileMap?.DrawLights();
            }

            //Draw.Sprite(ExampleGameTextures.lightTex64, Input.MousePosition + new vec2(-32,32));
            Draw.Sprite(ExampleGameTextures.lightTex64, player.Position.xy + new vec2(-32,32));
            float t = Time.Elapsed();
            Draw.Sprite(ExampleGameTextures.lightTex64, player.Position.xy + new vec2(-32,32) + new vec2(MathF.Sin(t), MathF.Cos(t))*4);
            Draw.Sprite(ExampleGameTextures.lightTex64, player.Position.xy + new vec2(-32,32) + new vec2(MathF.Sin(t*-1), MathF.Cos(t*1))*6);

        }
    }
}