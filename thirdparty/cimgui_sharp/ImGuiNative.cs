using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SDL3;
using ImGuiNET.Backend.SDLGPU;

namespace panpan.Rendering
{
    [Flags]
    internal enum ImGuiWindowFlags
    {
        None = 0,
        NoTitleBar = 1 << 0,
        NoResize = 1 << 1,
        NoMove = 1 << 2,
        NoScrollbar = 1 << 3,
        NoScrollWithMouse = 1 << 4,
        NoCollapse = 1 << 5,
        AlwaysAutoResize = 1 << 6,
        NoBackground = 1 << 7,
        NoSavedSettings = 1 << 8,
        NoMouseInputs = 1 << 9,
        MenuBar = 1 << 10,
        HorizontalScrollbar = 1 << 11,
        NoFocusOnAppearing = 1 << 12,
        NoBringToFrontOnFocus = 1 << 13,
        AlwaysVerticalScrollbar = 1 << 14,
        AlwaysHorizontalScrollbar = 1 << 15,
        AlwaysUseWindowPadding = 1 << 16,
        NoNavInputs = 1 << 18,
        NoNavFocus = 1 << 19,
        UnsavedDocument = 1 << 20,
        NoDocking = 1 << 21
    }

    internal static class ImGui
    {
        private static readonly SWIGTYPE_p_bool NullBool = new SWIGTYPE_p_bool(IntPtr.Zero, false);
        private static readonly SWIGTYPE_p_ImGuiSliderFlags NullSliderFlags = new SWIGTYPE_p_ImGuiSliderFlags(IntPtr.Zero, false);
        private static readonly SWIGTYPE_p_ImVec2 NullVec2 = new SWIGTYPE_p_ImVec2(IntPtr.Zero, false);
        private static readonly SWIGTYPE_p_ImVec4 NullVec4 = new SWIGTYPE_p_ImVec4(IntPtr.Zero, false);

        public static unsafe bool Begin(string name, ImGuiWindowFlags flags = ImGuiWindowFlags.None)
        {
            int* flagsPtr = stackalloc int[1];
            flagsPtr[0] = (int)flags;
            var swigFlags = new SWIGTYPE_p_ImGuiWindowFlags((nint)flagsPtr, false);
            return cimgui_sdlgpu.igBegin(name, NullBool, swigFlags);
        }

