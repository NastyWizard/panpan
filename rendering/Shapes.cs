
using GlmSharp;
using panpan.Util;

namespace panpan.Rendering
{

    public static class Shapes
    {
        public static Mesh triangle = new Mesh([
                //          x      y     z     r     g     b     a 
                new Vertex( 0.0f,  1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f),
                new Vertex(-1.0f, -1.0f, 0.0f, 1.0f, 1.0f, 0.0f, 1.0f),
                new Vertex( 1.0f, -1.0f, 0.0f, 1.0f, 0.0f, 1.0f, 1.0f),
            ], [0, 1, 2]);
        public static Mesh quad = new Mesh([
                //         x       y     z     r   g   b   a   u   v
                new Vertex(-0.0f,  0.0f, 0.0f, 1f, 1f, 1f, 1f, 0f, 0f), // top-left
                new Vertex( 1.0f,  0.0f, 0.0f, 1f, 1f, 1f, 1f, 1f, 0f), // top-right
                new Vertex(-0.0f, -1.0f, 0.0f, 1f, 1f, 1f, 1f, 0f, 1f), // bottom-left
                new Vertex( 1.0f, -1.0f, 0.0f, 1f, 1f, 1f, 1f, 1f, 1f), // bottom-right
            ],
            [
                0, 2, 1, // first triangle
                1, 2, 3  // second triangle
            ]
        );
        public static Mesh fsQuad = new Mesh([
                //         x       y     z     r   g   b   a   u   v
                new Vertex(-1.0f,  1.0f, 0.0f, 1f, 1f, 1f, 1f, 0f, 0f), // top-left
                new Vertex( 1.0f,  1.0f, 0.0f, 1f, 1f, 1f, 1f, 1f, 0f), // top-right
                new Vertex(-1.0f, -1.0f, 0.0f, 1f, 1f, 1f, 1f, 0f, 1f), // bottom-left
                new Vertex( 1.0f, -1.0f, 0.0f, 1f, 1f, 1f, 1f, 1f, 1f), // bottom-right
            ],
            [
                0, 2, 1, // first triangle
                1, 2, 3  // second triangle
            ]
        );
        public static Mesh line = new Mesh([
                //         x       y     z     r   g   b   a   u   v
                new Vertex(0.0f,  1.0f, 0.0f, 1f, 1f, 1f, 1f, 0f, 0f), // top
                new Vertex(0.0f,  0.0f, 0.0f, 1f, 1f, 1f, 1f, 0f, 1f), // bottom
            ],
            [
                0, 1
            ]
        );

        public static Mesh ClipQuad(Rect rect, float totalWidth, float totalHeight)
        {
            vec4 clipBox = new vec4(rect.X / totalWidth, rect.Y / totalHeight, rect.Width / totalWidth, rect.Height / totalHeight);

            var q = new Mesh([
                    //         x       y     z     r   g   b   a   u   v
                    new Vertex(-0.0f,  0.0f, 0.0f, 1f, 1f, 1f, 1f, clipBox.x, clipBox.y), // top-left
                    new Vertex( 1.0f,  0.0f, 0.0f, 1f, 1f, 1f, 1f, clipBox.x + clipBox.z, clipBox.y), // top-right
                    new Vertex(-0.0f, -1.0f, 0.0f, 1f, 1f, 1f, 1f, clipBox.x, clipBox.y + clipBox.w), // bottom-left
                    new Vertex( 1.0f, -1.0f, 0.0f, 1f, 1f, 1f, 1f, clipBox.x + clipBox.z, clipBox.y + clipBox.w), // bottom-right
                ],
                [
                    0, 2, 1, // first triangle
                    1, 2, 3  // second triangle
                ]
            );
            q.CopyPass();
            return q;
        }

        public static void Init()
        {
            triangle.CopyPass();
            quad.CopyPass();
            fsQuad.CopyPass();
            line.CopyPass();
        }
    }

}