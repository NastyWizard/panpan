
using System.Numerics;
using ImGuiNET.Backend.SDLGPU;
using GlmSharp;
using panpan;
using panpan.Rendering;
using panpan.Scene;
using panpan.Util;

namespace panpanExample
{
    public class SpriteEditor
    {
        private Entity? currentSelectedObject = null;
        private Entity? lastSelectedObject = null;
        private ImGuiTextureRef? cachedTextureRef;
        private Texture? cachedTexture;
        private float clipPreviewZoom = 1f;
        private Vector2 clipPreviewPan = Vector2.Zero;
        private bool clipPreviewPanActive = false;
        private Vector2 clipPreviewPanStart = Vector2.Zero;
        private Vector2 clipPreviewPanMouseStart = Vector2.Zero;
        private ClipHandle clipActiveHandle = ClipHandle.None;
        private Rect clipActiveStartRect;
        private Vector2 clipDragStartMouse = Vector2.Zero;
        private bool clipPreviewInitialized = false;
        private bool clipPreviewPixelPerfect = true;
        private bool originDragActive = false;
        private Vector2 originDragStart = Vector2.Zero;
        private Vector2 originOffsetStart = Vector2.Zero;

        public bool Visible = false;
        public void Show()
        {
            if (!Visible)
            {
                return;
            }

            bool showing = false;
            if (ImGui.Begin("Sprite Editor", ref showing))
            {
                ShowObjectSelect();
                ImGui.Separator();
                ShowSpriteClipper();
            }
            ImGui.End();
        }

        private void ShowObjectSelect()
        {
            if (currentSelectedObject == null)
            {
                currentSelectedObject = App.GetSceneManager().ActiveScene.Children[0];
            }

            string label = currentSelectedObject.GetType().Name;

            if (ImGui.BeginCombo("object", label))
            {
                var i = 0;
                foreach (var child in App.GetSceneManager().ActiveScene.Children)
                {
                    Type t = child.GetType();
                    bool isSelected = ReferenceEquals(child, currentSelectedObject);
                    if (ImGui.Selectable(t.Name + $" [{i++}]", ref isSelected))
                    {
                        if (!ReferenceEquals(currentSelectedObject, child))
                        {
                            currentSelectedObject = child;
                            OnSelectionChanged();
                        }
                    }
                }
                ImGui.EndCombo();
            }
        }

        private void ShowSpriteClipper()
        {
            if (currentSelectedObject == null)
            {
                return;
            }

            if (currentSelectedObject != lastSelectedObject)
            {
                OnSelectionChanged();
            }

            var spriteRenderer = currentSelectedObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                ImGui.Text("Selected object does not have a SpriteRenderer component.");
                return;
            }

            Texture? texture = spriteRenderer.Texture;
            if (texture == null)
            {
                ImGui.Text("SpriteRenderer does not have an assigned texture.");
                return;
            }

            ImGuiTextureRef textureRef = GetTextureRef(texture);

            int textureWidth = (int)texture.Width;
            int textureHeight = (int)texture.Height;

            Rect clipRect = spriteRenderer.ClipRect ?? new Rect(0, 0, textureWidth, textureHeight);
            Rect originalClip = clipRect;

            int clipX = clipRect.X;
            int clipY = clipRect.Y;
            int clipWidth = clipRect.Width;
            int clipHeight = clipRect.Height;

            bool clipChanged = false;
            clipChanged |= ImGui.InputInt("Clip X", ref clipX);
            clipChanged |= ImGui.InputInt("Clip Y", ref clipY);
            clipChanged |= ImGui.InputInt("Clip Width", ref clipWidth);
            clipChanged |= ImGui.InputInt("Clip Height", ref clipHeight);

            if (ImGui.Button("Reset Clip"))
            {
                clipX = 0;
                clipY = 0;
                clipWidth = textureWidth;
                clipHeight = textureHeight;
                clipChanged = true;
            }

            clipRect = new Rect(clipX, clipY, clipWidth, clipHeight);
            clipRect = ClampClipRect(clipRect, textureWidth, textureHeight);

            vec2 rendererOrigin = spriteRenderer.Origin;
            float originOffsetX = rendererOrigin.x * clipRect.Width;
            float originOffsetY = -rendererOrigin.y * clipRect.Height;
            ClampOrigin(ref originOffsetX, ref originOffsetY, clipRect.Width, clipRect.Height);

