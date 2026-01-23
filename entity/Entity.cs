
using System.Collections.Generic;
using System.Diagnostics;
using GlmSharp;
using panpan.Rendering;
using SDL3;

namespace panpan.Scene
{
    public abstract class Entity
    {
        public Scene? Scene { get; internal set; } = null;

        private readonly List<Component> components = new List<Component>();

        public Transform Transform = new Transform();
        public ref vec3 Position => ref Transform.Position;
        public ref vec3 Scale => ref Transform.Scale;
        public ref float Angle => ref Transform.Angle;
        
        public virtual void Init()
        {
            foreach (Component comp in components)
            {
                comp.Init();
            }
        }
        public virtual void Destroy()
        {
            foreach (Component comp in components)
            {
                comp.Destroy();
            }
        }
        public virtual void Update()
        {
            foreach (Component comp in components)
            {
                comp.Update();
            }
        }
        public virtual void Render()
        {
            foreach (Component comp in components)
            {
                comp.Render();
            }

            //if (Util.Debug.showObjectsWithoutRenderer && GetComponent<SpriteRenderer>() == null)
            {
                Draw.Sprite(Util.Debug.cursorTex, Transform.Position.xy + new vec2(3,3));
            }
        }

        public Component AddComponent(in Component comp)
        {
            components.Add(comp);
            comp.Parent = this;
            return comp;
        }

        public T? GetComponent<T>() where T : Component
        {
            foreach (Component comp in components)
            {
                if (comp is T match)
                {
                    return match;
                }
            }

            return null;
        }

    }
}