
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
        private byte[] pixelData;
        public nint GPUTexture => gpuTexture;
        public nint GPUSampler => gpuSampler;
        public uint Width => width;
        public uint Height => height;

        public Texture(byte[] pngData, uint width, uint height)
        {
            this.width = width;
            this.height = height;

            CreateTexture();
            CreateSampler();
            pixelData = DecodePNG(pngData, width, height);
        }

        private void CreateTexture()
        {
            // Create texture info
            SDL.GPUTextureCreateInfo textureCreateInfo = new SDL.GPUTextureCreateInfo();
            textureCreateInfo.Type = SDL.GPUTextureType.Texturetype2D;
            textureCreateInfo.Format = SDL.GPUTextureFormat.R8G8B8A8Unorm; // Use 8-bit per channel format for PNG data
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

        private byte[]? DecodePNG(byte[] pngData, uint width, uint height)
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

        private byte[] CreateTestPattern(uint width, uint height)
        {
            // Create a simple test pattern, pink/black checker
            uint pixelCount = width * height;
            byte[] pd = new byte[pixelCount * 4]; // RGBA format

            for (uint i = 0; i < pixelCount; i++)
            {
                uint pixelIndex = i * 4;
                uint x = i % width;
                uint y = i / width;

                // Create a simple pattern
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

        public void CopyPass(nint copyPass)
        {
            if (pixelData == null)
            {
                // Fallback to test pattern if PNG decoding fails
                CreateTestPattern(width, height);
            }

            var textureDataTransferBuffer = SDL.CreateGPUTransferBuffer(App.GetDevice(), new SDL.GPUTransferBufferCreateInfo
            {
                Usage = SDL.GPUTransferBufferUsage.Upload,
                Size = width * height * 4
            });

            unsafe
            {
                byte* textureTransferBufferPointer = (byte*)SDL.MapGPUTransferBuffer(App.GetDevice(), textureDataTransferBuffer, false);
                fixed (byte* inData = pixelData)
                {
                    Buffer.MemoryCopy(inData, textureTransferBufferPointer, width * height * 4, width * height * 4);
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

            return;
        }

        public void BindTexture(nint renderPass)
        {
            SDL.GPUTextureSamplerBinding[] bindings = new SDL.GPUTextureSamplerBinding[1];
            bindings[0].Texture = GPUTexture;
            bindings[0].Sampler = GPUSampler;
            SDL.BindGPUFragmentSamplers(renderPass, 0, bindings, 1);
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

        ~Texture()
        {
            Dispose();
        }
    }
}