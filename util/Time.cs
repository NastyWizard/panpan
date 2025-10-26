
using SDL3;

namespace panpan.Util
{
    public class Time
    {
        public static float Elapsed()
        {
            float seconds = SDL.GetTicks() / 1000.0f;
            return seconds;
        }

    }
}