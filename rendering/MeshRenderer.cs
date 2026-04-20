
using GlmSharp;
using SDL3;
using panpan.Scene;

namespace panpan.Rendering
{
    public delegate void UniformDelegate();
    public class MeshRenderer : Scene.Component
    {
        protected Mesh mesh;
        private mat4 modelMatrix;
        protected Material material;
        protected Texture? texture;
        protected Texture[] additionalTextures = [];
        private UniformDelegate? uniformDelegate;

        public float Width, Height;
        public vec2 Origin;

        protected Transform transform;

        public MeshRenderer(Mesh _mesh, Material _mat)
        {
            mesh = _mesh;
            material = _mat;
            Width = 1;
            Height = 1;
            Origin = vec2.Zero;
            transform = new Transform();
        }
        public override void Render()
        {
            SDL.BindGPUGraphicsPipeline(App.GetRenderPass(), material.Pipeline);
            SDL.GPUBufferBinding[] vertexBufferBindings = new SDL.GPUBufferBinding[1];
            vertexBufferBindings[0].Buffer = mesh.VertexBuffer;
            vertexBufferBindings[0].Offset = 0;

            SDL.GPUBufferBinding indexBufferBinding = new SDL.GPUBufferBinding();
            indexBufferBinding.Buffer = mesh.IndexBuffer;
            indexBufferBinding.Offset = 0;

            SDL.BindGPUVertexBuffers(App.GetRenderPass(), 0, vertexBufferBindings, 1);
            SDL.BindGPUIndexBuffer(App.GetRenderPass(), indexBufferBinding, SDL.GPUIndexElementSize.IndexElementSize32Bit);

            if (texture != null)
            {
                texture.BindTexture(App.GetRenderPass(), 0);
            }

            for (uint i = 0; i < additionalTextures.Length; i++)
            {
                additionalTextures[i].BindTexture(App.GetRenderPass(), i + 1);
            }

            uniformDelegate?.Invoke();

            if (Parent != null)
            {
                transform = Parent.Transform;
            }
            modelMatrix = ComputeModelMatrix();
            unsafe
            {
                fixed (mat4* ptr = &modelMatrix)
                {
                    SDL.PushGPUVertexUniformData(App.GetCommandBuffer(), 1, (nint)ptr, 16 * sizeof(float));
                }
            }

            SDL.DrawGPUIndexedPrimitives(App.GetRenderPass(), mesh.NumIndices, 1, 0, 0, 0);
            
            base.Render();
        }

        public override void Init()
        {
            CopyPass();

            base.Init();
        }

        public void SetTexture(Texture tex)
        {
            texture = tex;
            Width = tex.Width;
            Height = tex.Height;
        }

        public void SetAdditionalTextures(Texture[] tex)
        {
            additionalTextures = tex;
        }

        public void ClearAdditionalTextures()
        {
            additionalTextures = [];
        }

        public void SetMaterial(Material mat)
        {
            material = mat;
        }

        public void SetMesh(Mesh mesh)
        {
            this.mesh = mesh;
            CopyPass();
        }

        public void RegisterSetUniforms(UniformDelegate uniformDelegate)
        {
            this.uniformDelegate = uniformDelegate;
        }

        public void SetUniformFloat(float[] uniforms)
        {
            material.SetUniformFloat(uniforms);
        }

        private void CopyPass()
        {
            mesh.CopyPass();
            texture?.CopyPass();
        }

        protected virtual mat4 ComputeModelMatrix()
        {
            // Snap subpixels - use Floor(x + 0.5) for consistent rounding away from zero
            var pos = new vec3(MathF.Floor(transform.Position.x + 0.5f), MathF.Floor(transform.Position.y + 0.5f), MathF.Floor(transform.Position.z + 0.5f));
            var rot = transform.Angle;
            var scale = transform.Scale * new vec3(Width, Height, 1.0f);

            return (mat4.Translate(pos) *
                mat4.RotateZ(rot) *
                mat4.Scale(scale) *
                mat4.Translate(-new vec3(Origin.x, Origin.y, 0)));
        }

        public void SetTransform(vec3 pos, vec3? scale = null, float? angle = null)
        {
            transform.Position = pos;
            if (scale != null)
                transform.Scale = scale.Value;
            if (angle != null)
                transform.Angle = angle.Value;
        }
    }
}