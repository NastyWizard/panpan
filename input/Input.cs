
using SDL3;

namespace panpan.Input
{
    public class Input
    {
        public static void HandleEvents()
        {
            SDL.Event e;
            while(SDL.PollEvent(out e))
            {
                switch ((SDL.EventType)e.Type)
                {
                    case SDL.EventType.KeyDown:
                        
                        break;
                }
            }
        }

    }
}