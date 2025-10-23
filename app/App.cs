using SDL3;
using System.Runtime.InteropServices;

using panpan.Rendering;
using panpan.Assets;
using panpan.Scene;

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

        Platform platform;
        static nint gpuDevice;
        static nint window;
        static nint commandBuffer;

        static SceneManager sceneManager;
        static Input inputManager;
        
        MeshRenderer? meshTest;

        public App()
        {
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
            window = SDL.CreateWindow("GPU_API_Circular_Color_Fade", 800, 600, 0);
            if (!SDL.ClaimWindowForGPUDevice(gpuDevice, window))
            {
                Console.WriteLine($"Error: Failed to create window for gpu device: {SDL.GetError()}");
                Environment.Exit(1);
            }
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

        internal bool Init()
        {
            sceneManager = new SceneManager(new TestScene());
            inputManager = new panpan.Input();
            return true;
        }

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

        public void Update()
        {
            sceneManager.ActiveScene.Update();
        }

        public void Render()
        {
            commandBuffer = SDL.AcquireGPUCommandBuffer(gpuDevice);
            SDL.WaitAndAcquireGPUSwapchainTexture(
                commandBuffer, window,
                out var swapchainTexture,
                out var _,
                out var _
            );

            if (swapchainTexture != nint.Zero)
            {
                var colorTargetInfo = new SDL.GPUColorTargetInfo
                {
                    Texture = swapchainTexture,
                    LoadOp = SDL.GPULoadOp.Clear,
                    StoreOp = SDL.GPUStoreOp.Store,
                    ClearColor = new SDL.FColor
                    {
                        R = 0.0f,
                        G = 0.3f,
                        B = 0.8f,
                        A = 1
                    }
                };

                var ptr = SDL.StructureToPointer<SDL.GPUColorTargetInfo>(colorTargetInfo);
                var renderPass = SDL.BeginGPURenderPass(
                    commandBuffer,
                    ptr,
                    1,
                    nint.Zero
                );
                Marshal.FreeHGlobal(ptr);

                sceneManager.ActiveScene.Render(renderPass);


                SDL.EndGPURenderPass(renderPass);
            }

            SDL.SubmitGPUCommandBuffer(commandBuffer);
        }

    }
}