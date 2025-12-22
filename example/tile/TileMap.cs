
using panpan.Rendering;
using panpan.Scene;
using GlmSharp;
using panpan.Util;
using System.Diagnostics.Tracing;
using System.Collections.Generic;
using panpan;

namespace panpanExample
{
    public class TilePool
    {
        private static Stack<Tile> pool = new Stack<Tile>();
        
        public static Tile RequestTile(int x, int y, TileSet ts)
        {
            Tile t;
            if(pool.Count > 0)
            {
                t = pool.Pop();
                t.Reuse(x, y, ts);
            }
            else
            {
                t = new Tile(x, y, ts);
                t.Init();
            }

            return t;
        }

        public static void FillPool(int count)
        {
            Console.WriteLine($"Filling pool {count}");
            for(int i = 0; i < count; i++)
            {
                var tile = new Tile(-100,-100);
                tile.Init();
                pool.Push(tile);
            }
        }

        public static void ReturnTile(Tile t)
        {
            t.Position.x = -100;
            t.Position.y = -100;
            pool.Push(t);
        }
    }

    public class TileMap : Entity
    {
        private Tile[,] tileMap;
        private string[,] dataMap;
        private List<Tile> tiles;
        private int width;
        private int height;
        private int x;
        private int y;
        private bool loaded = false;
        private bool wasInView = false;

        public int Width => width;
        public int Height => height;
        public int X => x;
        public int Y => y;
        public int TileSize = 8;
        public bool IsInView = false;
        public bool Loaded => loaded;

        public bool drawLights = true;
        private FireFly[] fireFlies;
        public TileMap(int x, int y, int w, int h)
        {
            width = w;
            height = h;
            this.x = x;
            this.y = y;
            Position.xy = new vec2(x * 320, y * 176);
            tileMap = new Tile[w,h];
            dataMap = new string[w,h];
            tiles = new List<Tile>();

            fireFlies = new FireFly[16];
            var r = new Random();
            for(int i = 0; i < 16; i++)
            {
                fireFlies[i] = new FireFly((int)(r.NextInt64(0,320) + Position.x),(int)(r.NextInt64(0,176) + Position.y));
            }
        }

        public Tile AddTile(uint x, uint y, TileSet tileSet)
        {
            if(tileMap[x, y] != null)
                return tileMap[x, y];
            Tile tile = TilePool.RequestTile((int)x*8 + (int)Position.x, (int)y*8 + (int)Position.y, tileSet);
            tileMap[x, y] = tile;
            dataMap[x, y] = tileSet.index.ToString();
            tiles.Add(tile);

            return tile;
        }

        public Tile GetTile(uint x, uint y)
        {
            return tileMap[x,y];
        }
        public string GetTileType(uint x, uint y)
        {
            return dataMap[x,y];
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
                // Remove collider from collision manager before returning to pool
                if(tile.collider != null)
                {
                    App.GetCollisionManager().RemoveCollider(tile.collider);
                }
                TilePool.ReturnTile(tile);
                tileMap[x, y] = null;
                dataMap[x, y] = " ";
            }
        }

        public void UpdateAutoTiles()
        {
            var t = Time.Elapsed();
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

                    if(tile.CurrentFrame != autoTile)
                        tile.renderer.Clip(tSet.clips[autoTile]);
                    tile.CurrentFrame = autoTile;
                }
            }
            var et = Time.Elapsed();
            Console.WriteLine($"Updated Autotile {x}, {y}: {et - t}s");
        }

        public override void Init()
        {
            base.Init();
        }

        public override void Update()
        {
            base.Update();
            IsInView = InView();
            
            // Handle going out of view - return tiles to pool
            if(wasInView && !IsInView && loaded)
            {
                ReturnAllTilesToPool();
            }
            
            // Handle coming into view - load tiles
            if(!loaded && IsInView)
            {
                LoadFromData(TileMapData.mapData[x][y]);
            }
            
            // Only update tiles that are in view
            if(IsInView)
            {
                foreach(var tile in tiles)
                {
                    tile.Update();
                }
            }
            
            wasInView = IsInView;
        }
        public override void Render()
        {
            if(IsInView)
            {
                base.Render();
                foreach(var tile in tiles)
                {
                    tile.Render();
                }
            }
        }

        public void DrawLights()
        {
            if(!IsInView || !drawLights)
                return;
            Draw.Sprite(GameTextures.fsLight, Position.xy + vec2.UnitY * 176);
            for(int i = 0; i < 16; i++)
            {
                fireFlies[i].DrawLights();
            }
        }

        public void DrawBounds(vec4 color)
        {
            Draw.Rect(new Rect((int)Position.x-1, (int)Position.y, width*TileSize+1, height*TileSize+1), color);
        }

        private bool InView()
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
                    var ti = dataMap[x, y];
                    str += $"'{ti}',";
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
                    var d = data[(int)x][y];
                    dataMap[x, y] = d;
                    switch(d)
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
            var et = Time.Elapsed();
            Console.WriteLine($"Loaded {x}, {y}: {et - t}s");
            UpdateAutoTiles();
            loaded = true;
        }

        private void ReturnAllTilesToPool()
        {
            for(uint y = 0; y < height; y++)
            {
                for(uint x = 0; x < width; x++)
                {
                    var tile = tileMap[x, y];
                    if(tile != null)
                    {
                        if(tile.collider != null)
                        {
                            App.GetCollisionManager().RemoveCollider(tile.collider);
                        }
                        TilePool.ReturnTile(tile);
                        tileMap[x, y] = null;
                    }
                }
            }
            tiles.Clear();
            loaded = false;
        }
    }
}