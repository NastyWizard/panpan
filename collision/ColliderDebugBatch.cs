using System.Collections.Generic;
using System.Text;
using GlmSharp;
using panpan.Rendering;
using panpan.Scene;
using panpan.Util;

namespace panpan.Collision
{
    internal static class ColliderDebugBatch
    {
        private struct RectRequest
        {
            public Rect Rect;
            public vec4 Color;
        }

        private struct PixelRequest
        {
            public vec2 Position;
            public vec4 Color;
        }

        private static readonly List<RectRequest> rects = new List<RectRequest>();
        private static readonly List<PixelRequest> pixels = new List<PixelRequest>();
        private static readonly ColliderDebugRenderer renderer = new ColliderDebugRenderer();

        public static void BeginFrame()
        {
            rects.Clear();
            pixels.Clear();
        }

        public static void SubmitRect(Rect rect, vec4 color)
        {
            var r = new Rect(rect.X,rect.Y-1,rect.Width,rect.Height);
            rects.Add(new RectRequest { Rect = r, Color = color });
        }

        public static void SubmitPixel(vec2 position, vec4 color)
        {
            pixels.Add(new PixelRequest { Position = position - vec2.UnitY, Color = color });
        }

        public static void Flush()
        {
            CollisionManager collisionManager = App.GetCollisionManager();
            if (collisionManager == null || !collisionManager.ShowColliderDebug)
            {
                rects.Clear();
                pixels.Clear();
                return;
            }

            if (rects.Count == 0 && pixels.Count == 0)
            {
                return;
            }

            List<Vertex> vertices = new List<Vertex>((rects.Count * 4 + pixels.Count) * 4);
            List<uint> indices = new List<uint>((rects.Count * 4 + pixels.Count) * 6);
            uint indexOffset = 0;

            foreach (RectRequest request in rects)
            {
                AddRectOutline(request.Rect, request.Color, vertices, indices, ref indexOffset);
            }

            foreach (PixelRequest request in pixels)
            {
                AddQuad(request.Position.x, request.Position.y, request.Color, vertices, indices, ref indexOffset);
            }

            if (vertices.Count == 0 || indices.Count == 0)
            {
                rects.Clear();
                pixels.Clear();
                return;
            }

            renderer.UpdateGeometry(vertices.ToArray(), indices.ToArray());
            Scene.Scene activeScene = App.GetSceneManager().ActiveScene;
            activeScene.Camera.PushUniformData();
            renderer.RenderBatch();

            rects.Clear();
            pixels.Clear();
        }

        private static void AddRectOutline(Rect rect, vec4 color, List<Vertex> vertices, List<uint> indices, ref uint indexOffset)
        {
            int width = Math.Max(0, rect.Width);
            int height = Math.Max(0, rect.Height);
            int baseX = rect.X;
            int baseY = rect.Y;

            for (int x = 0; x <= width; x++)
            {
                AddQuad(baseX + x, baseY, color, vertices, indices, ref indexOffset);
                AddQuad(baseX + x, baseY + height, color, vertices, indices, ref indexOffset);
            }

            for (int y = 0; y <= height; y++)
            {
                AddQuad(baseX, baseY + y, color, vertices, indices, ref indexOffset);
                AddQuad(baseX + width, baseY + y, color, vertices, indices, ref indexOffset);
            }
        }

        private static void AddQuad(float x, float y, vec4 color, List<Vertex> vertices, List<uint> indices, ref uint indexOffset)
        {
            float r = color.x;
            float g = color.y;
            float b = color.z;
            float a = color.w;

            vertices.Add(new Vertex(x, y + 1f, 0f, r, g, b, a));
            vertices.Add(new Vertex(x + 1f, y + 1f, 0f, r, g, b, a));
            vertices.Add(new Vertex(x, y, 0f, r, g, b, a));
            vertices.Add(new Vertex(x + 1f, y, 0f, r, g, b, a));

            indices.Add(indexOffset + 0);
            indices.Add(indexOffset + 2);
            indices.Add(indexOffset + 1);
            indices.Add(indexOffset + 1);
            indices.Add(indexOffset + 2);
            indices.Add(indexOffset + 3);
            indexOffset += 4;
        }

        private sealed class ColliderDebugRenderer : MeshRenderer
        {
            private static readonly byte[] DebugVertBytes = Encoding.UTF8.GetBytes(@"
struct VertexInput
{
    float3 position : POSITION;
    float4 color    : COLOR0;
    float2 uv       : TEXCOORD0;
};

struct VertexOutput
{
    float4 position : SV_POSITION;
    float4 color    : COLOR0;
    float2 uv       : TEXCOORD0;
};

cbuffer CameraBuffer : register(b0, space1)
{
    float4x4 viewProjection;
}

cbuffer GlobalUniforms : register(b1, space1)
{
    float4x4 model;
}

VertexOutput main(VertexInput input)
{
    VertexOutput output;
    output.position = mul(mul(float4(input.position, 1.0f), model), viewProjection);
    output.color = input.color;
    output.uv = input.uv;
    return output;
}
");

            private static readonly byte[] DebugFragBytes = Encoding.UTF8.GetBytes(@"
struct FragInput
{
    float4 color : COLOR0;
    float2 uv    : TEXCOORD0;
};

float4 main(FragInput input) : SV_TARGET
{
    return input.color;
}
");

            public ColliderDebugRenderer()
                : base(
                    new Mesh(
                        new[]
                        {
                            new Vertex(0f, 1f, 0f, 1f, 1f, 1f, 1f),
                            new Vertex(1f, 1f, 0f, 1f, 1f, 1f, 1f),
                            new Vertex(0f, 0f, 0f, 1f, 1f, 1f, 1f),
                            new Vertex(1f, 0f, 0f, 1f, 1f, 1f, 1f),
                        },
                        new uint[] { 0, 2, 1, 1, 2, 3 }),
                    new Material(DebugFragBytes, DebugVertBytes))
            {
                Width = 1f;
                Height = 1f;
                Origin = vec2.Zero;
                transform.Position = new vec3(0f, 0f, 0f);
                transform.Scale = vec3.Ones;
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

