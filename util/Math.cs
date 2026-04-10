
using System.Runtime.InteropServices;
using GlmSharp;
using panpan.Rendering;

namespace panpan.Util
{
    public class PMath
    {
        public static float pi = 3.14159265359f;

        public static float RadToDeg(float radians)
        {
            return radians * (180 / pi);
        }

        public static float DegToRad(float degrees)
        {
            return degrees * (pi / 180);
        }

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int X, Y, Width, Height, OriginX, OriginY;
        public float Angle;
        public Rect(int x, int y, int w, int h)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;

            OriginX = 0;
            OriginY = 0;
            Angle = 0;
        }

        public bool Intersects(Rect other)
        {
            return !(other.X > X + Width ||
                    other.X + other.Width < X ||
                    other.Y > Y + Height ||
                    other.Y + other.Height < Y);
        }

        public bool IntersectsPosition(vec2 pos)
        {
            vec2 relative = new(pos.x - X, pos.y - Y);
            float pAngle = -Angle;
            float cos = MathF.Cos(pAngle);
            float sin = MathF.Sin(pAngle);

            float a = PMath.DegToRad(pAngle);
            mat4 rotz = new mat4(
                MathF.Cos(a),  -MathF.Sin(a),  0,    0,
                MathF.Sin(a),  MathF.Cos(a),   0,    0,
                0,                      0,     0,    0,
                0,                      0,     0,    1
            );
            
            vec2 local = (rotz * new vec4(relative,0,1)).xy;//new vec2(cos * relative.x - sin * relative.y, sin * relative.x + cos * relative.y);

            return local.x >= -OriginX && local.x <= Width - OriginX &&
                   local.y >= -OriginY && local.y <= Height - OriginY;

            //return pos.x >= X && pos.x <= X + Width &&
            //       pos.y >= Y && pos.y <= Y + Height;
        }

        public void DebugRender()
        {
            vec2 p1 = new(0,0);
            vec2 p2 = new(0 + Width,0);
            vec2 p3 = new(0 + Width, Height);
            vec2 p4 = new(0, Height);

            vec2 o = new(OriginX,OriginY);

            p1 -= o;
            p2 -= o;
            p3 -= o;
            p4 -= o;

            float a = PMath.DegToRad(Angle);
            
            mat4 rotz = new mat4(
                MathF.Cos(a),  -MathF.Sin(a),  0,    0,
                MathF.Sin(a),  MathF.Cos(a),   0,    0,
                0,                      0,             0,    0,
                0,                      0,             0,    1
            );

            p1 = (rotz * new vec4(p1,0,1)).xy;
            p2 = (rotz * new vec4(p2,0,1)).xy;
            p3 = (rotz * new vec4(p3,0,1)).xy;
            p4 = (rotz * new vec4(p4,0,1)).xy;

            vec2 p = new(X,Y);
            p1 += p;
            p2 += p;
            p3 += p;
            p4 += p;
            
            Draw.Line(p1, p2, Color.Green);
            Draw.Line(p2, p3, Color.Green);
            Draw.Line(p3, p4, Color.Green);
            Draw.Line(p4, p1, Color.Green);
            Draw.Line(p1, p3, Color.Green);
        }
    }
}