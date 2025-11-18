
using System.Reflection;
using panpan.Assets;
using panpan.Rendering;

namespace panpan.Util
{
    public class Debug
    {
        public static readonly Texture cursorTex = new Texture(Sprites.cursor,7,7);
        public static bool showObjectsWithoutRenderer = false;
    }
}
