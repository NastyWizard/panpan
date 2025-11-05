

using GlmSharp;

namespace panpan.Scene
{
    public class Transform
    {
        public vec3 Position;
        public vec3 Scale;
        public float Angle;

        public Transform()
        {
            Position = vec3.Zero;
            Scale = vec3.Ones;
            Angle = 0;
        }
    }
}