
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

            renderer.SetMesh(Shapes.quad);

            var dist = vec2.Distance(p1, p2);

            renderer.Position = new vec3(p1.xy, 1);
            renderer.Scale = new vec3(10, dist, 1);
            renderer.Angle =  MathF.Atan2(p2.y - p1.y, p2.x - p1.x);

            renderer.Render(renderPass);
        }

        public static void SetRenderPass(nint renderPass)
        {
            Draw.renderPass = renderPass;
        }

        private class BasicRenderer : MeshRenderer
        {

            public vec3 Position, Scale;
            public float Angle;
            public BasicRenderer() : base(Shapes.quad, RenderUtil.DefaultMaterial) { }

            protected override mat4 ComputeModelMatrix()
            {
                var pos =   Position;
                var rot =   Angle;
                var scale = Scale;

                return (mat4.Translate(pos) *
                    mat4.RotateZ(rot) *
                    mat4.Scale(scale) *
                    mat4.Translate(-new vec3(Origin.x, Origin.y, 0))).Transposed;
            }
        }
    }
}
