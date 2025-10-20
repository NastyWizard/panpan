
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

        struct Uniforms
        {
            public float time;
        }
        public void Draw(nint renderPass)
        {
            mesh.CopyPass();
            SDL.BindGPUGraphicsPipeline(renderPass, material.Pipeline);
            SDL.GPUBufferBinding[] gpuBufferBindings = new SDL.GPUBufferBinding[1];
            gpuBufferBindings[0].Buffer = mesh.VertexBuffer;
            gpuBufferBindings[0].Offset = 0;

            SDL.BindGPUVertexBuffers(renderPass, 0, gpuBufferBindings, 1);
            // Uniforms unf = new Uniforms();
            // unf.time = SDL.GetTicks() / 1000.0f;
            // material.SetUniforms<Uniforms>(unf);

            SDL.DrawGPUPrimitives(renderPass, 3, 1, 0, 0);
        }
    }
}