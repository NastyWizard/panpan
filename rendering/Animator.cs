
using panpan;
using GlmSharp;
using panpan.Scene;
using SDL3;

namespace panpan.Rendering
{
    internal struct Animation
    {
        public string Name;
        public int[] Frames;
        public float Framerate;
        public float CurrentFrame;
    }
    public class Animator : Component
    {
        private SpriteRenderer renderer;
        private Dictionary<string, Animation> animations;
        private int frameWidth;
        private int frameHeight;

        private string currentlyPlaying = "";
        private string quedToPlay = "";
        private bool quedAnimDoesResetOnStart = false;

        public Animator(ref SpriteRenderer renderer, int frameWidth, int frameHeight)
        {
            this.renderer = renderer;
            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;
            animations = new Dictionary<string, Animation>();
        }

        public override void FixedUpdate()
        {

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
                if (anim.CurrentFrame >= anim.Frames.Length)
                {
                    anim.CurrentFrame -= anim.Frames.Length;
                }
                animations[currentlyPlaying] = anim;

                renderer.Clip(new panpan.Util.Rect(anim.Frames[(int)anim.CurrentFrame] * frameWidth, 0, frameWidth, frameHeight));
            }
            base.FixedUpdate();
        }

        public void AddAnimation(string key, int[] frames, float framerate = 12f)
        {
            var anim = new Animation();
            anim.Name = key;
            anim.Frames = frames;
            anim.Framerate = framerate;
            anim.CurrentFrame = 0;
            animations.Add(key, anim);
        }

        public void Play(string key, bool resetOnStart = true)
        {
            quedToPlay = key;
            quedAnimDoesResetOnStart = resetOnStart;
        }

    }
}