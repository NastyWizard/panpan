
using panpan;
using GlmSharp;
using panpan.Scene;
using SDL3;
using panpan.Util;

namespace panpan.Rendering
{
    public class Camera : Entity
    {
        private float near = -200.0f;
        private float far = 200.0f;
        private mat4 projection;
        private mat4 viewProjection;
        private uint width, height;
        public uint Width => width;
        public uint Height => height;
        public Rect Bounds;

        public float Zoom = 1.0f;

        public Camera(int x, int y, uint width, uint height)
        {
            Transform.Position.x = x;
            Transform.Position.y = y;
            SetBounds(width, height);
        }

        public override void Init()
        {
            Position = vec3.UnitZ * 10;
            Update();
            base.Init();
        }

        public override void Update()
        {
            base.Update();
            mat4 view = mat4.LookAt(Position, Position - vec3.UnitZ, vec3.UnitY);
            viewProjection = (projection * view);
            UpdateBounds();
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
            UpdateProjection();
        }

        private void UpdateBounds()
        {
            Bounds = new Rect((int)(Transform.Position.x - (width*Zoom)/2),(int)(Transform.Position.y - (height*Zoom)/2), (int)(width * Zoom), (int)(height * Zoom));
        }

        public void UpdateProjection()
        {
            projection = mat4.Ortho(-(width * Zoom) / 2, (width * Zoom) / 2, -(height * Zoom) / 2, (height * Zoom) / 2, near, far); 
            UpdateBounds();
        }

        public vec2 GetBounds()
        {
            return new vec2(width, height);
        }

        public bool IsPointInView(vec2 pos)
        {
            return Bounds.IntersectsPosition(pos);
        }
        public bool IsRectInView(Rect rect)
        {
            return Bounds.Intersects(rect);
        }

        public mat4 GetViewProjectionMatrix() { return viewProjection; }
    }
}