
using SDL3;

namespace panpan
{
    public delegate void InputDelegate(SDL.Keycode? e);
    public class Input
    {
        public static Dictionary<string, InputDelegate> keyDownEvents = new Dictionary<string, InputDelegate>();
        public static Dictionary<string, InputDelegate> keyUpEvents = new Dictionary<string, InputDelegate>();
        public static Dictionary<string, InputDelegate> keyHeldEvents = new Dictionary<string, InputDelegate>();

        private static Dictionary<SDL.Keycode, bool> keysCurrentlyDown= new Dictionary<SDL.Keycode, bool>();

        public static void HandleEvents(SDL.Event e)
        {
            switch ((SDL.EventType)e.Type)
            {
                case SDL.EventType.KeyDown:
                    if (!keysCurrentlyDown.ContainsKey(e.Key.Key))
                    {
                        KeyDown(e.Key.Key);
                        keysCurrentlyDown.Add(e.Key.Key, true);
                    }
                    break;
                case SDL.EventType.KeyUp:
                    if (keysCurrentlyDown.ContainsKey(e.Key.Key))
                    {
                        KeyUp(e.Key.Key);
                        keysCurrentlyDown.Remove(e.Key.Key);
                    }
                    break;
            }
        }

        public static void Update()
        {
            foreach(var key in keysCurrentlyDown.Keys)
            {
                KeyHeld(key);
            }
        }

        public static void RegisterOnKeyHeld(InputDelegate action)
        {
            keyHeldEvents.Add(action.Method.ReflectedType.FullName, action);
        }
        public static void RegisterOnKeyDown(InputDelegate action)
        {
            keyDownEvents.Add(action.Method.ReflectedType.FullName, action);
        }
        public static void DeregisterOnKeyDown(InputDelegate action)
        {
            keyDownEvents.Remove(action.Method.ReflectedType.FullName);
        }

        private static void KeyDown(SDL.Keycode? keycode)
        {
            foreach (InputDelegate action in keyDownEvents.Values)
            {
                action.Invoke(keycode);
            }
        }
        private static void KeyUp(SDL.Keycode? keycode)
        {
            foreach (InputDelegate action in keyUpEvents.Values)
            {
                action.Invoke(keycode);
            }
        }
        private static void KeyHeld(SDL.Keycode? keycode)
        {
            foreach (InputDelegate action in keyHeldEvents.Values)
            {
                action.Invoke(keycode);
            }
        }
        
    }
}