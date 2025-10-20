
using SDL3;

namespace Rendering
{
    public class MeshRenderer
    {
        private Mesh mesh;
        private Material material;
        public MeshRenderer(Mesh _mesh, Material _mat)
        {
            mesh = _mesh;
            material = _mat;
        }
        public void Draw(nint renderPass)
        {
            SDL.BindGPUGraphicsPipeline(renderPass, material.Pipeline);
            SDL.GPUBufferBinding[] vertexBufferBindings = new SDL.GPUBufferBinding[1];
            vertexBufferBindings[0].Buffer = mesh.VertexBuffer;
            vertexBufferBindings[0].Offset = 0;

            SDL.GPUBufferBinding indexBufferBinding = new SDL.GPUBufferBinding();
            indexBufferBinding.Buffer = mesh.IndexBuffer;
            indexBufferBinding.Offset = 0;

            SDL.BindGPUVertexBuffers(renderPass, 0, vertexBufferBindings, 1);
            SDL.BindGPUIndexBuffer(renderPass, indexBufferBinding, SDL.GPUIndexElementSize.IndexElementSize32Bit);

            float[] uniforms = new float[8] {
                // time
                SDL.GetTicks() / 1000.0f,
                0.0f, 0.0f, 0.0f, // padding
                // color
                0.0f, 1.0f, 1.0f, 1.0f
            };
            material.SetUniformFloat(uniforms);

            SDL.DrawGPUIndexedPrimitives(renderPass, mesh.NumIndices, 1, 0, 0, 0);
        }
    }
}