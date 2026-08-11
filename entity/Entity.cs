
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
        public Entity? Parent { get; internal set;} = null;

        private readonly List<Component> components = new List<Component>();
        protected readonly List<Entity> children = new List<Entity>();

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
            foreach (Entity entity in children)
            {
                entity.Init();
            }
        }
        public virtual void Destroy()
        {
            foreach (Component comp in components)
            {
                comp.Destroy();
            }
            foreach (Entity entity in children)
            {
                entity.Destroy();
            }
        }
        public virtual void Update()
        {
            foreach (Component comp in components)
            {
                comp.Update();
            }
            foreach (Entity entity in children)
            {
                entity.Update();
            }
        }
        public virtual void FixedUpdate()
        {
            foreach (Component comp in components)
            {
                comp.FixedUpdate();
            }
            foreach (Entity entity in children)
            {
                entity.FixedUpdate();
            }
        }
        public virtual void Render()
        {
            foreach (Component comp in components)
            {
                comp.Render();
            }
            foreach (Entity entity in children)
            {
                entity.Render();
            }

            //if (Util.Debug.showObjectsWithoutRenderer && GetComponent<SpriteRenderer>() == null)
            {
                //Draw.Sprite(Util.Debug.cursorTex, Transform.Position.xy + new vec2(3,3));
            }
        }
        public virtual void OnRemove()
        {
            foreach(Entity child in children)
            {
                child.OnRemove();
            }
        }
        public virtual void OnAdd()
        {
        }

        public Component AddComponent(in Component comp)
        {
            components.Add(comp);
            comp.Parent = this;
            comp.OnAdd();
            return comp;
        }

        public void RemoveComponent(in Component comp)
        {
            components.Remove(comp);
            comp.OnRemove();
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
        public virtual Entity AddChild(in Entity entity)
        {
            children.Add(entity);
            entity.Parent = this;
            entity.OnAdd();
            return entity;
        }

        public virtual void RemoveChild(in Entity entity)
        {
            entity.OnRemove();
            entity.Parent = null;
            children.Remove(entity);
        }

        public virtual void ClearChildren()
        {
            children.Clear();
        }

    }
}