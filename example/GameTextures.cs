
using panpan.Assets;
using panpan.Rendering;

namespace panpanExample
{
    public class GameTextures
    {
        public static Texture lightTex64;
        public static Texture lightTex64_weak;
        public static Texture lightTex32_weak;
        public static Texture fsLight;
        public static Texture defaultPalette;
        public static Texture palette_1;
        public static Texture palette_2;

        public static void Init()
        {
            // lights
            lightTex64 = new Texture(Sprites.light64,64,64);
            lightTex64.CopyPass();

            lightTex32_weak = new Texture(Sprites.light32_weak,32,32);
            lightTex32_weak.CopyPass();

            lightTex64_weak = new Texture(Sprites.light64_weak,64,64);
            lightTex64_weak.CopyPass();

            fsLight = new Texture(Sprites.fsLight,320,176);
            fsLight.CopyPass();

            // Palettes
            defaultPalette = new Texture(Sprites.defaultpalette,16,1);
            defaultPalette.CopyPass();

            palette_1 = new Texture(Sprites.palette_1,16,1);
            palette_1.CopyPass();

            palette_2 = new Texture(Sprites.palette_3,16,1);
            palette_2.CopyPass();
        }
    }
}