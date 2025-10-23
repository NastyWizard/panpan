
using panpan.Rendering;
using panpan.Scene;
using panpan.Assets;

namespace panpanExample
{
    public class TestBox : Entity
    {
        MeshRenderer renderer;
        public override void Init()
        {
            renderer = (MeshRenderer)AddComponent(new MeshRenderer(Shapes.quad, new Material()));
            renderer.SetTexture(new Texture(Sprites.test, 72, 72));
            base.Init();
        }
    }
}