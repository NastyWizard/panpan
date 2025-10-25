
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
}