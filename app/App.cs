using SDL3;
using System.Runtime.InteropServices;

using panpan.Rendering;
using panpan.Assets;
using panpan.Scene;
using GlmSharp;
using panpan.Collision;

namespace panpan
{
    enum Platform
    {
        Mac,
        Windows,
        Linux,
        Unkown
    }
    public class App
    {

        // Rendering data
        Platform platform;
        static nint gpuDevice;
        static nint window;
        static nint commandBuffer;
        static nint renderPass;
        static nint swapchainTexture;
        static List<nint> renderFences = new List<nint>();

        // Managers
        static SceneManager sceneManager;
        static Input inputManager;
        static CollisionManager collisionManager;

        // Settings
        static vec4 bgColor;

        // Backbuffer
        static RenderTarget backBuffer;
        ivec2 gameSize;

        public App(string title = "panpan", int width = 320, int height = 180)
        {
            bgColor = Color.SkyBlue;
            gameSize = new ivec2(width, height);

            SDL.Init(SDL.InitFlags.Video);

            var gpuShaderFormat = SDL.GPUShaderFormat.SPIRV;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                platform = Platform.Mac;
                gpuShaderFormat |= SDL.GPUShaderFormat.MSL;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                platform = Platform.Windows;
                gpuShaderFormat |= SDL.GPUShaderFormat.DXBC | SDL.GPUShaderFormat.DXIL;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                platform = Platform.Linux;
            }
            else
            {
                platform = Platform.Unkown;
            }

            gpuDevice = SDL.CreateGPUDevice(gpuShaderFormat, true, null);
            if (gpuDevice == nint.Zero)
            {
                Console.WriteLine($"Error: Failed to create gpu device: {SDL.GetError()}");
                Environment.Exit(1);
            }
            window = SDL.CreateWindow(title, width, height, 0);

            if (!SDL.ClaimWindowForGPUDevice(gpuDevice, window))
            {
                Console.WriteLine($"Error: Failed to create window for gpu device: {SDL.GetError()}");
                Environment.Exit(1);
            }

            SDL.SetGPUAllowedFramesInFlight(gpuDevice, 3);
        }

        public static nint GetDevice()
        {
            return gpuDevice;
        }

        public static nint GetWindow()
        {
            return window;
        }
        public static nint GetCommandBuffer()
        {
            return commandBuffer;
        }
        public static SceneManager GetSceneManager()
        {
            return sceneManager;
        }
        public static CollisionManager GetCollisionManager()
        {
            return collisionManager;
        }

        public static nint GetRenderPass()
        {
            return renderPass;
        }

        public static void SetBGColor(vec4 col)
        {
            bgColor = col;
            backBuffer.SetClearColor(bgColor);
        }

        /// <summary>
        /// Initialize managers.
        /// </summary>
        /// <returns></returns>
        internal bool Init()
        {
            backBuffer = new RenderTarget((uint)gameSize.x, (uint)gameSize.y, bgColor);
            collisionManager = new CollisionManager(CollisionManager.ManagerType.SPACIAL_HASH, 8, new vec2(320, 180));
            sceneManager = new SceneManager(new TestScene());
            inputManager = new Input();
            return true;
        }

        /// <summary>
        /// Called by Project.cs to start the app.
        /// </summary>
        public void Run()
        {
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            Init();
            while (!token.IsCancellationRequested)
            {
                while (SDL.PollEvent(out var evt))
                {
                    if (evt.Type == (uint)SDL.EventType.WindowCloseRequested)
                    {
                        tokenSource.Cancel();
                    }
                    Input.HandleEvents(evt);
                }
                Input.Update();

                Update();
                Render();

            }

            // cleanup
            SDL.DestroyGPUDevice(gpuDevice);
            SDL.DestroyWindow(window);

        }

        /// <summary>
        /// Updates the active scene.
        /// </summary>
        private void Update()
        {
            sceneManager.ActiveScene.Update();
        }

        /// <summary>
        /// Renders the active scene.
        /// </summary>
        private void Render()
        {
            
            backBuffer.SetDoesClear(true);
            SetRenderTarget(backBuffer);
            sceneManager.ActiveScene.Render();
            EndRenderPass();

            CreateDefaultRenderTarget(SDL.GPULoadOp.Clear);
            if (swapchainTexture != nint.Zero)
            {
                Draw.RenderTarget(backBuffer, new vec2(-gameSize.x/2, gameSize.y/2));
            }
            EndRenderPass();
            swapchainTexture = nint.Zero;
        }

        private static void CreateDefaultRenderTarget(SDL.GPULoadOp loadOp = SDL.GPULoadOp.Load)
        {
            WaitAndClearFences();
            commandBuffer = SDL.AcquireGPUCommandBuffer(gpuDevice);

            if (swapchainTexture == nint.Zero)
            {
                if (!SDL.WaitAndAcquireGPUSwapchainTexture(
                    commandBuffer, window,
                    out swapchainTexture,
                    out var _,
                    out var _
                ))
                {
                    Console.WriteLine("SDL Error WaitAndAcquireGPUSwapchainTexture: " + SDL.GetError());
                }
            }

            var colorTargetInfo = new SDL.GPUColorTargetInfo
            {
                Texture = swapchainTexture,
                LoadOp = loadOp,
                StoreOp = SDL.GPUStoreOp.Store
            };
            if (loadOp == SDL.GPULoadOp.Clear)
            {
                colorTargetInfo.ClearColor = new SDL.FColor
                {
                    R = 1.0f,
                    G = 0.0f,
                    B = 1.0f,
                    A = 1.0f
                };
            }

            var ptr = SDL.StructureToPointer<SDL.GPUColorTargetInfo>(colorTargetInfo);
            renderPass = SDL.BeginGPURenderPass(
                commandBuffer,
                ptr,
                1,
                nint.Zero
            );
            Marshal.FreeHGlobal(ptr);
        }

        /// <summary>
        /// Resets to use the swapchain render target on a new render pass
        /// </summary>
        /// <param name="loadOp"></param>
        public static void ResetRenderTarget(SDL.GPULoadOp loadOp = SDL.GPULoadOp.Load)
        {
            EndRenderPass();
            backBuffer.SetDoesClear(false);
            SetRenderTarget(backBuffer);
        }

        /// <summary>
        /// Sets the active render pass to use passed in render target
        /// </summary>
        /// <param name="target">Render target</param>
        public static void SetRenderTarget(RenderTarget target)
        {
            WaitAndClearFences();
            commandBuffer = SDL.AcquireGPUCommandBuffer(gpuDevice);
            renderPass = target.CreateRenderPass();
        }

        /// <summary>
        /// Ends currently active render pass.
        /// </summary>
        private static void EndRenderPass()
        {
            SDL.EndGPURenderPass(renderPass);
            //SDL.SubmitGPUCommandBuffer(commandBuffer);
            nint fence = SDL.SubmitGPUCommandBufferAndAcquireFence(commandBuffer);
            renderFences.Add(fence);
        }

        private static void WaitAndClearFences()
        {
            if (renderFences.Count > 0)
            {
                SDL.WaitForGPUFences(GetDevice(), true, renderFences.ToArray(), (uint)renderFences.Count);

                foreach (var fence in renderFences)
                {
                    SDL.ReleaseGPUFence(GetDevice(), fence);
                }
                renderFences.Clear();
            }
        }
    }
}