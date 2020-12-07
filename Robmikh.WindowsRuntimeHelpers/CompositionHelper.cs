using System;
using System.Runtime.InteropServices;
using Windows.UI.Composition;
using Windows.UI.Composition.Desktop;
using WinRT;

namespace Robmikh.WindowsRuntimeHelpers
{
    public static class CompositionHelper
    {
        [ComImport]
        [Guid("25297D5C-3AD4-4C9C-B5CF-E36A38512330")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface ICompositorInterop
        {
            void CreateCompositionSurfaceForHandle(
                IntPtr swapChain, out IntPtr/*ICompositionSurface*/ surface);

            void CreateCompositionSurfaceForSwapChain(
                IntPtr swapChain, out IntPtr/*ICompositionSurface*/ surface);

            void CreateGraphicsDevice(
                IntPtr renderingDevice, out IntPtr/*CompositionGraphicsDevice*/ device);
        }

        [ComImport]
        [Guid("29E691FA-4567-4DCA-B319-D0F207EB6807")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface ICompositorDesktopInterop
        {
            void CreateDesktopWindowTarget(
                IntPtr hwnd,
                bool isTopmost,
                out IntPtr/*Windows.UI.Composition.Desktop.DesktopWindowTarget*/ target);
        }

        public static CompositionTarget CreateDesktopWindowTarget(this Compositor compositor, IntPtr hwnd, bool isTopmost)
        {
            var desktopInterop = compositor.As<ICompositorDesktopInterop>();
            desktopInterop.CreateDesktopWindowTarget(hwnd, isTopmost, out var raw);
            var result = DesktopWindowTarget.FromAbi(raw);
            Marshal.Release(raw);
            return result;
        }

        public static ICompositionSurface CreateCompositionSurfaceForSwapChain(this Compositor compositor, SharpDX.DXGI.SwapChain1 swapChain)
        {
            var interop = compositor.As<ICompositorInterop>();
            interop.CreateCompositionSurfaceForSwapChain(swapChain.NativePointer, out var raw);
            var result = MarshalInterface<ICompositionSurface>.FromAbi(raw);
            Marshal.Release(raw);
            return result;
        }
    }
}