        public static unsafe bool Begin(string name, ref bool isOpen, ImGuiWindowFlags flags = ImGuiWindowFlags.None)
        {
            byte* openPtr = stackalloc byte[1];
            openPtr[0] = isOpen ? (byte)1 : (byte)0;
            var swigOpen = new SWIGTYPE_p_bool((nint)openPtr, false);

            int* flagsPtr = stackalloc int[1];
            flagsPtr[0] = (int)flags;
            var swigFlags = new SWIGTYPE_p_ImGuiWindowFlags((nint)flagsPtr, false);

            bool result = cimgui_sdlgpu.igBegin(name, swigOpen, swigFlags);
            isOpen = openPtr[0] != 0;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void End() => cimgui_sdlgpu.igEnd();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Text(string text) => cimgui_sdlgpu.igText(text);

        public static unsafe bool Checkbox(string label, ref bool value)
        {
            byte* valuePtr = stackalloc byte[1];
            valuePtr[0] = value ? (byte)1 : (byte)0;
            var swigValue = new SWIGTYPE_p_bool((nint)valuePtr, false);
            bool changed = cimgui_sdlgpu.igCheckbox(label, swigValue);
            value = valuePtr[0] != 0;
            return changed;
        }

        public static unsafe bool SliderFloat(string label, ref float v, float vMin, float vMax, string format = "%.3f")
        {
            float* valuePtr = stackalloc float[1];
            valuePtr[0] = v;
            var swigValue = new SWIGTYPE_p_float((nint)valuePtr, false);
            bool changed = cimgui_sdlgpu.igSliderFloat(label, swigValue, vMin, vMax, format, NullSliderFlags);
            v = valuePtr[0];
            return changed;
        }

        public static unsafe bool Button(string label, float width = 0f, float height = 0f)
        {
            float* vecPtr = stackalloc float[2];
            vecPtr[0] = width;
            vecPtr[1] = height;
            var size = new SWIGTYPE_p_ImVec2((nint)vecPtr, false);
            return cimgui_sdlgpu.igButton(label, size);
        }

        public static unsafe void Image(ImGuiTextureRef textureRef, Vector2 size, Vector2? uv0 = null, Vector2? uv1 = null)
        {
            if (textureRef == null || textureRef.Handle == null)
            {
                return;
            }

            float* sizePtr = stackalloc float[2];
            sizePtr[0] = size.X;
            sizePtr[1] = size.Y;
            var sizeVec = new SWIGTYPE_p_ImVec2((nint)sizePtr, false);

            Vector2 uv0Value = uv0 ?? Vector2.Zero;
            Vector2 uv1Value = uv1 ?? Vector2.One;

            float* uv0Ptr = stackalloc float[2];
            uv0Ptr[0] = uv0Value.X;
            uv0Ptr[1] = uv0Value.Y;
            var uv0Vec = new SWIGTYPE_p_ImVec2((nint)uv0Ptr, false);

            float* uv1Ptr = stackalloc float[2];
            uv1Ptr[0] = uv1Value.X;
            uv1Ptr[1] = uv1Value.Y;
            var uv1Vec = new SWIGTYPE_p_ImVec2((nint)uv1Ptr, false);

            cimgui_sdlgpu.igImage(textureRef.Handle, sizeVec, uv0Vec, uv1Vec);
        }

        public static unsafe void Image(ImGuiTextureRef textureRef, float width, float height) =>
            Image(textureRef, new Vector2(width, height));

        public static unsafe void ImageWithBg(ImGuiTextureRef textureRef, Vector2 size, Vector2? uv0 = null, Vector2? uv1 = null, Vector4? background = null, Vector4? tint = null)
        {
            if (textureRef == null || textureRef.Handle == null)
            {
                return;
            }

            float* sizePtr = stackalloc float[2];
            sizePtr[0] = size.X;
            sizePtr[1] = size.Y;
            var sizeVec = new SWIGTYPE_p_ImVec2((nint)sizePtr, false);

            Vector2 uv0Value = uv0 ?? Vector2.Zero;
            Vector2 uv1Value = uv1 ?? Vector2.One;

            float* uv0Ptr = stackalloc float[2];
            uv0Ptr[0] = uv0Value.X;
            uv0Ptr[1] = uv0Value.Y;
            var uv0Vec = new SWIGTYPE_p_ImVec2((nint)uv0Ptr, false);

            float* uv1Ptr = stackalloc float[2];
            uv1Ptr[0] = uv1Value.X;
            uv1Ptr[1] = uv1Value.Y;
            var uv1Vec = new SWIGTYPE_p_ImVec2((nint)uv1Ptr, false);

            Vector4 bgValue = background ?? Vector4.Zero;
            float* bgPtr = stackalloc float[4];
            bgPtr[0] = bgValue.X;
            bgPtr[1] = bgValue.Y;
            bgPtr[2] = bgValue.Z;
            bgPtr[3] = bgValue.W;
            var bgVec = new SWIGTYPE_p_ImVec4((nint)bgPtr, false);

            Vector4 tintValue = tint ?? Vector4.One;
            float* tintPtr = stackalloc float[4];
            tintPtr[0] = tintValue.X;
            tintPtr[1] = tintValue.Y;
            tintPtr[2] = tintValue.Z;
            tintPtr[3] = tintValue.W;
            var tintVec = new SWIGTYPE_p_ImVec4((nint)tintPtr, false);

            cimgui_sdlgpu.igImageWithBg(textureRef.Handle, sizeVec, uv0Vec, uv1Vec, bgVec, tintVec);
        }

        public static void ImageWithBg(ImGuiTextureRef textureRef, float width, float height, Vector4? background = null, Vector4? tint = null) =>
            ImageWithBg(textureRef, new Vector2(width, height), null, null, background, tint);

        public static unsafe bool ImageButton(string id, ImGuiTextureRef textureRef, Vector2 size, Vector2? uv0 = null, Vector2? uv1 = null, Vector4? background = null, Vector4? tint = null)
        {
            if (textureRef == null || textureRef.Handle == null)
            {
                return false;
            }

            float* sizePtr = stackalloc float[2];
            sizePtr[0] = size.X;
            sizePtr[1] = size.Y;
            var sizeVec = new SWIGTYPE_p_ImVec2((nint)sizePtr, false);

            Vector2 uv0Value = uv0 ?? Vector2.Zero;
            float* uv0Ptr = stackalloc float[2];
            uv0Ptr[0] = uv0Value.X;
            uv0Ptr[1] = uv0Value.Y;

            Vector2 uv1Value = uv1 ?? Vector2.One;
            float* uv1Ptr = stackalloc float[2];
            uv1Ptr[0] = uv1Value.X;
            uv1Ptr[1] = uv1Value.Y;

            Vector4 bgValue = background ?? Vector4.Zero;
            float* bgPtr = stackalloc float[4];
            bgPtr[0] = bgValue.X;
            bgPtr[1] = bgValue.Y;
            bgPtr[2] = bgValue.Z;
            bgPtr[3] = bgValue.W;

            Vector4 tintValue = tint ?? Vector4.One;
            float* tintPtr = stackalloc float[4];
            tintPtr[0] = tintValue.X;
            tintPtr[1] = tintValue.Y;
            tintPtr[2] = tintValue.Z;
            tintPtr[3] = tintValue.W;

            var uv0Vec = new SWIGTYPE_p_ImVec2((nint)uv0Ptr, false);
            var uv1Vec = new SWIGTYPE_p_ImVec2((nint)uv1Ptr, false);
            var bgVec = new SWIGTYPE_p_ImVec4((nint)bgPtr, false);
            var tintVec = new SWIGTYPE_p_ImVec4((nint)tintPtr, false);

            return cimgui_sdlgpu.igImageButton(id, textureRef.Handle, sizeVec, uv0Vec, uv1Vec, bgVec, tintVec);
        }

        public static unsafe bool ImageButton(string id, ImGuiTextureRef textureRef, float width, float height, Vector4? background = null, Vector4? tint = null) =>
            ImageButton(id, textureRef, new Vector2(width, height), null, null, background, tint);
    }