            bool originChanged = false;
            float originRangeX = MathF.Max(1f, clipRect.Width);
            float originRangeY = MathF.Max(1f, clipRect.Height);

            originChanged |= ImGui.SliderFloat("Origin X", ref originOffsetX, -originRangeX, originRangeX);
            originChanged |= ImGui.SliderFloat("Origin Y", ref originOffsetY, -originRangeY, originRangeY);
            if (clipPreviewPixelPerfect)
            {
                originOffsetX = MathF.Round(originOffsetX);
                originOffsetY = MathF.Round(originOffsetY);
            }
            ClampOrigin(ref originOffsetX, ref originOffsetY, clipRect.Width, clipRect.Height);

            if (ImGui.Button("Center Origin"))
            {
                originOffsetX = clipRect.Width / 2f;
                originOffsetY = clipRect.Height / 2f;
                originChanged = true;
            }
            ImGui.SameLine();
            if (ImGui.Button("Reset Origin"))
            {
                originOffsetX = 0f;
                originOffsetY = 0f;
                originChanged = true;
            }
            ClampOrigin(ref originOffsetX, ref originOffsetY, clipRect.Width, clipRect.Height);

            ImGui.SliderFloat("Zoom", ref clipPreviewZoom, 0.1f, 16f);
            clipPreviewZoom = Math.Clamp(clipPreviewZoom, 0.1f, 32f);
            if (clipPreviewPixelPerfect)
            {
                clipPreviewZoom = Math.Max(1f, MathF.Round(clipPreviewZoom));
            }
            ImGui.SameLine();
            if (ImGui.Button("Reset View"))
            {
                ResetViewState();
            }

            ClampOrigin(ref originOffsetX, ref originOffsetY, clipRect.Width, clipRect.Height);

            bool previewOriginChanged;
            if (DrawSpritePreview(texture, textureRef, ref clipRect, ref originOffsetX, ref originOffsetY, out previewOriginChanged))
            {
                clipChanged = true;
            }
            if (previewOriginChanged)
            {
                originChanged = true;
            }

            ClampOrigin(ref originOffsetX, ref originOffsetY, clipRect.Width, clipRect.Height);

            clipRect = ClampClipRect(clipRect, textureWidth, textureHeight);

            float newOriginX = clipRect.Width != 0 ? originOffsetX / clipRect.Width : 0f;
            float newOriginY = clipRect.Height != 0 ? -originOffsetY / clipRect.Height : 0f;

            if (clipChanged || !clipRect.Equals(originalClip))
            {
                spriteRenderer.Clip(clipRect);
                originChanged = true;
            }

            if (originChanged)
            {
                spriteRenderer.Origin = new vec2(newOriginX, newOriginY);
            }

            ImGui.Text($"Clip: ({clipRect.X}, {clipRect.Y}) {clipRect.Width}x{clipRect.Height}");
        }

        private ImGuiTextureRef GetTextureRef(Texture texture)
        {
            if (cachedTextureRef == null || cachedTexture != texture)
            {
                InvalidateTextureCache();
                ResetViewState();
                cachedTexture = texture;
                cachedTextureRef = ImGuiTextureRef.FromTexture(texture);
            }

            return cachedTextureRef!;
        }

        private void ResetViewState()
        {
            clipPreviewZoom = 10f;
            clipPreviewPan = Vector2.Zero;
            clipPreviewPanActive = false;
            clipPreviewPanStart = Vector2.Zero;
            clipPreviewPanMouseStart = Vector2.Zero;
            clipActiveHandle = ClipHandle.None;
            clipPreviewInitialized = false;
            originDragActive = false;
            originDragStart = Vector2.Zero;
            originOffsetStart = Vector2.Zero;
        }

        private void InvalidateTextureCache()
        {
            cachedTextureRef?.Dispose();
            cachedTextureRef = null;
            cachedTexture = null;
        }

        private void OnSelectionChanged()
        {
            InvalidateTextureCache();
            ResetViewState();
            lastSelectedObject = currentSelectedObject;
        }

