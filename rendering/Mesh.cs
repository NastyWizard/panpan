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

        public nint IndexBuffer
        {
            get { return indexBuffer; }
        }

        public uint NumIndices
        {
            get { return (uint)indices.Length; }
        }

        private nint vertexBuffer;
        private nint indexBuffer;
        private nint vertexTransferBuffer;
        private nint indexTransferBuffer;
        private Vertex[] vertices;
        private uint[] indices;
        private uint vertexSize;
        private uint indexSize;

        public Mesh(in Vertex[] _vertices, in uint[] _indices)
        {
            HandleVerticies(_vertices);
            HandleIndices(_indices);
            CopyPass();
        }

        ~Mesh()
        {
            SDL.ReleaseGPUBuffer(App.GetDevice(), vertexBuffer);
            SDL.ReleaseGPUBuffer(App.GetDevice(), indexBuffer);
            SDL.ReleaseGPUTransferBuffer(App.GetDevice(), vertexTransferBuffer);
            SDL.ReleaseGPUTransferBuffer(App.GetDevice(), indexTransferBuffer);
        }


        private void HandleVerticies(in Vertex[] _vertices)
        {
            vertices = _vertices;

            unsafe
            {
                var vSize = sizeof(Vertex);
                vertexSize = (uint)(vSize * vertices.Length);
            }

            // Vertex Buff
            SDL.GPUBufferCreateInfo vertexBufferInfo = new SDL.GPUBufferCreateInfo();
            vertexBufferInfo.Size = vertexSize;
            vertexBufferInfo.Usage = SDL.GPUBufferUsageFlags.Vertex;
            vertexBuffer = SDL.CreateGPUBuffer(App.GetDevice(), vertexBufferInfo);

            // Transfer buffer
            SDL.GPUTransferBufferCreateInfo transferInfo = new SDL.GPUTransferBufferCreateInfo();
            transferInfo.Size = vertexSize;
            transferInfo.Usage = SDL.GPUTransferBufferUsage.Upload;
            vertexTransferBuffer = SDL.CreateGPUTransferBuffer(App.GetDevice(), transferInfo);

            unsafe
            {
                Vertex* data = (Vertex*)SDL.MapGPUTransferBuffer(App.GetDevice(), vertexTransferBuffer, false);
                fixed (Vertex* inData = vertices)
                {
                    Buffer.MemoryCopy(inData, data, vertexSize, vertexSize);
                }
            }
            SDL.UnmapGPUTransferBuffer(App.GetDevice(), vertexTransferBuffer);
        }

        private void HandleIndices(in uint[] _indicies)
        {
            indices = _indicies;
            indexSize = (uint)(sizeof(uint) * _indicies.Length);
            // Index Buffer
            SDL.GPUBufferCreateInfo indexBufferInfo = new SDL.GPUBufferCreateInfo();
            indexBufferInfo.Size = indexSize;
            indexBufferInfo.Usage = SDL.GPUBufferUsageFlags.Index;
            indexBuffer = SDL.CreateGPUBuffer(App.GetDevice(), indexBufferInfo);

            // Transfer buffer
            SDL.GPUTransferBufferCreateInfo transferInfo = new SDL.GPUTransferBufferCreateInfo();
            transferInfo.Size = indexSize;
            transferInfo.Usage = SDL.GPUTransferBufferUsage.Upload;
            indexTransferBuffer = SDL.CreateGPUTransferBuffer(App.GetDevice(), transferInfo);

            unsafe
            {
                uint* data = (uint*)SDL.MapGPUTransferBuffer(App.GetDevice(), indexTransferBuffer, false);
                fixed (uint* inData = indices)
                {
                    Buffer.MemoryCopy(inData, data, indexSize, indexSize);
                }
            }
            SDL.UnmapGPUTransferBuffer(App.GetDevice(), indexTransferBuffer);
        }
        private void CopyPass()
        {
            var commandBuffer = SDL.AcquireGPUCommandBuffer(App.GetDevice());
            var copyPass = SDL.BeginGPUCopyPass(commandBuffer);

            // Upload Verticies
            SDL.GPUTransferBufferLocation vertexLocation = new SDL.GPUTransferBufferLocation();
            vertexLocation.TransferBuffer = vertexTransferBuffer;
            vertexLocation.Offset = 0;

            SDL.GPUBufferRegion vertexRegion = new SDL.GPUBufferRegion();
            vertexRegion.Buffer = vertexBuffer;
            vertexRegion.Size = vertexSize;
            vertexRegion.Offset = 0;

            SDL.UploadToGPUBuffer(copyPass, vertexLocation, vertexRegion, true);

            // Upload Indices
            SDL.GPUTransferBufferLocation indexLocation = new SDL.GPUTransferBufferLocation();
            indexLocation.TransferBuffer = indexTransferBuffer;
            indexLocation.Offset = 0;

            SDL.GPUBufferRegion indexRegion = new SDL.GPUBufferRegion();
            indexRegion.Buffer = indexBuffer;
            indexRegion.Size = indexSize;
            indexRegion.Offset = 0;

            SDL.UploadToGPUBuffer(copyPass, indexLocation, indexRegion, true);

            SDL.EndGPUCopyPass(copyPass);
            SDL.SubmitGPUCommandBuffer(commandBuffer);
        }
    }
}