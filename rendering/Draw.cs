
using GlmSharp;
using SDL3;
using panpan.Util;

namespace panpan.Rendering
{
    public class Draw
    {
        static nint renderPass;
        static BasicRenderer renderer = new BasicRenderer();


        public static void Line(vec2 p1, vec2 p2, vec4? color = null)
        {
            color ??= Color.White;

            var dist = vec2.Distance(p1, p2);

            renderer.Position = new vec3(p1.xy, 1);
            renderer.Height = (uint)dist;
            renderer.Width = 5;
            renderer.Angle = MathF.Atan2(p2.y - p1.y, p2.x - p1.x);
            renderer.Origin = vec2.UnitY;

            renderer.Render(renderPass);
        }

        public static void SetRenderPass(nint renderPass)
        {
            Draw.renderPass = renderPass;
        }

        private class BasicRenderer : SpriteRenderer
        {

            public vec3 Position, Scale;
            public float Angle;
            public BasicRenderer() : base(new Texture()) { }

            protected override mat4 ComputeModelMatrix()
            {
                var pos =   Position;
                var rot =   Angle;
                var scale = Scale * new vec3(Width/2, Height/2, 1.0f);

                return (mat4.Translate(pos) *
                    mat4.RotateZ(rot) *
                    mat4.Scale(scale) *
                    mat4.Translate(-new vec3(Origin.x, Origin.y, 0))).Transposed;
            }
        }
    }
}
