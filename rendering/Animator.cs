
using panpan;
using GlmSharp;
using panpan.Scene;
using SDL3;
using panpan.Util;

namespace panpan.Rendering
{  
    public class Animator : Component
    {
        protected struct Animation
        {
            public string Name;
            public List<Rect> Frames;
            public float Framerate;
            public float CurrentFrame;
        }      
        private SpriteRenderer? renderer;
        private Dictionary<string, Animation> animations;
        private int frameWidth;
        private int frameHeight;

        private string currentlyPlaying = "";
        private string quedToPlay = "";
        private bool quedAnimDoesResetOnStart = false;

        protected bool submitOnRender = false;

        private const string LOG_TAG = "panpan-Animator";

        public Animator(SpriteRenderer? renderer, int frameWidth, int frameHeight)
        {
            this.renderer = renderer;
            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;
            animations = new Dictionary<string, Animation>();
        }

        public override void FixedUpdate()
        {
            Log.Assert(animations.ContainsKey(quedToPlay) || quedToPlay == "", $"qued animation doesnt exist: {quedToPlay}", LOG_TAG);

            if (currentlyPlaying != quedToPlay)
            {
                if (quedAnimDoesResetOnStart)
                {
                    var anim = animations[quedToPlay];
                    anim.CurrentFrame = 0;
                    animations[quedToPlay] = anim;
                }

                currentlyPlaying = quedToPlay;
            }

            if (currentlyPlaying != "")
            {
                var anim = animations[currentlyPlaying];
                float framerate = anim.Framerate;
                anim.CurrentFrame += framerate / 60.0f;
                if (anim.CurrentFrame >= anim.Frames.Count)
                {
                    anim.CurrentFrame -= anim.Frames.Count;
                }
                animations[currentlyPlaying] = anim;

                if(!submitOnRender)
                    SubmitAnimation(anim);
            }
            base.FixedUpdate();
        }

        public override void Render()
        {
            base.Render();
            if(submitOnRender)
            {
                if (currentlyPlaying != "")
                    SubmitAnimation(animations[currentlyPlaying]);
            }
                
        }

        public void AddAnimation(string key, int[] frames, float framerate = 12f)
        {
            var anim = new Animation();
            anim.Name = key;
            anim.Frames = new();
            foreach(int frame in frames)
                anim.Frames.Add(new panpan.Util.Rect(frame * frameWidth, 0, frameWidth, frameHeight));
            anim.Framerate = framerate;
            anim.CurrentFrame = 0;
            if(animations.ContainsKey(key))
                animations[key] = anim;
            else
                animations.Add(key, anim);
        }

        public void AddAnimation(string key, Rect[] frames, float framerate = 12f)
        {
            var anim = new Animation();
            anim.Name = key;
            anim.Frames = frames.ToList();
            anim.Framerate = framerate;
            anim.CurrentFrame = 0;
            if(animations.ContainsKey(key))
                animations[key] = anim;
            else
                animations.Add(key, anim);
        }

        public void Play(string key, bool resetOnStart = true)
        {
            quedToPlay = key;
            quedAnimDoesResetOnStart = resetOnStart;
        }

        protected virtual void SubmitAnimation(Animation anim)
        {
            renderer!.Clip(anim.Frames[(int)anim.CurrentFrame]);
        }

    }
}