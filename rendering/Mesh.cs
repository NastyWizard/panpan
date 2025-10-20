using System.Runtime.InteropServices;
using Game;
using SDL3;

namespace Rendering
{
    public class Mesh
    {

        public nint VertexBuffer
        {
            get { return vertexBuffer; }
        }
        public nint TransferBuffer
        {
            get { return transferBuffer; }
        }

        private nint vertexBuffer;
        private nint transferBuffer;
        private Vertex[] vertices;
        private uint size;

        public Mesh(in Vertex[] _vertices)
        {
            unsafe
            {
                vertices = _vertices;

                var vSize = sizeof(Vertex);
                size = (uint)(vSize * vertices.Length);
                
                SDL.GPUBufferCreateInfo bufferInfo = new SDL.GPUBufferCreateInfo();
                bufferInfo.Size = size;
                bufferInfo.Usage = SDL.GPUBufferUsageFlags.Vertex;
                vertexBuffer = SDL.CreateGPUBuffer(App.GetDevice(), bufferInfo);

                SDL.GPUTransferBufferCreateInfo transferInfo = new SDL.GPUTransferBufferCreateInfo();
                transferInfo.Size = size;
                transferInfo.Usage = SDL.GPUTransferBufferUsage.Upload;
                transferBuffer = SDL.CreateGPUTransferBuffer(App.GetDevice(), transferInfo);

                Vertex* data = (Vertex*)SDL.MapGPUTransferBuffer(App.GetDevice(), transferBuffer, false);
                fixed (Vertex* inData = vertices)
                {
                    Buffer.MemoryCopy(inData, data, size, size);
                }
                SDL.UnmapGPUTransferBuffer(App.GetDevice(), transferBuffer);
            }
        }

        ~Mesh()
        {
            SDL.ReleaseGPUBuffer(App.GetDevice(), vertexBuffer);
            SDL.ReleaseGPUTransferBuffer(App.GetDevice(), transferBuffer);
        }

        public void CopyPass()
        {
            var commandBuffer = SDL.AcquireGPUCommandBuffer(App.GetDevice());
            var copyPass = SDL.BeginGPUCopyPass(commandBuffer);

            SDL.GPUTransferBufferLocation location = new SDL.GPUTransferBufferLocation();
            location.TransferBuffer = TransferBuffer;
            location.Offset = 0;

            SDL.GPUBufferRegion region = new SDL.GPUBufferRegion();
            region.Buffer = VertexBuffer;
            region.Size = size;
            region.Offset = 0;

            SDL.UploadToGPUBuffer(copyPass, location, region, true);

            SDL.EndGPUCopyPass(copyPass);
            SDL.SubmitGPUCommandBuffer(commandBuffer);
        }
    }
}