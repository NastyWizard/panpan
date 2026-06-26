
using GlmSharp;
using panpan.Util;

namespace panpan.Rendering
{
    public class SpriteRenderer : MeshRenderer
    {
        private Rect? clip;
        public SpriteRenderer(Texture? tex, Rect? clip = null, Mesh? mesh = null, Material? mat = null)
            : base(mesh ?? Shapes.quad, mat ?? RenderUtil.DefaultMaterial)
        {
            if (tex != null)
            {
                SetTexture(tex);
            }
            if (clip != null)
            {
                MakeClip(clip.Value);
            }
        }
        private void MakeClip(int x, int y, int width, int height)
        {
            MakeClip(new Rect(x, y, width, height));
        }
        private void MakeClip(Rect rect)
        {
            clip = rect;
            Width = rect.Width;
            Height = rect.Height;
            SetMesh(Shapes.ClipQuad(rect, texture!.Width, texture.Height));
        }

        public void Clip(Rect? rect)
        {
            if(rect == null)
            {
                clip = null;
                Width = texture!.Width;
                Height = texture!.Height;
                SetMesh(Shapes.quad);
                return;
            }

            if (clip == null)
            {
                MakeClip(rect.Value);
                return;
            }

            clip = rect.Value;
            Width = rect.Value.Width;
            Height = rect.Value.Height;
            mesh.Clip(rect.Value, texture!.Width, texture.Height);
        }
        public void Clip(int x, int y, int width, int height) => Clip(new Rect(x, y, width, height));

        public Texture? Texture => texture;
        public Rect? ClipRect => clip;
    }
}