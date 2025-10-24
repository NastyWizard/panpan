
namespace panpan.Rendering
{
    public class SpriteRenderer : MeshRenderer
    {
        public SpriteRenderer(Texture tex) : base(Shapes.quad, RenderUtil.DefaultMaterial)
        {
            SetTexture(tex);
            Width = tex.Width;
            Height = tex.Height;
        }
    }
}