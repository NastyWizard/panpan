
namespace Rendering
{

    public static class Shapes
    {
        public static Mesh triangle = new Mesh([
                new Vertex(0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f),
                new Vertex(-1.0f, -1.0f, 0.0f, 1.0f, 1.0f, 0.0f, 1.0f),
                new Vertex(1.0f, -1.0f, 0.0f, 1.0f, 0.0f, 1.0f, 1.0f),
            ], [0, 1, 2]);
        public static Mesh quad = new Mesh([
                // Vertex(x, y, z, r, g, b, a)
                new Vertex(-1.0f,  1.0f, 0.0f, 1f, 0f, 0f, 1f), // top-left (v0)
                new Vertex( 1.0f,  1.0f, 0.0f, 0f, 1f, 0f, 1f), // top-right (v1)
                new Vertex(-1.0f, -1.0f, 0.0f, 0f, 0f, 1f, 1f), // bottom-left (v2)
                new Vertex( 1.0f, -1.0f, 0.0f, 1f, 1f, 0f, 1f), // bottom-right (v3)
            ],
            [
                0, 2, 1, // first triangle: top-left → bottom-left → top-right
                1, 2, 3  // second triangle: top-right → bottom-left → bottom-right
            ]
        );
    }

}