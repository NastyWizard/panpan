
using System.Runtime.InteropServices;
using GlmSharp;
using panpan;
using SDL3;
using StbImageSharp;

namespace panpan.Rendering
{
    public class RenderTarget
    {
        nint? gpuTexture;
        Texture texture;
        nint renderPass;
        vec4 clearColor;

        bool doesClear = true;

        public uint Width, Height;

        public RenderTarget(uint width, uint height, vec4? clearColor = null)
        {
            texture = new Texture(null, width, height);
            clearColor ??= vec4.Zero;
            this.clearColor = clearColor.Value;
            gpuTexture = null;
            // Create the texture
            Resize(width, height);
        }

        ~RenderTarget()
        {
            if (gpuTexture != null)
            {
                SDL.ReleaseGPUTexture(App.GetDevice(), gpuTexture.Value);
            }
            gpuTexture = null;
        }

        public Texture GetTexture()
        {
            return texture;
        }

        /// <summary>
        /// Used in <c>App.SetRenderTarget</c> to set the active render target.
        /// </summary>
        public nint CreateRenderPass()
        {
            var colorTargetInfo = new SDL.GPUColorTargetInfo
            {
                Texture = gpuTexture.Value,
                LoadOp = doesClear ? SDL.GPULoadOp.Clear : SDL.GPULoadOp.Load,
                StoreOp = SDL.GPUStoreOp.Store,
                ClearColor = new SDL.FColor
                {
                    R = clearColor.r,
                    G = clearColor.g,
                    B = clearColor.b,
                    A = clearColor.a
                }
            };

            var ptr = SDL.StructureToPointer<SDL.GPUColorTargetInfo>(colorTargetInfo);
            renderPass = SDL.BeginGPURenderPass(
                App.GetCommandBuffer(),
                ptr,
                1,
                nint.Zero
            );
            Marshal.FreeHGlobal(ptr);
            return renderPass;
        }

        /// <summary>
        /// Resize the render target, should only be done before being used in <c>App.SetRenderTarget</c>.
        /// </summary>
        public void Resize(uint width, uint height)
        {
            this.Width = width;
            this.Height = height;

            if (gpuTexture != null)
            {
                SDL.ReleaseGPUTexture(App.GetDevice(), gpuTexture.Value);
            }

            // Create texture info
            SDL.GPUTextureCreateInfo textureCreateInfo = new SDL.GPUTextureCreateInfo();
            textureCreateInfo.Type = SDL.GPUTextureType.Texturetype2D;
            textureCreateInfo.Format = SDL.GPUTextureFormat.B8G8R8A8Unorm;
            textureCreateInfo.Usage = SDL.GPUTextureUsageFlags.ColorTarget | SDL.GPUTextureUsageFlags.Sampler;
            textureCreateInfo.Width = width;
            textureCreateInfo.Height = height;
            textureCreateInfo.LayerCountOrDepth = 1;
            textureCreateInfo.NumLevels = 1;

            gpuTexture = SDL.CreateGPUTexture(App.GetDevice(), textureCreateInfo);
            if (gpuTexture == nint.Zero)
            {
                throw new Exception($"Failed to create GPU texture for Render Target: {SDL.GetError()}");
            }
            texture.SetGPUTexture(gpuTexture.Value);
        }

        public void SetDoesClear(bool doesClear)
        {
            this.doesClear = doesClear;
        }

        public void SetClearColor(vec4 col)
        {
            clearColor = col;
        }
    }
}