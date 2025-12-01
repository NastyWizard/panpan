
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
        private Tile[,] tileMap;
        private List<Tile> tiles;
        private int width;
        private int height;
        private int x;
        private int y;
        private bool loaded = false;

        public int Width => width;
        public int Height => height;
        public int X => x;
        public int Y => y;
        public int TileSize = 8;
        public TileMap(int x, int y, int w, int h)
        {
            width = w;
            height = h;
            this.x = x;
            this.y = y;
            Position.xy = new vec2(x * 320, y * 176);
            tileMap = new Tile[w,h];
            tiles = new List<Tile>();
        }

        public Tile AddTile(uint x, uint y, TileSet tileSet)
        {
            if(tileMap[x, y] != null)
                return tileMap[x, y];
            Tile tile = new Tile((int)x*8 + (int)Position.x, (int)y*8 + (int)Position.y, tileSet);
            tileMap[x, y] = tile;
            tiles.Add(tile);
            tile.Init();

            // Scene.AddChild(tile);

            return tile;
        }

        public Tile GetTile(uint x, uint y)
        {
            return tileMap[x,y];
        }

        public void RemoveTile(uint x, uint y)
        {
            if(x >= width || y >= height)
                return;
            if(tileMap[x, y] == null)
                return;
            var tile = tileMap[x, y];
            if(tiles.Remove(tile))
            {
                tile.Destroy();
                tileMap[x, y] = null;
            }
        }

        public void UpdateAutoTiles()
        {
            for(int h = 0; h < height; h++)
            {
                for(int w = 0; w < width; w++)
                {
                    var tile = tileMap[w,h];
                    if(tile == null)
                    {
                        continue;
                    }
                    var tSet = tile.tileSet;
                    bool left = w == 0 || tileMap[w-1,h] != null && tileMap[w-1,h].tileSet.index == tSet.index;
                    bool right = w == width-1 || tileMap[w+1,h] != null && tileMap[w+1,h].tileSet.index == tSet.index;
                    bool top = h == 0 || tileMap[w,h-1] != null && tileMap[w,h-1].tileSet.index == tSet.index;
                    bool bottom = h == height-1 || tileMap[w,h+1] != null && tileMap[w,h+1].tileSet.index == tSet.index;
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
            foreach(var tile in tiles)
            {
                tile.Update();
            }
            if(!loaded && InView())
            {
                LoadFromData(TileMapData.mapData[x][y]);
                loaded = true;
            }
        }
        public override void Render()
        {
            base.Render();
            foreach(var tile in tiles)
            {
                tile.Render();
            }
        }

        public void DrawBounds(vec4 color)
        {
            Draw.Rect(new Rect((int)Position.x-1, (int)Position.y, width*TileSize+1, height*TileSize+1), color);
        }

        public bool InView()
        {
            if (Scene?.Camera == null)
                return false;

            var camera = Scene.Camera;
            
            var tilemapBounds = new Rect(
                (int)Position.x - 1,
                (int)Position.y,
                width * TileSize + 1,
                height * TileSize + 1
            );

            // Calculate camera view bounds
            // Camera uses orthographic projection centered at Position
            // View extends from -(Width*Zoom)/2 to +(Width*Zoom)/2 and -(Height*Zoom)/2 to +(Height*Zoom)/2
            float effectiveWidth = camera.Width * camera.Zoom;
            float effectiveHeight = camera.Height * camera.Zoom;
            var cameraBounds = new Rect(
                (int)(camera.Position.x - effectiveWidth / 2.0f),
                (int)(camera.Position.y - effectiveHeight / 2.0f),
                (int)effectiveWidth,
                (int)effectiveHeight
            );

            return tilemapBounds.Intersects(cameraBounds);
        }

        public string GetMapData()
        {
            string str = ""; 
            for(uint y = 0; y < height; y++)
            {
                str += "[";
                for(uint x = 0; x < width; x++)
                {
                    var tile = GetTile(x, y);
                    if(tile == null)
                    {
                        str += "' ',";
                    }
                    else
                    {
                        str += $"'{tile.tileSet.index}',";
                    }
                }
                str = str.Remove(str.Length-1);
                str += "],";
            }
            str = str.Remove(str.Length-2);
            return str;
        }

        public void LoadFromData(string[][] data)
        {
            var t = Time.Elapsed();
            for(int y = height-1; y >= 0; y--)
            {
                for(uint x = 0; x < width; x++)
                {
                    switch(data[(int)x][y])
                    {
                        case "0":
                            AddTile(x, (uint)y, TileSets.Dirt);
                            break;
                        case "1":
                            AddTile(x, (uint)y, TileSets.Brick);
                            break;
                        default:
                            break;
                    }
                }
            }
            UpdateAutoTiles();
            var et = Time.Elapsed();
            Console.WriteLine($"Loaded {x}, {y}: {et - t}s");
        }
    }
}