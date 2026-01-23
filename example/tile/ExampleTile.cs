
using panpan.Rendering;
using panpan.Scene;
using panpan.Assets;
using GlmSharp;
using panpan.Collision;
using panpan;

namespace panpanExample
{
    public class ExampleTile : Entity
    {
        public SpriteRenderer renderer = null!;
        public ExampleTileSet tileSet;
        public BoxCollider collider = null!;
        public int CurrentFrame = -1;
        public ExampleTile(int x, int y, ExampleTileSet? tileSet = null)
        {
            tileSet ??= new Random().Next(0,1) == 1 ? ExampleTileSets.Dirt : ExampleTileSets.Brick;
            this.tileSet = tileSet.Value;
            Position.xy = new vec2(x, y);
        }

        public override void Init()
        {
            renderer = (SpriteRenderer)AddComponent(new SpriteRenderer(ExampleTileSets.TilesetTexture));
            CurrentFrame = 15;
            renderer.Clip(tileSet.clips[CurrentFrame]);
            renderer.Origin = new vec2(0,-8.0f/8.0f);

            collider = (BoxCollider)AddComponent(new BoxCollider(8, 8));
            collider.SetOffset(0,1);
            base.Init();
        }

        public override void Update()
        {
            base.Update();
        }
        public override void Render()
        {
            base.Render();
            //collider.DrawDebug();
        }

        public void SetTileSet(ExampleTileSet tileSet)
        {
            this.tileSet = tileSet;
        }

        public void Reuse(int x, int y, ExampleTileSet ts)
        {
            if(ts.index != tileSet.index)
            {
                CurrentFrame = 15;
                renderer.Clip(ts.clips[CurrentFrame]);
            }
            tileSet = ts;
            Position.xy = new vec2(x, y);
            if(collider != null)
            {
                App.GetCollisionManager().UpdateCollider(collider);
            }
        }
    }
}