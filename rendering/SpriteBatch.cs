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
            public vec2 Scale;
            public vec3 Rotation;
            public vec3 Origin;
            public Rect Crop;
            public vec4 Color;
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
                mat = new Material();
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

        public DrawBatchRenderer GetRenderer()
        {
            return renderer;
        }

        public void SetOrigin (vec2 o)
        {
            renderer.Origin = o;
        }

        public void BeginFrame()
        {
            sprites.Clear();
        }

        public void SubmitSprite(vec3 pos, Rect crop, vec2? scale = null, vec3? rotation = null, vec3? origin = null)
        {
            scale ??= vec2.Ones;
            rotation ??= vec3.Zero;
            origin ??= vec3.Zero;
            sprites.Add(new SpriteRequest { Position = pos, Scale = scale.Value, Rotation = rotation.Value, Origin = origin.Value, Crop = crop, Color = Color.White });
        }
        public void SubmitSprite(vec3 pos, Rect crop, vec4 color, vec2? scale = null, vec3? rotation = null, vec3? origin = null)
        {
            scale ??= vec2.Ones;
            rotation ??= vec3.Zero;
            origin ??= vec3.Zero;
            sprites.Add(new SpriteRequest { Position = pos, Scale = scale.Value, Rotation = rotation.Value, Origin = origin.Value, Crop = crop, Color = color });
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

            sprites.Sort(delegate (SpriteRequest a, SpriteRequest b)
            {
                if(a.Position.z == b.Position.z) return 0;
                else if(a.Position.z > b.Position.z) return -1;
                else return 1;
            });

            foreach (SpriteRequest request in sprites)
            {
                AddSprite(request.Position, request.Scale, request.Rotation, request.Origin, request.Crop, request.Color, vertices, indices, ref indexOffset);
            }

            if (vertices.Count == 0 || indices.Count == 0)
            {
                sprites.Clear();
                return;
            }

            renderer.UpdateGeometry(vertices.ToArray(), indices.ToArray());
            renderer.RenderBatch();

            sprites.Clear();
        }

        private void AddSprite(vec3 position, vec2 scale, vec3 rotation, vec3 origin, Rect crop, vec4 color, List<Vertex> vertices, List<uint> indices, ref uint indexOffset)
        {
            float r = color.r;
            float g = color.g;
            float b = color.b;
            float a = color.a;

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

            float width = crop.Width*scale.x;
            float height = crop.Height*scale.y;


            vec4[] verts =
            {
              new vec4(0,height,0,1),
              new vec4(width,height,0,1),
              new vec4(0,0,0,1),
              new vec4(width,0,0,1),  
            };

            if(rotation != vec3.Zero)
            {
                mat4 rotx = new mat4(
                    1,  0,                     0,                        0,
                    0,  MathF.Cos(rotation.x), -MathF.Sin(rotation.x),   0,
                    0,  MathF.Sin(rotation.x), MathF.Cos(rotation.x),    0,
                    0,  0,                     0,                        1
                );
                
                mat4 roty = new mat4(
                    MathF.Cos(rotation.y),  0,  MathF.Sin(rotation.y),    0,
                    0,                      1,  0,                        0,
                    -MathF.Sin(rotation.y), 0,  MathF.Cos(rotation.y),    0,
                    0,                      0,  0,                        1
                );
                
                mat4 rotz = new mat4(
                    MathF.Cos(rotation.z),  -MathF.Sin(rotation.z),  0,    0,
                    MathF.Sin(rotation.z),  MathF.Cos(rotation.z),   0,    0,
                    0,                      0,                       0,    0,
                    0,                      0,                       0,    1
                );

                for(int i = 0; i < 4; i++)
                {
                    verts[i] -= new vec4(origin.x,origin.y,origin.z,0);
                    verts[i] = rotx * verts[i];
                    verts[i] = roty * verts[i];
                    verts[i] = rotz * verts[i];
                }
            }
            else if(origin != vec3.Zero)
            {
                for(int i = 0; i < 4; i++)
                {
                    verts[i] -= new vec4(origin.x,origin.y,origin.z,0);   
                }
            }


            vertices.Add(new Vertex(x + verts[0].x, y + verts[0].y, z + verts[0].z, r, g, b, a, uvs[0].x, uvs[0].y)); // tl
            vertices.Add(new Vertex(x + verts[1].x, y + verts[1].y, z + verts[1].x, r, g, b, a, uvs[1].x, uvs[1].y)); // tr
            vertices.Add(new Vertex(x + verts[2].x, y + verts[2].y, z + verts[2].z, r, g, b, a, uvs[2].x, uvs[2].y)); // bl
            vertices.Add(new Vertex(x + verts[3].x, y + verts[3].y, z + verts[3].z, r, g, b, a, uvs[3].x, uvs[3].y)); // br

            indices.Add(indexOffset + 0);
            indices.Add(indexOffset + 2);
            indices.Add(indexOffset + 1);
            indices.Add(indexOffset + 1);
            indices.Add(indexOffset + 2);
            indices.Add(indexOffset + 3);
            indexOffset += 4;
        }

        public sealed class DrawBatchRenderer : MeshRenderer
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

