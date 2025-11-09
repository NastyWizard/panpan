
using System.Runtime.InteropServices;

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
        public int X, Y, Width, Height;
        public Rect(int x, int y, int w, int h)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
        }

        public bool Intersects(Rect other)
        {
            return !(other.X > X + Width ||
                    other.X + other.Width < X ||
                    other.Y > Y + Height ||
                    other.Y + other.Height < Y);
        }
    }
}