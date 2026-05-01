
using GlmSharp;
using panpan.Util;
using panpan.Assets;
using System.Drawing;
using panpan.Rendering.Text;
using panpan.Rendering.Util;

namespace panpan.Rendering
{
    public partial class Draw
    {
        static BasicRenderer renderer = new BasicRenderer();
        static SpriteRenderer spriteRenderer = new SpriteRenderer(null, null, Shapes.quad, DefaultMaterials.Standard!);
        static SpriteRenderer backRenderer = new SpriteRenderer(null, null, Shapes.quad, DefaultMaterials.Backbuffer!);
        static TextRenderer textRenderer = new TextRenderer();
        public static TextRenderer TextRenderer => textRenderer;
        
        static vec4 color;

        // Text
        public static void Text(String str, Font font, vec3 pos, vec4? color = null)
        {
            textRenderer.SetFont(font);
            textRenderer.DrawText(str, pos, color);
        }
        public static void Text(String str, Font font, int x, int y, int z, vec4? color = null) => Text(str, font, new vec3(x,y,z), color);
        
        // CENTER
        public static void TextCenter(String str, Font font, vec3 pos, vec4? color = null)
        {
            textRenderer.SetFont(font);
            textRenderer.DrawText(str, pos - new vec3(font.Measure(str)/2,0,0), color);
        }
        public static void TextCenter(String str, Font font, int x, int y, int z, vec4? color = null) => TextCenter(str, font, new vec3(x,y,z), color);

        // WRAP
        public static void TextWrap(string str, Font font, vec3 pos, int maxLen, vec4? color = null)
        {
            textRenderer.SetFont(font);

            string outString = "";
            string currentLine = "";
            string currentWord = "";

            int pen = 0;

            while (pen < str.Length)
            {
                char c = str[pen];

                if (c != ' ')
                    currentWord += c;

                if (c == ' ' || pen == str.Length - 1)
                {
                    // handle last char case
                    if (pen == str.Length - 1 && c != ' ')
                        currentWord += "";

                    string testLine = currentLine == "" ? currentWord : currentLine + " " + currentWord;
                    int m = font.Measure(testLine);

                    if (currentLine == "")
                    {
                        currentLine = currentWord;
                    }
                    else if (m < maxLen)
                    {
                        currentLine += " " + currentWord;
                    }
                    else
                    {
                        outString += currentLine + "\n";
                        currentLine = currentWord;
                    }

                    currentWord = "";
                }

                pen++;
            }

            if (currentLine.Length > 0)
                outString += currentLine;

            textRenderer.DrawText(outString, pos, color);
        }

        // OUTLINE
        //
        public static void TextOutline(String str, Font font, vec3 pos, int thickness = 1, vec4? color = null, vec4? outline = null)
        {
            color ??= panpan.Rendering.Color.White;
            outline ??= panpan.Rendering.Color.Black;

            for(int i = 0; i < thickness; i++)
            {
                Draw.Text(str,font,pos - new vec3(i,0,pos.z-1),outline);
                Draw.Text(str,font,pos - new vec3(-i,0,pos.z-1),outline);
                Draw.Text(str,font,pos - new vec3(0,i,pos.z-1),outline);
                Draw.Text(str,font,pos - new vec3(0,-i,pos.z-1),outline);

                Draw.Text(str,font,pos - new vec3(i,i,pos.z-1),outline);
                Draw.Text(str,font,pos - new vec3(-i,i,pos.z-1),outline);
                Draw.Text(str,font,pos - new vec3(i,-i,pos.z-1),outline);
                Draw.Text(str,font,pos - new vec3(-i,-i,pos.z-1),outline);
            }
            Draw.Text(str,font,pos,color);
        }
        public static void TextOutline(String str, Font font, int x, int y, int z, int thickness = 1, vec4? color = null, vec4? outline = null) => TextOutline(str, font, new vec3(x,y,z), thickness, color, outline);
        
        //
        public static void TextCenterOutline(String str, Font font, vec3 pos, int thickness = 1, vec4? color = null, vec4? outline = null)
        {
            Draw.TextOutline(str, font, pos - new vec3(font.Measure(str)/2,0,0), thickness, color, outline);
        }
        public static void TextCenterOutline(String str, Font font, int x, int y, int z, int thickness = 1, vec4? color = null, vec4? outline = null) => TextCenterOutline(str, font, new vec3(x,y,z), thickness, color, outline);


