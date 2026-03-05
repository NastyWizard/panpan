using FreeTypeSharp;
using GlmSharp;
using panpan;
using panpan.Util;
using System.Runtime.InteropServices;
using System.Text;

namespace panpan.Rendering.Text
{
    public struct Glyph
    {
        public int BearingX, BearingY;
        public Rect Clip;
        public int Advance;
    }

    public unsafe class Font : IDisposable
    {
        private struct FontAtlas
        {
            public Texture Tex;
            public byte[] Pixels;
            public Glyph?[] glyphs;
            public uint Width, Height;
        }

        private FT_FaceRec_* face = null;
        private FontAtlas atlas;

        private bool pixelPerfect = false;
        private uint pixelSize;
        private uint kerning;
        private uint spacing;

        private SpriteBatch batch;


        public uint PixelSize => pixelSize;
        public uint Kerning => kerning;
        public uint Spacing => spacing;


 
        public Font(byte[] fontData, uint pixelSize, bool pixelPerfect = true, uint kerning = 1, uint spacing = 2)
        {
            this.pixelSize = pixelSize;
            this.pixelPerfect = pixelPerfect;
            this.kerning = kerning;
            this.spacing = spacing;
            FT_FaceRec_* face = null;

            fixed (byte* fontPtr = fontData)
            {
                // TODO: use FT_New_Memory_Face and bundle font in Assets
                FT.FT_New_Memory_Face(App.FreetypeLib, fontPtr, fontData.Length * sizeof(byte), 0, &face);
                FT.FT_Set_Pixel_Sizes(face, 0, pixelSize);
                this.face = face;
            }

            PopulateAtlas();

            batch = new SpriteBatch(GetAtlasTexture(), new Material(Assets.Shaders.standardFont_frag_hlsl, Assets.Shaders.standard_vert_hlsl));
            batch.SetOrigin(new vec2(0,1));
        }

        private void PopulateAtlas()
        {
            nuint numGlyphs = 128;
            ivec2 pen = new ivec2(0,0);

            atlas = new FontAtlas();
            atlas.glyphs = new Glyph?[128];
            atlas.Width = 1;
            
            int cellSize = (int)(face->size->metrics.height >> 6) + 1;
            int maxDim = (int)(cellSize * MathF.Ceiling(MathF.Sqrt((float)numGlyphs)));
            while(atlas.Width < maxDim) atlas.Width <<= 1; 
            atlas.Height = atlas.Width;
            atlas.Pixels = new byte[atlas.Width * atlas.Height];

            for(nuint i = 0; i < numGlyphs; i++)
            {
                // Gather glyph info
                var loadOp = FT_LOAD.FT_LOAD_RENDER;
                if(pixelPerfect)
                {
                    loadOp |= FT_LOAD.FT_LOAD_MONOCHROME;
                }

                if (FT.FT_Load_Char(face, i, loadOp) != FT_Error.FT_Err_Ok)
                    continue;

                var glyphSlot = face->glyph;
                var bitmap = glyphSlot->bitmap;
                

                uint width = bitmap.width;
                uint height = bitmap.rows;

                if(width == 0)
                    continue;

                if(pen.x + width >= atlas.Width)
                {
                    pen.x = 0;
                    pen.y += (int)(face->size->metrics.height >> 6) + 1;
                }

                
                // Save glyph data for lookup
                var glyph = new Glyph
                {
                    Clip = new Rect(pen.x, pen.y, (int)width, (int)height),
                    BearingX = glyphSlot->bitmap_left,
                    BearingY = glyphSlot->bitmap_top,
                    Advance = (int)(glyphSlot->advance.x >> 6)
                };

                atlas.glyphs[i] = glyph;

                // Write to the atlas
                if(pixelPerfect)
                {
                    WriteAtlasPixelPerfect(bitmap, pen);
                }
                else
                {
                    WriteAtlasAA(bitmap, pen);
                }

                // Move to the next char
                pen.x += (int)width+1;
            }

            atlas.Pixels = ConvertR8ToRGBA(atlas.Pixels,atlas.Width,atlas.Height);
        
            // Create texture atlas
            atlas.Tex = new Texture(atlas.Pixels, atlas.Width, atlas.Height, SDL3.SDL.GPUTextureFormat.R8G8B8A8Unorm, true);
            atlas.Tex.CopyPass();
        }

        private void WriteAtlasAA(FT_Bitmap_ bitmap, ivec2 pen)
        {
            for(int row = 0; row < bitmap.rows; ++row)
            {
                for(int col = 0; col < bitmap.width; ++col)
                {
                    int x = pen.x + col;
                    int y = pen.y + row;
                    atlas.Pixels[y * atlas.Width + x] = bitmap.buffer[row * bitmap.pitch + col];
                }   
            }
        }

        private void WriteAtlasPixelPerfect(FT_Bitmap_ bitmap, ivec2 pen)
        {
            for (int row = 0; row < bitmap.rows; ++row)
            {
                for (int col = 0; col < bitmap.width; ++col)
                {
                    int byteIndex = row * bitmap.pitch + (col >> 3);
                    byte byteValue = bitmap.buffer[byteIndex];

                    int bitIndex = 7 - (col & 7);
                    bool bitSet = (byteValue & (1 << bitIndex)) != 0;

                    byte value = bitSet ? (byte)255 : (byte)0;

                    int x = pen.x + col;
                    int y = pen.y + row;
                    atlas.Pixels[y * atlas.Width + x] = value;
                }
            }
        }

        public void Dispose()
        {
            unsafe
            {
                FT.FT_Done_Face(face);
            }
        }

        public Texture GetAtlasTexture()
        {
            return atlas.Tex;
        }

        public int GetSpacing()
        {
            var g = GetGlyph('i');
            if(!g.HasValue)
                return 0;
            return g.Value.Clip.Width;
        }

        public Glyph? GetGlyph(char c)
        {
            return atlas.glyphs[c];
        }

        public SpriteBatch GetBatch()
        {
            return batch;
        }

        private byte[] ConvertR8ToRGBA(byte[] rData, uint w, uint h)
        {
            byte[] rgba = new byte[w * h * 4];

            for (int i = 0; i < rData.Length; i++)
            {
                byte v = rData[i];
                int idx = i * 4;

                rgba[idx + 0] = 255;
                rgba[idx + 1] = 255;
                rgba[idx + 2] = 255;
                rgba[idx + 3] = v;
            }

            return rgba;
        }

        public int Measure(string text)
        {
            int measure = 0; 
            foreach(char c in text)
            {
                Glyph? g = GetGlyph(c);
                if(g != null)
                {
                    measure += (int)(g.Value.Clip.Width + kerning);
                }
            }

            return measure - (int)kerning;
        }
    }
}