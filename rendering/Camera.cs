
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
            this.width = width;
            this.height = height;
        }

        public override void Init()
        {
            Position = vec3.Zero;
            projection = mat4.Ortho(-width / 2, width / 2, -height / 2, height / 2);
            Update();
            base.Init();
        }

        public override void Update()
        {
            base.Update();
            mat4 view = mat4.LookAt(Position, Position + vec3.UnitZ, vec3.UnitY);
            viewProjection = projection * view;
            viewProjection = mat4.Identity;
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
    }
}