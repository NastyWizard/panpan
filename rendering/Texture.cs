
using panpan;
using SDL3;
using StbImageSharp;

namespace panpan.Rendering
{
    public class Texture
    {
        private nint gpuTexture;
        private nint gpuSampler;
        private uint width;
        private uint height;
        private byte[]? pixelData;
        private SDL.GPUTextureFormat gpuTextureFormat;
        private uint BytesPerPixel =>
        gpuTextureFormat switch
        {
            SDL.GPUTextureFormat.R8Unorm => 1,
            SDL.GPUTextureFormat.R8Int => 1,
            SDL.GPUTextureFormat.R8G8B8A8Unorm => 4,
            _ => throw new NotSupportedException("Unsupported format")
        };
        private bool uploaded = false;
        public nint GPUTexture => gpuTexture;
        public nint GPUSampler => gpuSampler;
        public uint Width => width;
        public uint Height => height;
        public bool HasPixelData => pixelData != null;

        public Texture(byte[]? pngData, uint width, uint height, SDL.GPUTextureFormat gpuTextureFormat = SDL.GPUTextureFormat.R8G8B8A8Unorm, bool isRaw = false)
        {
            this.width = width;
            this.height = height;
            this.gpuTextureFormat = gpuTextureFormat;

            if (pngData != null)
            {
                CreateTexture();
                if(!isRaw)
                {
                    pixelData = DecodePNG(pngData, width, height);
                }
                else
                {
                    pixelData = pngData;
                }
            }
            CreateSampler();
        }

        /// <summary>
        /// Creates a gpu texture.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void CreateTexture()
        {
            // Create texture info
            SDL.GPUTextureCreateInfo textureCreateInfo = new SDL.GPUTextureCreateInfo();
            textureCreateInfo.Type = SDL.GPUTextureType.Texturetype2D;
            textureCreateInfo.Format = gpuTextureFormat;
            textureCreateInfo.Usage = SDL.GPUTextureUsageFlags.Sampler;
            textureCreateInfo.Width = width;
            textureCreateInfo.Height = height;
            textureCreateInfo.LayerCountOrDepth = 1;
            textureCreateInfo.NumLevels = 1;

            gpuTexture = SDL.CreateGPUTexture(App.GetDevice(), textureCreateInfo);
            if (gpuTexture == nint.Zero)
            {
                throw new Exception($"Failed to create GPU texture: {SDL.GetError()}");
            }
        }

        private void CreateSampler()
        {
            SDL.GPUSamplerCreateInfo samplerInfo = new SDL.GPUSamplerCreateInfo();
            samplerInfo.MagFilter = SDL.GPUFilter.Nearest;
            samplerInfo.MinFilter = SDL.GPUFilter.Nearest;
            samplerInfo.MipmapMode = SDL.GPUSamplerMipmapMode.Nearest;
            samplerInfo.AddressModeU = SDL.GPUSamplerAddressMode.ClampToEdge;
            samplerInfo.AddressModeV = SDL.GPUSamplerAddressMode.ClampToEdge;
            samplerInfo.AddressModeW = SDL.GPUSamplerAddressMode.ClampToEdge;

            gpuSampler = SDL.CreateGPUSampler(App.GetDevice(), samplerInfo);
            if (gpuSampler == nint.Zero)
            {
                throw new Exception($"Failed to create GPU sampler: {SDL.GetError()}");
            }
        }

        private byte[] DecodePNG(byte[] pngData, uint width, uint height)
        {
            try
            {
                ImageResult image = ImageResult.FromMemory(pngData, ColorComponents.RedGreenBlueAlpha);
                return image.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PNG decoding failed: {ex.Message}");
                return CreateTestPattern(width, height);
            }
        }

        public static byte[] CreateTestPattern(uint width, uint height)
        {
            // Create a test pattern, pink/black checker
            uint pixelCount = width * height;
            byte[] pd = new byte[pixelCount * 4]; // RGBA format

            for (uint i = 0; i < pixelCount; i++)
            {
                uint pixelIndex = i * 4;
                uint x = i % width;
                uint y = i / width;

                if (x >= width / 2 && y <= height / 2 || x < width / 2 && y > height / 2)
                { // pink
                    pd[pixelIndex] = 255;        // R
                    pd[pixelIndex + 1] = 0;      // G
                    pd[pixelIndex + 2] = 255;    // B
                    pd[pixelIndex + 3] = 255;    // A
                }
                else
                { // black
                    pd[pixelIndex] = 0;        // R
                    pd[pixelIndex + 1] = 0;      // G
                    pd[pixelIndex + 2] = 0;    // B
                    pd[pixelIndex + 3] = 255;    // A
                }
            }
            return pd;
        }

