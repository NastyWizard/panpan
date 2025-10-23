
using System.ComponentModel;
using panpan;
using panpan.Scene;
using SDL3;

namespace panpan.Rendering
{
    public class MeshRenderer : Scene.Component
    {
        private Mesh mesh;
        private Material material;
        public MeshRenderer(Mesh _mesh, Material _mat)
        {
            mesh = _mesh;
            material = _mat;
        }
        public override void Render(nint renderPass)
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
    
            material.UseTexture(renderPass);

            float[] uniforms = new float[8] {
                // time
                SDL.GetTicks() / 1000.0f,
                0.0f, 0.0f, 0.0f, // padding
                // color
                0.0f, 1.0f, 1.0f, 1.0f
            };
            material.SetUniformFloat(uniforms);

            //Parent.Scene.Camera.PushUniformData();

            SDL.DrawGPUIndexedPrimitives(renderPass, mesh.NumIndices, 1, 0, 0, 0);
            
            base.Render(renderPass);
        }

        public override void Init()
        {
            CopyPass();

            base.Init();
        }

        public void SetTexture(Texture tex)
        {
            material.SetTexture(tex);
        }

        private void CopyPass()
        {
            var commandBuffer = SDL.AcquireGPUCommandBuffer(App.GetDevice());
            var copyPass = SDL.BeginGPUCopyPass(commandBuffer);
            
            mesh.CopyPass(copyPass);
            if (material.Texture != null)
                material.Texture.CopyPass(copyPass);
            
            SDL.EndGPUCopyPass(copyPass);
            SDL.SubmitGPUCommandBuffer(commandBuffer);
        }
    }
}