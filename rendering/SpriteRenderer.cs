
using GlmSharp;

namespace panpan.Rendering
{
    public class SpriteRenderer : MeshRenderer
    {
        public SpriteRenderer(Texture? tex, Mesh? mesh = null, Material? mat = null)
            : base(mesh ?? Shapes.quad, mat ?? RenderUtil.DefaultMaterial)
        {
            if (tex != null)
            {
                SetTexture(tex);
            }
        }
    }
}