        private bool DrawSpritePreview(Texture texture, ImGuiTextureRef textureRef, ref Rect clipRect, ref float originOffsetX, ref float originOffsetY, out bool originChanged)
        {
            bool changed = false;
            originChanged = false;

            Vector2 avail = ImGui.GetContentRegionAvail();
            float viewportWidth = MathF.Min(avail.X, 512f);
            if (viewportWidth <= 0f || float.IsNaN(viewportWidth) || float.IsInfinity(viewportWidth))
            {
                viewportWidth = 512f;
            }
            float viewportHeight = 384f;

            if (ImGui.BeginChild("SpritePreview", new Vector2(viewportWidth, viewportHeight), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                Vector2 canvasAvail = ImGui.GetContentRegionAvail();
                if (canvasAvail.X <= 0f || canvasAvail.Y <= 0f)
                {
                    canvasAvail = new Vector2(viewportWidth, viewportHeight);
                }
                canvasAvail = new Vector2(MathF.Max(canvasAvail.X, 1f), MathF.Max(canvasAvail.Y, 1f));

                ImGui.InvisibleButton("SpritePreviewCanvas", canvasAvail);
                Vector2 canvasMin = ImGui.GetItemRectMin();
                Vector2 canvasMax = ImGui.GetItemRectMax();
                Vector2 canvasSize = canvasMax - canvasMin;
                Vector2 mousePos = ImGui.GetMousePos();
                bool hovered = ImGui.IsItemHovered();

                clipPreviewZoom = MathF.Max(clipPreviewZoom, 0.05f);
                if (clipPreviewPixelPerfect)
                {
                    clipPreviewZoom = MathF.Max(1f, MathF.Round(clipPreviewZoom));
                }

                if (!clipPreviewInitialized)
                {
                    clipPreviewInitialized = true;
                }

                Vector2 textureSize = new Vector2(texture.Width, texture.Height);
                Vector2 imageSize = textureSize * clipPreviewZoom;
                if (clipPreviewPixelPerfect)
                {
                    imageSize = RoundVector(imageSize);
                    imageSize.X = MathF.Max(1f, imageSize.X);
                    imageSize.Y = MathF.Max(1f, imageSize.Y);
                }
                Vector2 basePan = (canvasSize - imageSize) * 0.5f;
                if (clipPreviewPixelPerfect)
                {
                    basePan = RoundVector(basePan);
                }
                Vector2 imageMin = canvasMin + basePan + clipPreviewPan;
                if (clipPreviewPixelPerfect)
                {
                    imageMin = RoundVector(imageMin);
                }
                Vector2 imageMax = imageMin + imageSize;

                var drawList = ImGui.GetWindowDrawList();
                bool drewImage = false;
                if (clipPreviewPixelPerfect && texture.HasPixelData)
                {
                    float zoom = MathF.Max(1f, clipPreviewZoom);
                    int texWidth = (int)texture.Width;
                    int texHeight = (int)texture.Height;

                    for (int py = 0; py < texHeight; py++)
                    {
                        for (int px = 0; px < texWidth; px++)
                        {
                            if (!texture.TryGetPixel(px, py, out byte r, out byte g, out byte b, out byte a))
                            {
                                continue;
                            }
                            if (a == 0)
                            {
                                continue;
                            }

                            Vector2 pixelMin = imageMin + new Vector2(px * zoom, py * zoom);
                            Vector2 pixelMax = pixelMin + new Vector2(zoom, zoom);
                            drawList.AddRectFilled(pixelMin, pixelMax, ImGui.ColorU32(r, g, b, a));
                        }
                    }

                    drewImage = true;
                }

                if (!drewImage)
                {
                    drawList.AddImage(textureRef, imageMin, imageMax);
                }

                bool panInitiated = hovered && (ImGui.IsMouseDown(ImGuiMouseButton.Middle) || ImGui.IsMouseDown(ImGuiMouseButton.Right));
                if (!clipPreviewPanActive && panInitiated)
                {
                    clipPreviewPanActive = true;
                    clipPreviewPanMouseStart = mousePos;
                    clipPreviewPanStart = clipPreviewPan;
                }

                if (clipPreviewPanActive)
                {
                    bool continuePan = ImGui.IsMouseDown(ImGuiMouseButton.Middle) || ImGui.IsMouseDown(ImGuiMouseButton.Right);
                    if (continuePan)
                    {
                        clipPreviewPan = clipPreviewPanStart + (mousePos - clipPreviewPanMouseStart);
                        if (clipPreviewPixelPerfect)
                        {
                            clipPreviewPan = RoundVector(clipPreviewPan);
                        }
                    }
                    else
                    {
                        clipPreviewPanActive = false;
                    }
                }

                Vector2 clipMin = imageMin + new Vector2(clipRect.X, clipRect.Y) * clipPreviewZoom;
                Vector2 clipMax = imageMin + new Vector2(clipRect.X + clipRect.Width, clipRect.Y + clipRect.Height) * clipPreviewZoom;
                if (clipPreviewPixelPerfect)
                {
                    clipMin = RoundVector(clipMin);
                    clipMax = RoundVector(clipMax);
                }

                Vector2 originCenter = imageMin + new Vector2(originOffsetX, originOffsetY) * clipPreviewZoom;
                if (clipPreviewPixelPerfect)
                {
                    originCenter = RoundVector(originCenter);
                }

                ClipHandle hoveredHandle = ClipHandle.None;
                bool originHovered = false;
                if (!clipPreviewPanActive && hovered)
                {
                    float crosshairRadius = MathF.Max(1f, 1f * clipPreviewZoom * 0.5f);
                    originHovered = Vector2.Distance(mousePos, originCenter) <= crosshairRadius;
                    if (!originHovered && !originDragActive)
                    {
                        hoveredHandle = GetHoveredHandle(mousePos, imageMin, imageMax, clipMin, clipMax);
                    }
                }

                if (clipActiveHandle == ClipHandle.None && hoveredHandle != ClipHandle.None && ImGui.IsMouseDown(ImGuiMouseButton.Left))
                {
                    clipActiveHandle = hoveredHandle;
                    clipDragStartMouse = mousePos;
                    clipActiveStartRect = clipRect;
                }
                else if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
                {
                    clipActiveHandle = ClipHandle.None;
                }

                if (clipActiveHandle != ClipHandle.None && ImGui.IsMouseDown(ImGuiMouseButton.Left))
                {
                    Vector2 deltaPixels = mousePos - clipDragStartMouse;
                    if (clipPreviewPixelPerfect)
                    {
                        deltaPixels = RoundVector(deltaPixels);
                    }
                    Vector2 deltaTex = deltaPixels / clipPreviewZoom;
                    if (clipPreviewPixelPerfect)
                    {
                        deltaTex = RoundVector(deltaTex);
                    }
                    Rect adjusted = ApplyHandleDelta(clipActiveStartRect, clipActiveHandle, deltaTex, (int)texture.Width, (int)texture.Height);
                    adjusted = ClampClipRect(adjusted, (int)texture.Width, (int)texture.Height);
                    if (!adjusted.Equals(clipRect))
                    {
                        clipRect = adjusted;
                        changed = true;
                    }
                }

                if (!originDragActive && originHovered && ImGui.IsMouseDown(ImGuiMouseButton.Left))
                {
                    originDragActive = true;
                    originDragStart = mousePos;
                    originOffsetStart = new Vector2(originOffsetX, originOffsetY);
                }

                if (originDragActive)
                {
                    if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
                    {
                        Vector2 deltaPixels = mousePos - originDragStart;
                        if (clipPreviewPixelPerfect)
                        {
                            deltaPixels = RoundVector(deltaPixels);
                        }
                        Vector2 deltaTex = deltaPixels / clipPreviewZoom;
                        if (clipPreviewPixelPerfect)
                        {
                            deltaTex = RoundVector(deltaTex);
                        }
                        originOffsetX = originOffsetStart.X + deltaTex.X;
                        originOffsetY = originOffsetStart.Y + deltaTex.Y;
                        ClampOrigin(ref originOffsetX, ref originOffsetY, clipRect.Width, clipRect.Height);
                        if (clipPreviewPixelPerfect)
                        {
                            originOffsetX = MathF.Round(originOffsetX);
                            originOffsetY = MathF.Round(originOffsetY);
                        }
                        originChanged = true;
                    }
                    else
                    {
                        originDragActive = false;
                    }
                }

                uint overlayColor = ImGui.ColorU32(255, 255, 255, 40);
                uint borderColor = ImGui.ColorU32(255, 200, 0);
                drawList.AddRectFilled(clipMin, clipMax, overlayColor);
                drawList.AddRect(clipMin, clipMax, borderColor, 2f);

                DrawHandles(drawList, clipMin, clipMax, hoveredHandle, clipActiveHandle);

                float crosshairLength = MathF.Max(1f, 1f * clipPreviewZoom);
                float crosshairThickness = MathF.Max(1f, clipPreviewZoom * 0.5f);
                uint crosshairColor = originDragActive ? ImGui.ColorU32(255, 128, 0) :
                    originHovered ? ImGui.ColorU32(255, 255, 0) : ImGui.ColorU32(0, 255, 255);

                drawList.AddLine(new Vector2(originCenter.X - crosshairLength, originCenter.Y),
                                 new Vector2(originCenter.X + crosshairLength, originCenter.Y),
                                 crosshairColor, crosshairThickness);
                drawList.AddLine(new Vector2(originCenter.X, originCenter.Y - crosshairLength),
                                 new Vector2(originCenter.X, originCenter.Y + crosshairLength),
                                 crosshairColor, crosshairThickness);
                Vector2 boxHalf = new Vector2(MathF.Max(1f, crosshairThickness * 1.5f));
                drawList.AddRect(originCenter - boxHalf, originCenter + boxHalf, crosshairColor, crosshairThickness);

                ImGui.EndChild();
            }
            else
            {
                ImGui.EndChild();
            }

            return changed;
        }

        private void DrawHandles(ImDrawListWrapper drawList, Vector2 clipMin, Vector2 clipMax, ClipHandle hovered, ClipHandle active)
        {
            float size = 8f;
            Vector2 half = new Vector2(size * 0.5f);

            var handles = new (Vector2 center, ClipHandle handle)[]
            {
                (clipMin, ClipHandle.TopLeft),
                (new Vector2(clipMax.X, clipMin.Y), ClipHandle.TopRight),
                (new Vector2(clipMin.X, clipMax.Y), ClipHandle.BottomLeft),
                (clipMax, ClipHandle.BottomRight),
                (new Vector2((clipMin.X + clipMax.X) * 0.5f, clipMin.Y), ClipHandle.Top),
                (new Vector2((clipMin.X + clipMax.X) * 0.5f, clipMax.Y), ClipHandle.Bottom),
                (new Vector2(clipMin.X, (clipMin.Y + clipMax.Y) * 0.5f), ClipHandle.Left),
                (new Vector2(clipMax.X, (clipMin.Y + clipMax.Y) * 0.5f), ClipHandle.Right)
            };

            uint inactiveColor = ImGui.ColorU32(200, 200, 200, 200);
            uint hoveredColor = ImGui.ColorU32(255, 255, 255, 230);
            uint activeColor = ImGui.ColorU32(255, 180, 0);
            uint outlineColor = ImGui.ColorU32(0, 0, 0);

            foreach (var entry in handles)
            {
                Vector2 min = entry.center - half;
                Vector2 max = entry.center + half;
                bool isActive = entry.handle == active;
                bool isHovered = entry.handle == hovered;
                uint fill = isActive ? activeColor : isHovered ? hoveredColor : inactiveColor;
                drawList.AddRectFilled(min, max, fill);
                drawList.AddRect(min, max, outlineColor, 1f);
            }
        }

        private ClipHandle GetHoveredHandle(Vector2 mouse, Vector2 imageMin, Vector2 imageMax, Vector2 clipMin, Vector2 clipMax)
        {
            if (!PointInRect(mouse, imageMin, imageMax))
            {
                return ClipHandle.None;
            }

            float handleSize = 10f;
            float edgeThreshold = handleSize;
            Vector2 half = new Vector2(handleSize * 0.5f);

            Vector2 topRight = new Vector2(clipMax.X, clipMin.Y);
            Vector2 bottomLeft = new Vector2(clipMin.X, clipMax.Y);

            if (PointInRect(mouse, clipMin - half, clipMin + half)) return ClipHandle.TopLeft;
            if (PointInRect(mouse, topRight - half, topRight + half)) return ClipHandle.TopRight;
            if (PointInRect(mouse, bottomLeft - half, bottomLeft + half)) return ClipHandle.BottomLeft;
            if (PointInRect(mouse, clipMax - half, clipMax + half)) return ClipHandle.BottomRight;

            if (MathF.Abs(mouse.X - clipMin.X) <= edgeThreshold && mouse.Y >= clipMin.Y && mouse.Y <= clipMax.Y) return ClipHandle.Left;
            if (MathF.Abs(mouse.X - clipMax.X) <= edgeThreshold && mouse.Y >= clipMin.Y && mouse.Y <= clipMax.Y) return ClipHandle.Right;
            if (MathF.Abs(mouse.Y - clipMin.Y) <= edgeThreshold && mouse.X >= clipMin.X && mouse.X <= clipMax.X) return ClipHandle.Top;
            if (MathF.Abs(mouse.Y - clipMax.Y) <= edgeThreshold && mouse.X >= clipMin.X && mouse.X <= clipMax.X) return ClipHandle.Bottom;

            if (PointInRect(mouse, clipMin, clipMax))
            {
                return ClipHandle.Move;
            }

            return ClipHandle.None;
        }

        private static bool PointInRect(Vector2 point, Vector2 min, Vector2 max)
        {
            float minX = MathF.Min(min.X, max.X);
            float minY = MathF.Min(min.Y, max.Y);
            float maxX = MathF.Max(min.X, max.X);
            float maxY = MathF.Max(min.Y, max.Y);
            return point.X >= minX && point.X <= maxX && point.Y >= minY && point.Y <= maxY;
        }

        private static Rect ApplyHandleDelta(Rect startRect, ClipHandle handle, Vector2 deltaTex, int texWidth, int texHeight)
        {
            int deltaX = (int)MathF.Round(deltaTex.X);
            int deltaY = (int)MathF.Round(deltaTex.Y);

            int startLeft = startRect.X;
            int startTop = startRect.Y;
            int startRight = startRect.X + startRect.Width;
            int startBottom = startRect.Y + startRect.Height;

            Rect result = startRect;

            switch (handle)
            {
                case ClipHandle.Move:
                    result.X = Math.Clamp(startLeft + deltaX, 0, Math.Max(0, texWidth - startRect.Width));
                    result.Y = Math.Clamp(startTop + deltaY, 0, Math.Max(0, texHeight - startRect.Height));
                    break;
                case ClipHandle.Left:
                case ClipHandle.TopLeft:
                case ClipHandle.BottomLeft:
                    int newLeft = Math.Clamp(startLeft + deltaX, 0, startRight - 1);
                    result.X = newLeft;
                    result.Width = startRight - newLeft;
                    break;
                case ClipHandle.Right:
                case ClipHandle.TopRight:
                case ClipHandle.BottomRight:
                    int newRight = Math.Clamp(startRight + deltaX, startLeft + 1, texWidth);
                    result.Width = newRight - startLeft;
                    break;
            }

            switch (handle)
            {
                case ClipHandle.Top:
                case ClipHandle.TopLeft:
                case ClipHandle.TopRight:
                    int newTop = Math.Clamp(startTop + deltaY, 0, startBottom - 1);
                    result.Y = newTop;
                    result.Height = startBottom - newTop;
                    break;
                case ClipHandle.Bottom:
                case ClipHandle.BottomLeft:
                case ClipHandle.BottomRight:
                    int newBottom = Math.Clamp(startBottom + deltaY, startTop + 1, texHeight);
                    result.Height = newBottom - result.Y;
                    break;
            }

            result.Width = Math.Max(1, result.Width);
            result.Height = Math.Max(1, result.Height);
            return ClampClipRect(result, texWidth, texHeight);
        }

        private static Rect ClampClipRect(Rect rect, int textureWidth, int textureHeight)
        {
            rect.Width = Math.Max(1, rect.Width);
            rect.Height = Math.Max(1, rect.Height);

            int maxX = Math.Max(0, textureWidth - rect.Width);
            int maxY = Math.Max(0, textureHeight - rect.Height);

            rect.X = Math.Clamp(rect.X, 0, maxX);
            rect.Y = Math.Clamp(rect.Y, 0, maxY);

            rect.Width = Math.Clamp(rect.Width, 1, Math.Max(1, textureWidth - rect.X));
            rect.Height = Math.Clamp(rect.Height, 1, Math.Max(1, textureHeight - rect.Y));

            return rect;
        }

        private static Vector2 RoundVector(Vector2 value)
        {
            return new Vector2(MathF.Round(value.X), MathF.Round(value.Y));
        }

        private static void ClampOrigin(ref float originX, ref float originY, int clipWidth, int clipHeight)
        {
            float limitX = MathF.Max(1f, clipWidth);
            float limitY = MathF.Max(1f, clipHeight);
            originX = Math.Clamp(originX, -limitX, limitX);
            originY = Math.Clamp(originY, -limitY, limitY);
        }

        private enum ClipHandle
        {
            None,
            Move,
            Left,
            Right,
            Top,
            Bottom,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }
    }
}