    internal sealed class ImGuiTextureRef : IDisposable
    {
        private GCHandle bindingHandle;
        private SDL.GPUTextureSamplerBinding[]? bindingArray;
        internal SWIGTYPE_p_ImTextureRef? Handle { get; private set; }

        private ImGuiTextureRef(SDL.GPUTextureSamplerBinding binding)
        {
            bindingArray = new SDL.GPUTextureSamplerBinding[1];
            bindingArray[0] = binding;
            bindingHandle = GCHandle.Alloc(bindingArray, GCHandleType.Pinned);

            var texId = new SWIGTYPE_p_ImTextureID(bindingHandle.AddrOfPinnedObject(), false);
            Handle = cimgui_sdlgpu.ImTextureRef_ImTextureRef_TextureID(texId);
        }

        public static ImGuiTextureRef FromTexture(Texture texture)
        {
            if (texture == null)
            {
                throw new ArgumentNullException(nameof(texture));
            }

            texture.CopyPass();

            SDL.GPUTextureSamplerBinding binding = new SDL.GPUTextureSamplerBinding
            {
                Texture = texture.GPUTexture,
                Sampler = texture.GPUSampler
            };
            return new ImGuiTextureRef(binding);
        }

        public static ImGuiTextureRef FromBinding(SDL.GPUTextureSamplerBinding binding) => new ImGuiTextureRef(binding);

        public static ImGuiTextureRef FromHandles(nint texture, nint sampler) =>
            new ImGuiTextureRef(new SDL.GPUTextureSamplerBinding
            {
                Texture = texture,
                Sampler = sampler
            });

        public void Dispose()
        {
            if (Handle != null)
            {
                cimgui_sdlgpu.ImTextureRef_destroy(Handle);
                Handle = null;
            }

            if (bindingHandle.IsAllocated)
            {
                bindingHandle.Free();
            }

            bindingArray = null;
        }
    }
}

