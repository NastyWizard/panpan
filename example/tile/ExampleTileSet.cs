
using panpan.Assets;
using panpan.Rendering;
using panpan.Util;

namespace panpanExample
{
    public struct ExampleTileSet
    {
        public uint index;
        public Rect[] clips = new Rect[16];

        public ExampleTileSet(uint index)
        {
            this.index = index;
            var tileSize = 8;
            int x = (int)(this.index % 4) * (4 * tileSize);
            int y = 0; //(int)Math.DivRem(this.index * 4 * tileSize, 4).Quotient;
            clips[6] = new Rect(x   ,y,8,8);            // 0
            clips[7] = new Rect(x+8 ,y,8,8);            // 1
            clips[5] = new Rect(x+16,y,8,8);            // 2
            clips[4] = new Rect(x+24,y,8,8);            // 3
            
            clips[14] = new Rect(x   ,y+8,8,8);          // 4
            clips[15] = new Rect(x+8 ,y+8,8,8);          // 5
            clips[13] = new Rect(x+16,y+8,8,8);          // 6
            clips[12] = new Rect(x+24,y+8,8,8);          // 7
            
            clips[10] = new Rect(x   ,y+16,8,8);         // 8
            clips[11] = new Rect(x+8 ,y+16,8,8);         // 9
            clips[9] = new Rect(x+16,y+16,8,8);        // 10
            clips[8] = new Rect(x+24,y+16,8,8);        // 11
            
            clips[2] = new Rect(x   ,y+24,8,8);        // 12
            clips[3] = new Rect(x+8 ,y+24,8,8);        // 13
            clips[1] = new Rect(x+16,y+24,8,8);        // 14   
            clips[0] = new Rect(x+24,y+24,8,8);        // 15
        }
    }

    public class ExampleTileSets
    {
        public static Texture TilesetTexture = new Texture(Sprites.tiles, 128, 128);
        public static ExampleTileSet Dirt = new ExampleTileSet(0);
        public static ExampleTileSet Brick = new ExampleTileSet(1);

    }
}