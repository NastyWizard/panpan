
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
                new Vertex(-0.0f,  1.0f, 0.0f, 1f, 1f, 1f, 1f, 0f, 0f), // top-left
                new Vertex( 1.0f,  1.0f, 0.0f, 1f, 1f, 1f, 1f, 1f, 0f), // top-right
                new Vertex(-0.0f, -0.0f, 0.0f, 1f, 1f, 1f, 1f, 0f, 1f), // bottom-left
                new Vertex( 1.0f, -0.0f, 0.0f, 1f, 1f, 1f, 1f, 1f, 1f), // bottom-right
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
    }

}