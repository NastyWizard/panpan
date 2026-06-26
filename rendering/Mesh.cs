using System;
using System.Runtime.InteropServices;
using GlmSharp;
using panpan;
using panpan.Util;
using SDL3;

namespace panpan.Rendering
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
        private Vertex[] vertices = Array.Empty<Vertex>();
        private uint[] indices = Array.Empty<uint>();
        private uint vertexSize;
        private uint indexSize;
        private bool uploaded = false;

        public Mesh(in Vertex[] _vertices, in uint[] _indices)
        {
            HandleVerticies(_vertices);
            HandleIndices(_indices);
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
            if (vertexBuffer != nint.Zero)
            {
                SDL.ReleaseGPUBuffer(App.GetDevice(), vertexBuffer);
                vertexBuffer = nint.Zero;
            }
            if (vertexTransferBuffer != nint.Zero)
            {
                SDL.ReleaseGPUTransferBuffer(App.GetDevice(), vertexTransferBuffer);
                vertexTransferBuffer = nint.Zero;
            }

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

            TransferBuffer(vertices, vertexSize, vertexTransferBuffer);
            // unsafe
            // {
            //     Vertex* data = (Vertex*)SDL.MapGPUTransferBuffer(App.GetDevice(), vertexTransferBuffer, false);
            //     fixed (Vertex* inData = vertices)
            //     {
            //         Buffer.MemoryCopy(inData, data, vertexSize, vertexSize);
            //     }
            // }
            // SDL.UnmapGPUTransferBuffer(App.GetDevice(), vertexTransferBuffer);
        }

        private void HandleIndices(in uint[] _indicies)
        {
            if (indexBuffer != nint.Zero)
            {
                SDL.ReleaseGPUBuffer(App.GetDevice(), indexBuffer);
                indexBuffer = nint.Zero;
            }
            if (indexTransferBuffer != nint.Zero)
            {
                SDL.ReleaseGPUTransferBuffer(App.GetDevice(), indexTransferBuffer);
                indexTransferBuffer = nint.Zero;
            }

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

            TransferBuffer(indices, indexSize, indexTransferBuffer);
            // unsafe
            // {
            //     uint* data = (uint*)SDL.MapGPUTransferBuffer(App.GetDevice(), indexTransferBuffer, false);
            //     fixed (uint* inData = indices)
            //     {
            //         Buffer.MemoryCopy(inData, data, indexSize, indexSize);
            //     }
            // }
            // SDL.UnmapGPUTransferBuffer(App.GetDevice(), indexTransferBuffer);
        }

        private void TransferBuffer<T>(in T[] inData, uint size, nint transferBuffer) where T : unmanaged
        {
            unsafe
            {
                var mappedPtr = (T*)SDL.MapGPUTransferBuffer(App.GetDevice(), transferBuffer, false);
                if (mappedPtr == null)
                {
                    throw new InvalidOperationException("Failed to map GPU transfer buffer.");
                }

                fixed (T* d = inData)
                {
                    Buffer.MemoryCopy(d, mappedPtr, size, size);
                }
            }
            SDL.UnmapGPUTransferBuffer(App.GetDevice(), transferBuffer);
        }

        public void CopyPass()
        {
            if (uploaded)
            {
                return;
            }

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
            uploaded = true;
        }

        public void SetData(Vertex[] newVertices, uint[] newIndices)
        {
            if (newVertices == null || newIndices == null || newVertices.Length == 0 || newIndices.Length == 0)
            {
                return;
            }

            HandleVerticies(newVertices);
            HandleIndices(newIndices);
            uploaded = false;
            CopyPass();
        }

        public void Clip(Rect rect, float totalWidth, float totalHeight)
        {
            vec4 clipBox = new vec4(rect.X / totalWidth, rect.Y / totalHeight, rect.Width / totalWidth, rect.Height / totalHeight);

            vertices[0].uv = new vec2(clipBox.x, clipBox.y);
            vertices[1].uv = new vec2(clipBox.x + clipBox.z, clipBox.y);
            vertices[2].uv = new vec2(clipBox.x, clipBox.y + clipBox.w);
            vertices[3].uv = new vec2(clipBox.x + clipBox.z, clipBox.y + clipBox.w);

            TransferBuffer(vertices, vertexSize, vertexTransferBuffer);
            uploaded = false;
            CopyPass();
        }
        
        public Mesh Clone()
        {
            Mesh clone = new (vertices, indices);
            return clone;
        }
    }
}