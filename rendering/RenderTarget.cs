
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
        nint depthStencilTexture = nint.Zero;
        Texture texture;
        nint renderPass;
        vec4 clearColor;

        bool doesClear = true;
        bool hasDepth;

        public uint Width, Height;

        public RenderTarget(uint width, uint height, vec4? clearColor = null, bool hasDepth = false)
        {
            texture = new Texture(null, width, height);
            clearColor ??= vec4.Zero;
            this.clearColor = clearColor.Value;
            gpuTexture = null;
            this.hasDepth = hasDepth;
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
            if (gpuTexture is null)
            {
                throw new InvalidOperationException("Render target GPU texture has not been created.");
            }

            SDL.GPUColorTargetInfo colorTargetInfo = new SDL.GPUColorTargetInfo
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

            SDL.GPUDepthStencilTargetInfo depthInfo = new SDL.GPUDepthStencilTargetInfo
            {
                Texture = depthStencilTexture,
                LoadOp = SDL.GPULoadOp.Clear,
                StoreOp = SDL.GPUStoreOp.Store,
                StencilLoadOp = SDL.GPULoadOp.Clear,
                StencilStoreOp = SDL.GPUStoreOp.Store,
                ClearDepth = 1,
                ClearStencil = 0,
                Cycle = 1
            };

            nint pColorInfo = SDL.StructureToPointer<SDL.GPUColorTargetInfo>(colorTargetInfo);
            nint pDepthInfo = SDL.StructureToPointer<SDL.GPUDepthStencilTargetInfo>(depthInfo);
            
            renderPass = SDL.BeginGPURenderPass(
                App.GetCommandBuffer(),
                pColorInfo,
                1,
                hasDepth ? pDepthInfo : nint.Zero
            );
            Marshal.FreeHGlobal(pColorInfo);
            Marshal.FreeHGlobal(pDepthInfo);
            return renderPass;
        }

        /// <summary>
        /// Resize the render target, should only be done before being used in <c>App.SetRenderTarget</c>.
        /// </summary>
        public void Resize(uint width, uint height)
        {
            this.Width = width;
            this.Height = height;

            if (gpuTexture.HasValue)
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


            if(hasDepth)
            {
                SDL.GPUTextureCreateInfo depthTextureCreateInfo = new SDL.GPUTextureCreateInfo
                {
                    Type = SDL.GPUTextureType.Texturetype2D,
                    Width = width,
                    Height = height,
                    LayerCountOrDepth = 1,
                    NumLevels = 1,
                    SampleCount = SDL.GPUSampleCount.SampleCount1,
                    Format = SDL.GPUTextureFormat.D16Unorm,
                    Usage = SDL.GPUTextureUsageFlags.Sampler | SDL.GPUTextureUsageFlags.DepthStencilTarget
                };

                depthStencilTexture = SDL.CreateGPUTexture(App.GetDevice(), depthTextureCreateInfo);

                if (depthStencilTexture == nint.Zero)
                {
                    throw new Exception($"Failed to create depth texture for Render Target: {SDL.GetError()}");
                }
                else
                {
                    Log.Info("Created depth texture");
                }
            }

            var handle = SDL.CreateGPUTexture(App.GetDevice(), textureCreateInfo);
            if (handle == nint.Zero)
            {
                throw new Exception($"Failed to create color texture for Render Target: {SDL.GetError()}");
            }
            gpuTexture = handle;
            texture.SetGPUTexture(handle);
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