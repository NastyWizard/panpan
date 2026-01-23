
using GlmSharp;
using panpan.Rendering;
using panpan.Scene;
using panpan.Util;

namespace panpanExample
{
    public enum LightSize
    {
        CUSTOM = 0,
        SMALL = 32,
        MEDIUM = 64
    };
    public class ExampleLight : ExampleGameObject
    {
        public ExampleLight(int x, int y, LightSize size = LightSize.MEDIUM): base(x, y)
        {
        }

        public virtual void DrawLights()
        {
            vec2 p = new vec2(MathF.Floor(Position.x), MathF.Floor(Position.y)) + new vec2(-16,16);
            Draw.Sprite(ExampleGameTextures.lightTex32_weak, p);
        }
    }
}