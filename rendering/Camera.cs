
using panpan;
using GlmSharp;
using panpan.Scene;
using SDL3;

namespace panpan.Rendering
{
    public class Camera : Entity
    {
        private mat4 projection;
        private mat4 viewProjection;
        private uint width, height;
        public uint Width => width;
        public uint Height => height;

        public Camera(uint width, uint height)
        {
            SetBounds(width, height);
        }

        public override void Init()
        {
            Position = vec3.UnitZ;
            Update();
            base.Init();
        }

        public override void Update()
        {
            base.Update();
            mat4 view = mat4.LookAt(Position, Position - vec3.UnitZ, vec3.UnitY);
            viewProjection = (projection * view).Transposed;
        }

        public void PushUniformData()
        {
            unsafe
            {
                fixed (mat4* ptr = &viewProjection)
                {
                    SDL.PushGPUVertexUniformData(App.GetCommandBuffer(), 0, (nint)ptr, 16 * sizeof(float));
                }
            }
        }

        public void SetBounds(uint w, uint h)
        {
            width = w;
            height = h;
            projection = mat4.Ortho(-width / 2, width / 2, -height / 2, height / 2, 0.1f, 100.0f);
        }

        public vec2 GetBounds()
        {
            return new vec2(width, height);
        }

        public mat4 GetViewProjectionMatrix() { return viewProjection; }
    }
}