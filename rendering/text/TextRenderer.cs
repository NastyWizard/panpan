using System.ComponentModel;
using System.Text;
using GlmSharp;
using panpan.Assets;
using panpan.Util;

namespace panpan.Rendering.Text
{
    public delegate void CommandDelegate();
    public class TextRenderer
    {
        private Font font;
        private Dictionary<string, CommandDelegate> textCommands = new Dictionary<string, CommandDelegate>();
        private vec4? colorOverride = null;
        public TextRenderer()
        {
            RegisterCommand("resetcol", ResetColorOverride);
        }

        public void SetFont(Font font)
        {
            this.font = font;
        }

        public void OverrideColor(vec4 col)
        {
            colorOverride = col;
        }

        public void ResetColorOverride()
        {
            colorOverride = null;
        }

        public void DrawText(String str, vec2 pos, vec4? color = null)
        {
            if(color == null)
                color = Color.White;

            var batch = this.font.GetBatch(); 
            //batch.BeginFrame();
            var tex = font.GetAtlasTexture();
            ivec2 pen = (ivec2)pos;

            bool isCommand = false;
            string command = "";

            for(int i = 0; i < str.Length; i++)
            {
                char c = str[i];

                if(c == '{')
                {
                    isCommand = true;
                    continue;
                }
                if(c == '}')
                {
                    isCommand = false;
                    ProcessCommand(command);
                    command = ""; // flush command
                    continue;
                }

                if(isCommand)
                {
                    command += c;
                    continue;
                }

                if(c == '\n')
                {
                    pen.x = (int)pos.x;
                    pen.y -= (int)this.font.PixelSize+(int)font.Spacing;
                    continue;
                }
                Glyph? g = font.GetGlyph(c);
                if(g.HasValue)
                {
                    vec4 col = colorOverride != null ? colorOverride.Value : color.Value;
                    batch.SubmitSprite(new vec3(pen + new ivec2(g.Value.BearingX,-g.Value.Clip.Height+g.Value.BearingY), 0), g.Value.Clip, col);
                    pen.x += g.Value.Clip.Width+(int)font.Kerning;
                }
                if(c == ' ')
                {
                    pen.x += font.GetSpacing();
                }
            }
            //batch.Render();
        }

        public void RegisterCommand(string command, CommandDelegate action)
        {
            textCommands.Add(command, action);
        }
        private void ProcessCommand(string command)
        {
            if(!textCommands.ContainsKey(command))
            {
                Console.WriteLine($"ERROR: Unknown text command [{command}]");
                return;
            }
            textCommands[command].Invoke();
        }

    }
}

