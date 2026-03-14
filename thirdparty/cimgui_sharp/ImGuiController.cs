using System;
using System.Runtime.InteropServices;
using SDL3;
using ImGuiNET.Backend.SDLGPU;

namespace panpan.Rendering
{
    public sealed class ImGuiController : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct ImGuiImplSDLGPU3InitInfoNative
        {
            public nint Device;
            public SDL.GPUTextureFormat ColorTargetFormat;
            public SDL.GPUSampleCount MSAASamples;
            public SDL.GPUSwapchainComposition SwapchainComposition;
            public SDL.GPUPresentMode PresentMode;
        }

#if DEBUG
        private SWIGTYPE_p_ImGuiContext? context;
        private SWIGTYPE_p_ImDrawData? currentDrawData;
#endif
        private bool initialized;
        public event Action? OnRender;
        public void Initialize(nint device, nint window)
        {
#if DEBUG
            if (initialized)
            {
                return;
            }

            context = cimgui_sdlgpu.igCreateContext(null);
            cimgui_sdlgpu.igSetCurrentContext(context);
            cimgui_sdlgpu.igStyleColorsDark(null);

            var windowPtr = new SWIGTYPE_p_SDL_Window(window, false);
            if (!cimgui_sdlgpu.igImplSDL3_InitForSDLGPU(windowPtr))
            {
                throw new InvalidOperationException("ImGui_ImplSDL3_InitForSDLGPU failed.");
            }

            var initInfoNative = new ImGuiImplSDLGPU3InitInfoNative
            {
                Device = device,
                ColorTargetFormat = SDL.GetGPUSwapchainTextureFormat(device, window),
                MSAASamples = SDL.GPUSampleCount.SampleCount1,
                SwapchainComposition = SDL.GPUSwapchainComposition.SDR,
                PresentMode = SDL.GPUPresentMode.Immediate
            };

            var initInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf<ImGuiImplSDLGPU3InitInfoNative>());
            try
            {
                Marshal.StructureToPtr(initInfoNative, initInfoPtr, false);
                var initInfo = new SWIGTYPE_p_ImGui_ImplSDLGPU3_InitInfo(initInfoPtr, false);
                if (!cimgui_sdlgpu.igImplSDLGPU3_Init(initInfo))
                {
                    throw new InvalidOperationException("ImGui_ImplSDLGPU3_Init failed.");
                }
            }
            finally
            {
                Marshal.FreeHGlobal(initInfoPtr);
            }

            initialized = true;
#endif
        }

        public unsafe void ProcessEvent(ref SDL.Event evt)
        {
#if DEBUG
            if (!initialized)
            {
                return;
            }

            fixed (SDL.Event* evtPtr = &evt)
            {
                var swigEvent = new SWIGTYPE_p_SDL_Event((nint)evtPtr, false);
                cimgui_sdlgpu.igImplSDL3_ProcessEvent(swigEvent);
            }
#endif
        }

        public void NewFrame()
        {
#if DEBUG
            if (!initialized || context == null)
            {
                return;
            }

            cimgui_sdlgpu.igSetCurrentContext(context);

            cimgui_sdlgpu.igImplSDLGPU3_NewFrame();
            cimgui_sdlgpu.igImplSDL3_NewFrame();
            cimgui_sdlgpu.igNewFrame();
#endif
        }

        public void RenderUI()
        {
#if DEBUG
            if (!initialized)
            {
                return;
            }

            OnRender?.Invoke();

            cimgui_sdlgpu.igRender();
            currentDrawData = cimgui_sdlgpu.igGetDrawData();
#endif
        }

        public void PrepareDrawData(nint commandBuffer)
        {
#if DEBUG
            if (!initialized || currentDrawData == null || commandBuffer == nint.Zero)
            {
                return;
            }

            var commandBufferPtr = new SWIGTYPE_p_SDL_GPUCommandBuffer(commandBuffer, false);
            cimgui_sdlgpu.igImplSDLGPU3_PrepareDrawData(currentDrawData, commandBufferPtr);
#endif
        }

        public void RenderDrawData(nint commandBuffer, nint renderPass)
        {
#if DEBUG
            if (!initialized || currentDrawData == null || commandBuffer == nint.Zero || renderPass == nint.Zero)
            {
                return;
            }

            var commandBufferPtr = new SWIGTYPE_p_SDL_GPUCommandBuffer(commandBuffer, false);
            var renderPassPtr = new SWIGTYPE_p_SDL_GPURenderPass(renderPass, false);
            cimgui_sdlgpu.igImplSDLGPU3_RenderDrawData(currentDrawData, commandBufferPtr, renderPassPtr);
#endif
        }

        public void Dispose()
        {
#if DEBUG
            if (!initialized)
            {
                return;
            }

            cimgui_sdlgpu.igImplSDLGPU3_Shutdown();
            cimgui_sdlgpu.igImplSDL3_Shutdown();

            if (context != null)
            {
                cimgui_sdlgpu.igDestroyContext(context);
                context = null;
            }

            initialized = false;
#endif
        }
    }
}