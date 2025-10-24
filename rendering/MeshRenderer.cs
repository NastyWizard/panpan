
using System.ComponentModel;
using GlmSharp;
using panpan;
using panpan.Scene;
using SDL3;

namespace panpan.Rendering
{
    public class MeshRenderer : Scene.Component
    {
        private Mesh mesh;
        private mat4 modelMatrix;
        protected Material material;
        protected Texture texture;

        public uint Width, Height;

        public MeshRenderer(Mesh _mesh, Material _mat)
        {
            mesh = _mesh;
            material = _mat;
            Width = 1;
            Height = 1;
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

            if (texture != null)
            {
                texture.BindTexture(renderPass);
            }

            float[] uniforms = new float[8] {
                // time
                SDL.GetTicks() / 1000.0f,
                0.0f, 0.0f, 0.0f, // padding
                // color
                0.0f, 1.0f, 1.0f, 1.0f
            };
            material.SetUniformFloat(uniforms);

            var scale = Parent.Scale;
            scale.x *= Width;
            scale.y *= Height;
            modelMatrix = mat4.Translate(Parent.Position) * mat4.RotateZ(Parent.Angle) * mat4.Scale(scale);
            modelMatrix = modelMatrix.Transposed;
            unsafe
            {
                fixed (mat4* ptr = &modelMatrix)
                {
                    SDL.PushGPUVertexUniformData(App.GetCommandBuffer(), 1, (nint)ptr, 16 * sizeof(float));
                }
            }

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
            texture = tex;
        }

        private void CopyPass()
        {
            var commandBuffer = SDL.AcquireGPUCommandBuffer(App.GetDevice());
            var copyPass = SDL.BeginGPUCopyPass(commandBuffer);
            
            mesh.CopyPass(copyPass);
            if (texture != null)
                texture.CopyPass(copyPass);
            
            SDL.EndGPUCopyPass(copyPass);
            SDL.SubmitGPUCommandBuffer(commandBuffer);
        }
    }
}