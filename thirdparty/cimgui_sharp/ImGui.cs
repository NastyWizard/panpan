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

    [Flags]
    internal enum ImGuiComboFlags
    {
        None = 0
    }

    [Flags]
    internal enum ImGuiSelectableFlags
    {
        None = 0
    }

    [Flags]
    internal enum ImGuiChildFlags
    {
        None = 0,
        Border = 1 << 0
    }

    [Flags]
    internal enum ImGuiHoveredFlags
    {
        None = 0
    }

    internal enum ImGuiMouseButton
    {
        Left = 0,
        Right = 1,
        Middle = 2
    }

    internal static class ImGui
    {
        private static readonly SWIGTYPE_p_bool NullBool = new SWIGTYPE_p_bool(IntPtr.Zero, false);
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

        public static void Separator() => cimgui_sdlgpu.igSeparator();

        public static void SameLine() => cimgui_sdlgpu.igSameLine(0,10);

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
            int* flagsPtr = stackalloc int[1];
            flagsPtr[0] = 0;
            var sliderFlags = new SWIGTYPE_p_ImGuiSliderFlags((nint)flagsPtr, false);
            bool changed = cimgui_sdlgpu.igSliderFloat(label, swigValue, vMin, vMax, format, sliderFlags);
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

        public static unsafe bool InputInt(string label, ref int value, int step = 1, int stepFast = 100)
        {
            int* valuePtr = stackalloc int[1];
            valuePtr[0] = value;
            var valueSwig = new SWIGTYPE_p_int((nint)valuePtr, false);
            int* flagsPtr = stackalloc int[1];
            flagsPtr[0] = 0;
            var flagsSwig = new SWIGTYPE_p_ImGuiInputTextFlags((nint)flagsPtr, false);
            bool changed = cimgui_sdlgpu.igInputInt(label, valueSwig, step, stepFast, flagsSwig);
            value = valuePtr[0];
            return changed;
        }

        public static unsafe bool BeginChild(string id, Vector2 size, bool border = false, ImGuiWindowFlags windowFlags = ImGuiWindowFlags.None)
        {
            float* sizePtr = stackalloc float[2];
            sizePtr[0] = size.X;
            sizePtr[1] = size.Y;
            var sizeVec = new SWIGTYPE_p_ImVec2((nint)sizePtr, false);

            int* childFlagsPtr = stackalloc int[1];
            childFlagsPtr[0] = border ? (int)ImGuiChildFlags.Border : (int)ImGuiChildFlags.None;
            var childFlags = new SWIGTYPE_p_ImGuiChildFlags((nint)childFlagsPtr, false);

            int* windowFlagsPtr = stackalloc int[1];
            windowFlagsPtr[0] = (int)windowFlags;
            var windowFlagsSwig = new SWIGTYPE_p_ImGuiWindowFlags((nint)windowFlagsPtr, false);

            return cimgui_sdlgpu.igBeginChild_Str(id, sizeVec, childFlags, windowFlagsSwig);
        }

        public static void EndChild() => cimgui_sdlgpu.igEndChild();

        public static unsafe bool InvisibleButton(string id, Vector2 size)
        {
            float* sizePtr = stackalloc float[2];
            sizePtr[0] = size.X;
            sizePtr[1] = size.Y;
            var sizeVec = new SWIGTYPE_p_ImVec2((nint)sizePtr, false);

            int* flagsPtr = stackalloc int[1];
            flagsPtr[0] = 0;
            var flags = new SWIGTYPE_p_ImGuiButtonFlags((nint)flagsPtr, false);
            return cimgui_sdlgpu.igInvisibleButton(id, sizeVec, flags);
        }

        public static unsafe Vector2 GetContentRegionAvail()
        {
            float* vec = stackalloc float[2];
            var swig = new SWIGTYPE_p_ImVec2((nint)vec, false);
            cimgui_sdlgpu.igGetContentRegionAvail(swig);
            return new Vector2(vec[0], vec[1]);
        }

        public static unsafe Vector2 GetCursorScreenPos()
        {
            float* vec = stackalloc float[2];
            var swig = new SWIGTYPE_p_ImVec2((nint)vec, false);
            cimgui_sdlgpu.igGetCursorScreenPos(swig);
            return new Vector2(vec[0], vec[1]);
        }

        public static unsafe void SetCursorScreenPos(Vector2 pos)
        {
            float* vec = stackalloc float[2];
            vec[0] = pos.X;
            vec[1] = pos.Y;
            var swig = new SWIGTYPE_p_ImVec2((nint)vec, false);
            cimgui_sdlgpu.igSetCursorScreenPos(swig);
        }

        public static unsafe Vector2 GetItemRectMin()
        {
            float* vec = stackalloc float[2];
            var swig = new SWIGTYPE_p_ImVec2((nint)vec, false);
            cimgui_sdlgpu.igGetItemRectMin(swig);
            return new Vector2(vec[0], vec[1]);
        }

        public static unsafe Vector2 GetItemRectMax()
        {
            float* vec = stackalloc float[2];
            var swig = new SWIGTYPE_p_ImVec2((nint)vec, false);
            cimgui_sdlgpu.igGetItemRectMax(swig);
            return new Vector2(vec[0], vec[1]);
        }

        public static unsafe bool IsItemHovered(ImGuiHoveredFlags flags = ImGuiHoveredFlags.None)
        {
            int* flagsPtr = stackalloc int[1];
            flagsPtr[0] = (int)flags;
            var swigFlags = new SWIGTYPE_p_ImGuiHoveredFlags((nint)flagsPtr, false);
            return cimgui_sdlgpu.igIsItemHovered(swigFlags);
        }

        public static bool IsItemActive() => cimgui_sdlgpu.igIsItemActive();

        public static unsafe bool IsMouseDown(ImGuiMouseButton button)
        {
            int* buttonPtr = stackalloc int[1];
            buttonPtr[0] = (int)button;
            var swigButton = new SWIGTYPE_p_ImGuiMouseButton((nint)buttonPtr, false);
            return cimgui_sdlgpu.igIsMouseDown_Nil(swigButton);
        }

        public static unsafe bool IsMouseDragging(ImGuiMouseButton button, float lockThreshold = -1f)
        {
            int* buttonPtr = stackalloc int[1];
            buttonPtr[0] = (int)button;
            var swigButton = new SWIGTYPE_p_ImGuiMouseButton((nint)buttonPtr, false);
            return cimgui_sdlgpu.igIsMouseDragging(swigButton, lockThreshold);
        }

        public static unsafe Vector2 GetMouseDragDelta(ImGuiMouseButton button, float lockThreshold = -1f)
        {
            float* vec = stackalloc float[2];
            var swigVec = new SWIGTYPE_p_ImVec2((nint)vec, false);
            int* buttonPtr = stackalloc int[1];
            buttonPtr[0] = (int)button;
            var swigButton = new SWIGTYPE_p_ImGuiMouseButton((nint)buttonPtr, false);
            cimgui_sdlgpu.igGetMouseDragDelta(swigVec, swigButton, lockThreshold);
            return new Vector2(vec[0], vec[1]);
        }

        public static unsafe void ResetMouseDragDelta(ImGuiMouseButton button)
        {
            int* buttonPtr = stackalloc int[1];
            buttonPtr[0] = (int)button;
            var swigButton = new SWIGTYPE_p_ImGuiMouseButton((nint)buttonPtr, false);
            cimgui_sdlgpu.igResetMouseDragDelta(swigButton);
        }

        public static unsafe Vector2 GetMousePos()
        {
            float* vec = stackalloc float[2];
            var swig = new SWIGTYPE_p_ImVec2((nint)vec, false);
            cimgui_sdlgpu.igGetMousePos(swig);
            return new Vector2(vec[0], vec[1]);
        }

        public static ImDrawListWrapper GetWindowDrawList()
        {
            var handle = cimgui_sdlgpu.igGetWindowDrawList();
            return new ImDrawListWrapper(handle);
        }

        public static uint ColorU32(byte r, byte g, byte b, byte a = 255) =>
            (uint)(r | (uint)(g << 8) | (uint)(b << 16) | (uint)(a << 24));

        public static bool RadioButton(string label, ref bool active)
        {
            if (cimgui_sdlgpu.igRadioButton_Bool(label, active))
            {
                active = !active;
            }
            return active;
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

        public static bool BeginMainMenuBar()
        {
            return cimgui_sdlgpu.igBeginMainMenuBar();
        }

        public static void EndMainMenuBar()
        {
            cimgui_sdlgpu.igEndMainMenuBar();
        }

        public static bool BeginMenuBar()
        {
            return cimgui_sdlgpu.igBeginMenuBar();
        }

        public static void EndMenuBar()
        {
            cimgui_sdlgpu.igEndMenuBar();
        }

        public static bool BeginMenu(string label, bool enabled = true)
        {
            return cimgui_sdlgpu.igBeginMenu(label, enabled);
        }

        public static void EndMenu()
        {
            cimgui_sdlgpu.igEndMenu();
        }

        public static bool MenuItem(string label, string? shortcut = null, bool selected = false, bool enabled = true)
        {
            return cimgui_sdlgpu.igMenuItem_Bool(label, shortcut ?? string.Empty, selected, enabled);
        }

        public static bool BeginCombo(string label, string previewValue, ImGuiComboFlags flags = ImGuiComboFlags.None)
        {
            int rawFlags = (int)flags;
            unsafe
            {
                int* ptr = &rawFlags;
                var swigFlags = new SWIGTYPE_p_ImGuiComboFlags((nint)ptr, false);
                return cimgui_sdlgpu.igBeginCombo(label, previewValue, swigFlags);
            }
        }

        public static void EndCombo() => cimgui_sdlgpu.igEndCombo();

        public static bool Selectable(string label, ref bool selected, ImGuiSelectableFlags flags = ImGuiSelectableFlags.None, Vector2? size = null)
        {
            int rawFlags = (int)flags;
            unsafe
            {
                int* flagPtr = &rawFlags;
                var swigFlags = new SWIGTYPE_p_ImGuiSelectableFlags((nint)flagPtr, false);

                Vector2 sizeValue = size ?? Vector2.Zero;
                float* sizePtr = stackalloc float[2];
                sizePtr[0] = sizeValue.X;
                sizePtr[1] = sizeValue.Y;
                var sizeVec = new SWIGTYPE_p_ImVec2((nint)sizePtr, false);

                bool changed = cimgui_sdlgpu.igSelectable_Bool(label, selected, swigFlags, sizeVec);
                if (changed)
                {
                    selected = !selected;
                }
                return changed;
            }
        }

        public static void ShowDemoWindow()
        {
            cimgui_sdlgpu.igShowDemoWindow(null);
        }
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

    internal sealed class ImDrawListWrapper
    {
        private readonly SWIGTYPE_p_ImDrawList? handle;

        internal ImDrawListWrapper(SWIGTYPE_p_ImDrawList? handle)
        {
            this.handle = handle;
        }

        public unsafe void AddImage(ImGuiTextureRef textureRef, Vector2 min, Vector2 max, Vector2? uv0 = null, Vector2? uv1 = null, uint color = 0xFFFFFFFF)
        {
            if (handle == null || textureRef.Handle == null)
            {
                return;
            }

            float* minPtr = stackalloc float[2];
            minPtr[0] = min.X;
            minPtr[1] = min.Y;
            var minVec = new SWIGTYPE_p_ImVec2((nint)minPtr, false);

            float* maxPtr = stackalloc float[2];
            maxPtr[0] = max.X;
            maxPtr[1] = max.Y;
            var maxVec = new SWIGTYPE_p_ImVec2((nint)maxPtr, false);

            Vector2 uv0Value = uv0 ?? Vector2.Zero;
            float* uv0Ptr = stackalloc float[2];
            uv0Ptr[0] = uv0Value.X;
            uv0Ptr[1] = uv0Value.Y;
            var uv0Vec = new SWIGTYPE_p_ImVec2((nint)uv0Ptr, false);

            Vector2 uv1Value = uv1 ?? Vector2.One;
            float* uv1Ptr = stackalloc float[2];
            uv1Ptr[0] = uv1Value.X;
            uv1Ptr[1] = uv1Value.Y;
            var uv1Vec = new SWIGTYPE_p_ImVec2((nint)uv1Ptr, false);

            uint* colorPtr = stackalloc uint[1];
            colorPtr[0] = color;
            var colorVec = new SWIGTYPE_p_ImU32((nint)colorPtr, false);

            cimgui_sdlgpu.ImDrawList_AddImage(handle, textureRef.Handle, minVec, maxVec, uv0Vec, uv1Vec, colorVec);
        }

        public unsafe void AddRect(Vector2 min, Vector2 max, uint color, float thickness = 1f)
        {
            if (handle == null)
            {
                return;
            }

            float* minPtr = stackalloc float[2];
            minPtr[0] = min.X;
            minPtr[1] = min.Y;
            var minVec = new SWIGTYPE_p_ImVec2((nint)minPtr, false);

            float* maxPtr = stackalloc float[2];
            maxPtr[0] = max.X;
            maxPtr[1] = max.Y;
            var maxVec = new SWIGTYPE_p_ImVec2((nint)maxPtr, false);

            uint* colorPtr = stackalloc uint[1];
            colorPtr[0] = color;
            var colorVec = new SWIGTYPE_p_ImU32((nint)colorPtr, false);

            int* flagsPtr = stackalloc int[1];
            flagsPtr[0] = 0;
            var flags = new SWIGTYPE_p_ImDrawFlags((nint)flagsPtr, false);

            cimgui_sdlgpu.ImDrawList_AddRect(handle, minVec, maxVec, colorVec, 0f, flags, thickness);
        }

        public unsafe void AddRectFilled(Vector2 min, Vector2 max, uint color)
        {
            if (handle == null)
            {
                return;
            }

            float* minPtr = stackalloc float[2];
            minPtr[0] = min.X;
            minPtr[1] = min.Y;
            var minVec = new SWIGTYPE_p_ImVec2((nint)minPtr, false);

            float* maxPtr = stackalloc float[2];
            maxPtr[0] = max.X;
            maxPtr[1] = max.Y;
            var maxVec = new SWIGTYPE_p_ImVec2((nint)maxPtr, false);

            uint* colorPtr = stackalloc uint[1];
            colorPtr[0] = color;
            var colorVec = new SWIGTYPE_p_ImU32((nint)colorPtr, false);

            int* flagsPtr = stackalloc int[1];
            flagsPtr[0] = 0;
            var flags = new SWIGTYPE_p_ImDrawFlags((nint)flagsPtr, false);

            cimgui_sdlgpu.ImDrawList_AddRectFilled(handle, minVec, maxVec, colorVec, 0f, flags);
        }

        public unsafe void AddLine(Vector2 p1, Vector2 p2, uint color, float thickness = 1f)
        {
            if (handle == null)
            {
                return;
            }

            float* p1Ptr = stackalloc float[2];
            p1Ptr[0] = p1.X;
            p1Ptr[1] = p1.Y;
            var p1Vec = new SWIGTYPE_p_ImVec2((nint)p1Ptr, false);

            float* p2Ptr = stackalloc float[2];
            p2Ptr[0] = p2.X;
            p2Ptr[1] = p2.Y;
            var p2Vec = new SWIGTYPE_p_ImVec2((nint)p2Ptr, false);

            uint* colorPtr = stackalloc uint[1];
            colorPtr[0] = color;
            var colorVec = new SWIGTYPE_p_ImU32((nint)colorPtr, false);

            cimgui_sdlgpu.ImDrawList_AddLine(handle, p1Vec, p2Vec, colorVec, thickness);
        }
    }
}

