using System.ComponentModel;
using System.Text;
using GlmSharp;
using panpan.Assets;
using panpan.Util;

namespace panpan.Rendering
{
    public class SpriteBatch : Component
    {
        private struct SpriteRequest
        {
            public vec3 Position;
            public Rect Crop;
        }

        private readonly List<SpriteRequest> sprites;
        private readonly DrawBatchRenderer renderer;
        private Texture texture;
        private vec2 textureSize;

        public SpriteBatch(Texture? texture, Material? mat = null)
        {
            if(texture != null)
            {
                this.texture = texture;
                textureSize = new vec2(this.texture.Width, this.texture.Height);
            }

            if(mat == null)
            {
                mat = new Material(Shaders.standard_frag_hlsl, Shaders.standard_vert_hlsl);
            }

            renderer = new DrawBatchRenderer(texture, mat);
            sprites = new List<SpriteRequest>();
        }

        public void SetTexture(Texture tex)
        {
            this.texture = tex;
            textureSize = new vec2(this.texture.Width, this.texture.Height);
            renderer.SetTexture(tex);
        }

        public void SetOrigin (vec2 o)
        {
            renderer.Origin = o;
        }

        public void BeginFrame()
        {
            sprites.Clear();
        }

        public void SubmitSprite(vec3 pos, Rect crop)
        {
            sprites.Add(new SpriteRequest { Position = pos, Crop = crop });
        }

        public void Render()
        {
            if (sprites.Count == 0)
            {
                return;
            }

            List<Vertex> vertices = new List<Vertex>(sprites.Count * 4 * 4);
            List<uint> indices = new List<uint>(sprites.Count * 4 * 6);
            uint indexOffset = 0;

            foreach (SpriteRequest request in sprites)
            {
                AddSprite(request.Position, request.Crop, vertices, indices, ref indexOffset);
            }

            if (vertices.Count == 0 || indices.Count == 0)
            {
                sprites.Clear();
                return;
            }

            renderer.UpdateGeometry(vertices.ToArray(), indices.ToArray());
            //Scene.Scene activeScene = App.GetSceneManager().ActiveScene;
            //activeScene.ActiveCamera.PushUniformData();
            renderer.RenderBatch();

            sprites.Clear();
        }

        private void AddSprite(vec3 position, Rect crop, List<Vertex> vertices, List<uint> indices, ref uint indexOffset)
        {
            float r = 1.0f;
            float g = 1.0f;
            float b = 1.0f;
            float a = 1.0f;

            float x = position.x;
            float y = position.y;
            float z = position.z;

            vec2[] uvs =
            {
                new vec2(crop.X,                crop.Y) / textureSize, // br
                new vec2(crop.X + crop.Width,   crop.Y) / textureSize, // br
                new vec2(crop.X,                crop.Y + crop.Height) / textureSize, // tl
                new vec2(crop.X + crop.Width,   crop.Y + crop.Height) / textureSize // tr
            };

            vertices.Add(new Vertex(x, y + crop.Height, z, r, g, b, a, uvs[0].x, uvs[0].y)); // tl
            vertices.Add(new Vertex(x + crop.Width, y + crop.Height, z, r, g, b, a, uvs[1].x, uvs[1].y)); // tr
            vertices.Add(new Vertex(x, y, z, r, g, b, a, uvs[2].x, uvs[2].y)); // bl
            vertices.Add(new Vertex(x + crop.Width, y, z, r, g, b, a, uvs[3].x, uvs[3].y)); // br

            indices.Add(indexOffset + 0);
            indices.Add(indexOffset + 2);
            indices.Add(indexOffset + 1);
            indices.Add(indexOffset + 1);
            indices.Add(indexOffset + 2);
            indices.Add(indexOffset + 3);
            indexOffset += 4;
        }

        private sealed class DrawBatchRenderer : MeshRenderer
        {

            public DrawBatchRenderer(Texture? texture, Material mat)
                : base(
                    new Mesh(
                        new Vertex[]
                        {
                            new Vertex(0f, 1f, 0f, 1f, 1f, 1f, 1f),
                            new Vertex(1f, 1f, 0f, 1f, 1f, 1f, 1f),
                            new Vertex(0f, 0f, 0f, 1f, 1f, 1f, 1f),
                            new Vertex(1f, 0f, 0f, 1f, 1f, 1f, 1f),
                        },
                        new uint[] { 0, 2, 1, 1, 2, 3 }),
                    mat)
            {
                Width = 1f;
                Height = 1f;
                Origin = vec2.Zero;
                transform.Position = new vec3(0f, 0f, 0f);
                transform.Scale = vec3.Ones;
                this.texture = texture;
            }

            public void UpdateGeometry(Vertex[] vertices, uint[] indices)
            {
                mesh.SetData(vertices, indices);
            }

            public void RenderBatch()
            {
                Render();
            }
        }
    }
}

