
using System;
using GlmSharp;
using SDL3;

namespace panpan
{
    public delegate void InputDelegate(SDL.Keycode? e);
    public delegate void MouseDelegate(byte button);
    public delegate void MouseWheelDelegate(float delta);
    public class Input
    {
        public static Dictionary<string, InputDelegate> keyDownEvents = new Dictionary<string, InputDelegate>();
        public static Dictionary<string, InputDelegate> keyUpEvents = new Dictionary<string, InputDelegate>();
        public static Dictionary<string, InputDelegate> keyHeldEvents = new Dictionary<string, InputDelegate>();

        public static Dictionary<string, MouseDelegate> mouseDownEvents = new Dictionary<string, MouseDelegate>();
        public static Dictionary<string, MouseDelegate> mouseUpEvents = new Dictionary<string, MouseDelegate>();
        public static Dictionary<string, MouseDelegate> mouseHeldEvents = new Dictionary<string, MouseDelegate>();
        public static Dictionary<string, MouseWheelDelegate> mouseWheelEvents = new Dictionary<string, MouseWheelDelegate>();

        private static Dictionary<SDL.Keycode, bool> keysCurrentlyDown = new Dictionary<SDL.Keycode, bool>();
        private static Dictionary<byte, bool> mouseCurrentlyDown = new Dictionary<byte, bool>();

        public static vec2 MousePosition;
        public static vec2 MousePositionWindow;

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
                case SDL.EventType.MouseButtonDown:
                    if (!mouseCurrentlyDown.ContainsKey(e.Button.Button))
                    {
                        MouseDown(e.Button.Button);
                        mouseCurrentlyDown.Add(e.Button.Button, true);
                    }
                    break;
                case SDL.EventType.MouseButtonUp:
                    if (mouseCurrentlyDown.ContainsKey(e.Button.Button))
                    {
                        MouseUp(e.Button.Button);
                        mouseCurrentlyDown.Remove(e.Button.Button);
                    }
                    break;
                case SDL.EventType.MouseWheel:
                    MouseWheel(e.Wheel.Y);
                    break;
            }
        }

        public static void Update()
        {
            foreach (var key in keysCurrentlyDown.Keys)
            {
                KeyHeld(key);
            }

            foreach (var mb in mouseCurrentlyDown.Keys)
            {
                MouseHeld(mb);
            }
            int w, h;
            SDL.GetWindowSize(App.GetWindow(), out w, out h);
            SDL.GetMouseState(out MousePositionWindow.x, out MousePositionWindow.y);
            
            MousePosition = MousePositionWindow;
            vec4 clipPos = new vec4((2.0f * MousePosition.x) / w - 1.0f, (2.0f * MousePosition.y) / h - 1.0f, 0f, 1f);
            vec4 worldPos = App.GetSceneManager().ActiveScene.Camera.GetViewProjectionMatrix().Inverse * clipPos;
            MousePosition = new vec2(worldPos.x, -worldPos.y) + App.GetSceneManager().ActiveScene.Camera.Position.xy;
        }

        public static void RegisterOnKeyHeld(InputDelegate action)
        {
            keyHeldEvents.Add(BuildActionKey(action), action);
        }
        public static void RegisterOnKeyDown(InputDelegate action)
        {
            keyDownEvents.Add(BuildActionKey(action), action);
        }
        public static void RegisterOnKeyReleased(InputDelegate action)
        {
            keyUpEvents.Add(BuildActionKey(action), action);
        }

        public static void RegisterOnMouseDown(MouseDelegate action)
        {
            mouseDownEvents.Add(BuildActionKey(action), action);
        }
        public static void RegisterOnMouseHeld(MouseDelegate action)
        {
            mouseHeldEvents.Add(BuildActionKey(action), action);
        }
        public static void RegisterOnMouseReleased(MouseDelegate action)
        {
            mouseUpEvents.Add(BuildActionKey(action), action);
        }
        public static void RegisterOnMouseWheel(MouseWheelDelegate action)
        {
            mouseWheelEvents.Add(BuildActionKey(action), action);
        }

        public static void DeregisterAll()
        {
            mouseDownEvents.Clear();
            mouseHeldEvents.Clear();
            mouseUpEvents.Clear();
            mouseWheelEvents.Clear();

            keyDownEvents.Clear();
            keyUpEvents.Clear();
            keyHeldEvents.Clear();
        }
      
        public static void DeregisterOnMouseDown(MouseDelegate action)
        {
            mouseDownEvents.Remove(BuildActionKey(action));
        }
        
        public static void DeregisterOnMouseHeld(MouseDelegate action)
        {
            mouseHeldEvents.Remove(BuildActionKey(action));
        }
       
        public static void DeregisterOnMouseReleased(MouseDelegate action)
        {
            mouseUpEvents.Remove(BuildActionKey(action));
        }

       
        public static void DeregisterOnMouseWheel(MouseWheelDelegate action)
        {
            mouseWheelEvents.Remove(BuildActionKey(action));
        }


        public static void DeregisterOnKeyDown(InputDelegate action)
        {
            keyDownEvents.Remove(BuildActionKey(action));
        }
        
        public static void DeregisterOnKeyHeld(InputDelegate action)
        {
            keyHeldEvents.Remove(BuildActionKey(action));
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
        
        private static void MouseDown(byte btn)
        {
            foreach (MouseDelegate action in mouseDownEvents.Values)
            {
                action.Invoke(btn);
            }
        }
        
        private static void MouseHeld(byte btn)
        {
            foreach (MouseDelegate action in mouseHeldEvents.Values)
            {
                action.Invoke(btn);
            }
        }
        
        private static void MouseUp(byte btn)
        {
            foreach (MouseDelegate action in mouseUpEvents.Values)
            {
                action.Invoke(btn);
            }
        }
        
        private static void MouseWheel(float delta)
        {
            foreach (MouseWheelDelegate action in mouseWheelEvents.Values)
            {
                action.Invoke(delta);
            }
        }

        private static string BuildActionKey(Delegate action)
        {
            var target = action.Target;
            var method = action.Method;

            int targetId = target != null 
                ? System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(target) 
                : 0;

            int methodId = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(method);

            return $"{targetId}:{methodId}";
        }
    }
}