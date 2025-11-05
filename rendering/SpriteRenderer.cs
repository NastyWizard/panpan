
using GlmSharp;

namespace panpan.Rendering
{
    public class SpriteRenderer : MeshRenderer
    {
        public SpriteRenderer(Texture? tex) : base(Shapes.quad, RenderUtil.DefaultMaterial)
        {
            if (tex != null)
            {
                SetTexture(tex);
            }
        }
    }
}