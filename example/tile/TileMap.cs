
using panpan.Rendering;
using panpan.Scene;
using panpan.Assets;
using GlmSharp;
using panpan.Collision;
using panpan;
using panpan.Util;

namespace panpanExample
{
    public class TileMap : Entity
    {
        Tile[,] tiles;
        private int width;
        private int height;

        public int Width => width;
        public int Height => height;
        public int TileSize = 8;
        public TileMap(int x, int y, int w, int h)
        {
            width = w;
            height = h;
            Position.xy = new vec2(x, y);
            tiles = new Tile[w,h];
        }

        public Tile AddTile(uint x, uint y, TileSet tileSet)
        {
            if(tiles[x, y] != null)
                return tiles[x, y];
            Tile tile = new Tile((int)x*8, (int)y*8, tileSet);
            tiles[x, y] = tile;
            tile.Init();

            Scene.AddChild(tile);

            return tile;
        }

        public Tile GetTile(uint x, uint y)
        {
            return tiles[x,y];
        }

        public void RemoveTile(uint x, uint y)
        {
            if(x >= width || y >= height)
                return;
            if(tiles[x, y] == null)
                return;
            var tile = tiles[x, y];
            if(Scene.RemoveChild(tile))
            {
                tile.Destroy();
                tiles[x, y] = null;
            }
        }

        public void UpdateAutoTiles()
        {
            for(int h = 0; h < height; h++)
            {
                for(int w = 0; w < width; w++)
                {
                    var tile = tiles[w,h];
                    if(tile == null)
                    {
                        continue;
                    }
                    var tSet = tile.tileSet;
                    bool left = w == 0 || tiles[w-1,h] != null && tiles[w-1,h].tileSet.index == tSet.index;
                    bool right = w == width-1 || tiles[w+1,h] != null && tiles[w+1,h].tileSet.index == tSet.index;
                    bool top = h == 0 || tiles[w,h-1] != null && tiles[w,h-1].tileSet.index == tSet.index;
                    bool bottom = h == height-1 || tiles[w,h+1] != null && tiles[w,h+1].tileSet.index == tSet.index;
                    var autoTile = 0;
                    if(left) autoTile |= 1;
                    if(right) autoTile |= 2;
                    if(top) autoTile |= 4;
                    if(bottom) autoTile |= 8;
                    tile.renderer.Clip(tSet.clips[autoTile]);
                }
            }
        }

        public override void Init()
        {
            base.Init();
        }

        public override void Update()
        {
            base.Update();
        }
        public override void Render()
        {
            base.Render();
        }

        public void DrawBounds()
        {
            Draw.Rect(new Rect((int)Position.x-1, (int)Position.y, width*TileSize+1, height*TileSize+1), Color.SkyBlue);
        }
    }
}