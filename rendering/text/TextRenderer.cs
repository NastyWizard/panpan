using System.ComponentModel;
using System.Text;
using GlmSharp;
using panpan.Assets;
using panpan.Util;

namespace panpan.Rendering.Text
{
    public class TextRenderer
    {
        private SpriteBatch batch;

        private Font font;

        float oY = 1;

        public TextRenderer()
        {
        }

        public void SetFont(Font font)
        {
            batch = new SpriteBatch(font.GetAtlasTexture(), new Material(Assets.Shaders.standardFont_frag_hlsl, Assets.Shaders.standard_vert_hlsl));
            batch.SetOrigin(new vec2(0,oY));
            this.font = font;
        }

        public void DrawText(String str, vec2 pos)
        {
            batch.BeginFrame();
            var tex = font.GetAtlasTexture();
            ivec2 pen = (ivec2)pos;

            for(int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                Glyph? g = font.GetGlyph(c);
                if(g.HasValue)
                {
                    //Draw.Sprite(tex, pen + new ivec2(g.Value.BearingX,g.Value.BearingY), vec2.Ones, g.Value.Clip);
                    // TODO: needs fixing for bearing, batch should render from top left not bottom left 
                    batch.SubmitSprite(new vec3(pen + new ivec2(g.Value.BearingX,0), 0), g.Value.Clip);
                    pen.x += g.Value.Clip.Width+1;
                }
                if(c == ' ')
                {
                    pen.x += this.font.GetSpacing();
                }
            }
            batch.Render();

            ImGui.Begin("debug");


            if(ImGui.SliderFloat("OffsetY", ref oY, -2, 2))
                batch.SetOrigin(new vec2(0,oY));
            ImGui.End();


        }
    }
}

