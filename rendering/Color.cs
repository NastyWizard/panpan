

using System.Globalization;
using GlmSharp;

namespace panpan.Rendering
{
    public class Color
    {
        public static vec4 White = vec4.Ones;
        public static vec4 Black = vec4.UnitW;

        public static vec4 Transparent = vec4.Zero;

        public static vec4 Green = new vec4(0f,1f,0f,1f);

        public static vec4 SkyBlue = Hex("#639bff");

        public static vec4 Hex(string hex)
        {
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);

            if (hex.Length != 6 && hex.Length != 8)
                throw new ArgumentException("Hex color must be 6 (RGB) or 8 (RGBA) characters long.");

            byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            byte a = (hex.Length == 8) ? byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber) : (byte)255;

            return new vec4(r / 255f, g / 255f, b / 255f, a / 255f);
        }
    }
    


}