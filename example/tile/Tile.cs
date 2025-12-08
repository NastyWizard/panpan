
using panpan.Rendering;
using panpan.Scene;
using panpan.Assets;
using GlmSharp;
using panpan.Collision;
using panpan;

namespace panpanExample
{
    public class Tile : Entity
    {
        public SpriteRenderer renderer = null!;
        public TileSet tileSet;
        public BoxCollider collider = null!;
        public Tile(int x, int y, TileSet? tileSet = null)
        {
            tileSet ??= TileSets.Dirt;
            this.tileSet = tileSet.Value;
            Position.xy = new vec2(x, y);
        }

        public override void Init()
        {
            renderer = (SpriteRenderer)AddComponent(new SpriteRenderer(TileSets.TilesetTexture));
            renderer.Clip(tileSet.clips[15]);
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

        public void SetTileSet(TileSet tileSet)
        {
            this.tileSet = tileSet;
        }

        public void Reuse(int x, int y, TileSet ts)
        {
            tileSet = ts;
            Position.xy = new vec2(x, y);
            // Update renderer clip for new tileSet
            if(renderer != null)
            {
                renderer.Clip(ts.clips[15]);
            }
            // Update collision bounds and spatial hash when position changes
            if(collider != null)
            {
                collider.UpdateBounds();
                // Re-add to spatial hash to update cell positions
                App.GetCollisionManager().AddCollider(collider);
            }
        }
    }
}