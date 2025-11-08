using System;
using System.Runtime.CompilerServices;
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
    }
}