        // Shapes
        public static void Rect(vec2 bl, vec2 size, vec4? color = null)
        {
            color ??= Color.White;
            var rect = new Rect((int)bl.x, (int)bl.y, (int)size.x, (int)size.y);
            DrawBatch.SubmitRect(rect, color.Value);
        }
        public static void Rect(Rect rect, vec4? color = null, bool fill = false)
        {
            color ??= Color.White;
            DrawBatch.SubmitRect(rect, color.Value, fill);
        }

        public static void Line(vec2 p1, vec2 p2, vec4? color = null, bool pixelPerfect = true)
        {
            color ??= Color.White;
            if (pixelPerfect)
            {
                p1.x = MathF.Round(p1.x);
                p1.y = MathF.Round(p1.y);
                p2.x = MathF.Round(p2.x);
                p2.y = MathF.Round(p2.y);

                var current = p1;

                while (vec2.Distance(current.xy, p2.xy) >= 1f)
                {
                    Draw.Dot(current.xy, color);

                    var dir = p2 - current;
                    current += dir.Normalized;
                }
            }
            else
            {
                var dist = vec2.Distance(p1, p2);

                renderer.Position = new vec3(p1.xy, 1);
                renderer.Scale = vec3.Ones;
                renderer.Width = 1f;
                renderer.Height = (uint)dist;
                renderer.Angle = MathF.Atan2(p2.y - p1.y, p2.x - p1.x) + PMath.DegToRad(90);
                renderer.Origin = vec2.UnitY;

                renderer.Render();
            }
        }
        public static void Dot(vec2 p1, vec4? color = null)
        {
            color ??= Color.White;
            DrawBatch.SubmitPixel(p1, color.Value);
        }

        // Sprites / RT
        public static void Sprite(Texture texture, vec2 pos, vec2? scale = null, Rect? clipRect = null)
        {
            scale ??= vec2.Ones;
            App.GetSceneManager().ActiveScene.ActiveCamera.PushUniformData();
            spriteRenderer.SetTexture(texture);
            
            if (clipRect != null)
            {
                spriteRenderer.Clip(clipRect);
                float scaledClipWidth = clipRect.Value.Width * scale.Value.x;
                float scaledClipHeight = clipRect.Value.Height * scale.Value.y;
                spriteRenderer.Width = scaledClipWidth;
                spriteRenderer.Height = scaledClipHeight;
                vec3 _scale = vec3.Ones;
                spriteRenderer.SetTransform(new vec3(pos.x, pos.y, 0.0f), _scale);
            }
            else
            {
                // No clip, scale applies to full texture
                vec3 _scale = new vec3(scale.Value.x, scale.Value.y, 1.0f);
                spriteRenderer.SetTransform(new vec3(pos.x, pos.y, 0.0f), _scale);
            }
            
            spriteRenderer.Render();
        }

        public static void RenderTarget(RenderTarget rt, vec2 pos, vec2? scale = null)
        {
            App.GetSceneManager().ActiveScene.Camera.PushUniformData();
            backRenderer.SetTexture(rt.GetTexture());
            backRenderer.SetTransform(new vec3(pos.x, pos.y, 0.0f));
            backRenderer.Render();
        }

        public static void SetRTMaterial(Material mat)
        {
            backRenderer.SetMaterial(mat);
        }
        public static void ResetRTMaterial()
        {
            backRenderer.SetMaterial(DefaultMaterials.Backbuffer!);
        }

        public static void SetRTUniformFloats(float[] unfs)
        {
            backRenderer.SetUniformFloat(unfs);
        }
        public static void SetRTAdditionalTextures(Texture[] textures)
        {
            backRenderer.SetAdditionalTextures(textures);
        }
        public static void ClearRTAdditionalTextures()
        {
            backRenderer.ClearAdditionalTextures();
        }

        private class BasicRenderer : MeshRenderer
        {

            public vec3 Position, Scale;
            public float Angle;
            public BasicRenderer() : base(Shapes.quad, DefaultMaterials.StandardNoTexture!)
            {
                RegisterSetUniforms(SetUniforms);
            }

            private void SetUniforms()
            {
                float[] uniforms = new float[4]
                {
                    color.r, color.g, color.b, color.a
                };

                SetUniformFloat(uniforms);
            }

            protected override mat4 ComputeModelMatrix()
            {
                var pos =   Position;
                var rot =   Angle;
                var scale = Scale * new vec3(Width, Height, 1.0f);

                return (mat4.Translate(pos) *
                    mat4.RotateZ(rot) *
                    mat4.Scale(scale) *
                    mat4.Translate(-new vec3(Origin.x, Origin.y, 0)));
            }
        }
    }
}