        private byte[] CreateWhiteTex(uint width, uint height)
        {
            uint pixelCount = width * height;
            byte[] pd = new byte[pixelCount * 4];

            for (uint i = 0; i < pixelCount; i++)
            {
                uint pixelIndex = i * 4;

                pd[pixelIndex] = 255;        // R
                pd[pixelIndex + 1] = 255;      // G
                pd[pixelIndex + 2] = 255;    // B
                pd[pixelIndex + 3] = 255;    // A
            }
            return pd;
        }

        /// <summary>
        /// Used to manually set a gpu texture, e.g. when using a <c>RenderTarget</c>.
        /// </summary>
        /// <param name="tex">GPU texture</param>
        public void SetGPUTexture(nint tex)
        {
            this.gpuTexture = tex;
        }

        public void Resize(uint w, uint h)
        {
            width = w;
            height = h;
        }

        public void CopyPass()
        {
            if (uploaded || pixelData == null)
            {
                return;
            }

            var commandBuffer = SDL.AcquireGPUCommandBuffer(App.GetDevice());
            var copyPass = SDL.BeginGPUCopyPass(commandBuffer);

            var textureDataTransferBuffer = SDL.CreateGPUTransferBuffer(App.GetDevice(), new SDL.GPUTransferBufferCreateInfo
            {
                Usage = SDL.GPUTransferBufferUsage.Upload,
                Size = width * height * BytesPerPixel
            });

            unsafe
            {
                byte* textureTransferBufferPointer = (byte*)SDL.MapGPUTransferBuffer(App.GetDevice(), textureDataTransferBuffer, false);
                fixed (byte* inData = pixelData)
                {
                    ulong size = width * height * BytesPerPixel;
                    Buffer.MemoryCopy(inData, textureTransferBufferPointer, size, size);
                }
            }
            SDL.UnmapGPUTransferBuffer(App.GetDevice(), textureDataTransferBuffer);

            SDL.UploadToGPUTexture(copyPass, new SDL.GPUTextureTransferInfo
            {
                TransferBuffer = textureDataTransferBuffer,
                Offset = 0
            }, new SDL.GPUTextureRegion
            {
                Texture = GPUTexture,
                W = width,
                H = height,
                D = 1
            }, false);

            SDL.EndGPUCopyPass(copyPass);
            SDL.SubmitGPUCommandBuffer(commandBuffer);

            uploaded = true;

            return;
        }

        public void BindTexture(nint renderPass)
        {
            BindTexture(renderPass, 0);
        }

        public void BindTexture(nint renderPass, uint slot)
        {
            SDL.GPUTextureSamplerBinding[] bindings = new SDL.GPUTextureSamplerBinding[1];
            bindings[0].Texture = GPUTexture;
            bindings[0].Sampler = GPUSampler;
            SDL.BindGPUFragmentSamplers(renderPass, slot, bindings, 1);
        }

        public void Dispose()
        {
            if (gpuSampler != nint.Zero)
            {
                SDL.ReleaseGPUSampler(App.GetDevice(), gpuSampler);
                gpuSampler = nint.Zero;
            }

            if (gpuTexture != nint.Zero)
            {
                SDL.ReleaseGPUTexture(App.GetDevice(), gpuTexture);
                gpuTexture = nint.Zero;
            }
        }

        public bool TryGetPixel(int x, int y, out byte r, out byte g, out byte b, out byte a)
        {
            r = g = b = a = 0;
            if (pixelData == null)
            {
                return false;
            }
            if (x < 0 || y < 0 || x >= (int)width || y >= (int)height)
            {
                return false;
            }

            long index = ((long)y * width + x) * 4L;

            if (index + 3 >= pixelData.Length)
            {
                return false;
            }

            int idx = (int)index;
            r = pixelData[idx];
            g = pixelData[idx + 1];
            b = pixelData[idx + 2];
            a = pixelData[idx + 3];
            return true;
        }

        ~Texture()
        {
            Dispose();
        }
    }
}