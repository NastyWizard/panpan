
namespace panpan.Rendering
{
    public struct Vertex
    {
        public float x, y, z, r, g, b, a, u, v;
        public Vertex(float _x, float _y, float _z, float _r = 1.0f, float _g = 1.0f, float _b = 1.0f, float _a = 1.0f, float _u = 0.0f, float _v = 0.0f)
        {
            x = _x;
            y = _y;
            z = _z;
            r = _r;
            g = _g;
            b = _b;
            a = _a;
            u = _u;
            v = _v;
        }
    }
}