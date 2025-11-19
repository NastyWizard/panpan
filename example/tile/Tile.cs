
using panpan.Rendering;
using panpan.Scene;
using panpan.Assets;
using GlmSharp;
using panpan.Collision;

namespace panpanExample
{
    public class Tile : Entity
    {
        public SpriteRenderer renderer = null!;
        public TileSet tileSet;
        BoxCollider collider;
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

            collider = (BoxCollider)AddComponent(new BoxCollider(8, 8));
            collider.SetOffset(0,-7);
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
    }
}