
using GlmSharp;
using panpan.Util;
using panpan.Assets;

namespace panpan.Rendering
{
    public class Draw
    {
        static nint renderPass;
        static BasicRenderer renderer = new BasicRenderer();
        static vec4 color;

        public static void Rect(vec2 bl, vec2 size, vec4? color = null)
        {
            color ??= Color.White;
            Draw.color = color.Value;

            Draw.Line(bl, bl + size * vec2.UnitX, color); // bl - br
            Draw.Line(bl, bl + size * vec2.UnitY, color); // bl - tl
            Draw.Line(bl + size * vec2.UnitX, bl + size * vec2.UnitY + size * vec2.UnitX + vec2.UnitY, color); // br - tr
            Draw.Line(bl + size * vec2.UnitY, bl + size * vec2.UnitY + size * vec2.UnitX, color); // tl - tr
            
        }
        public static void Line(vec2 p1, vec2 p2, vec4? color = null)
        {
            color ??= Color.White;
            Draw.color = color.Value;

            p1.x = MathF.Round(p1.x);
            p1.y = MathF.Round(p1.y);
            p2.x = MathF.Round(p2.x);
            p2.y = MathF.Round(p2.y);

            var current = p1;

            while (vec2.Distance(current.xy, p2.xy) >= 1f)
            {
                Draw.Dot(current.xy, color);

                var dir = p2 - current;
                current += dir.Normalized;
            }
            return;

            // faster non-pixel perfect line
            var dist = vec2.Distance(p1, p2);

            renderer.Position = new vec3(p1.xy, 1);
            renderer.Scale = vec3.Ones;
            renderer.Width = 1f;
            renderer.Height = (uint)dist;
            renderer.Angle = MathF.Atan2(p2.y - p1.y, p2.x - p1.x) + PMath.DegToRad(90);
            renderer.Origin = vec2.UnitY;

            renderer.Render(renderPass);
        }
        public static void Dot(vec2 p1, vec4? color = null)
        {
            color ??= Color.White;
            Draw.color = color.Value;

            renderer.Position = new vec3(MathF.Round(p1.x), MathF.Round(p1.y), 1);
            renderer.Scale = vec3.Ones;
            renderer.Width = 1f;
            renderer.Height = 1f;
            renderer.Angle = 0;
            renderer.Origin = vec2.Zero;

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
            public BasicRenderer() : base(Shapes.quad, new Material(Shaders.standardNoTex_frag_hlsl,Shaders.standard_vert_hlsl))
            {
                RegisterSetUniforms(SetUniforms);
            }

            private void SetUniforms()
            {
                float[] uniforms = new float[4]
                {
                    color.r, color.g, color.b, color.a
                };

                SetUniformFloat(uniforms);
            }

            protected override mat4 ComputeModelMatrix()
            {
                var pos =   Position;
                var rot =   Angle;
                var scale = Scale * new vec3(Width, Height, 1.0f);

                return (mat4.Translate(pos) *
                    mat4.RotateZ(rot) *
                    mat4.Scale(scale) *
                    mat4.Translate(-new vec3(Origin.x, Origin.y, 0))).Transposed;
            }
        }
    }
}
