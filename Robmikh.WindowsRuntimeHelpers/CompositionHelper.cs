using Windows.Win32.Foundation;
using Windows.Win32.System.WinRT;
using Windows.Win32.Graphics.Dxgi;
using Windows.Win32.System.WinRT.Composition;
using System.Runtime.InteropServices;
using Windows.UI.Composition;
using WinRT;

namespace Robmikh.WindowsRuntimeHelpers
{
    public static class CompositionHelper
    {
        public static CompositionTarget CreateDesktopWindowTarget(this Compositor compositor, HWND hwnd, bool isTopmost)
        {
            var desktopInterop = compositor.As<ICompositorDesktopInterop>();
            desktopInterop.CreateDesktopWindowTarget(hwnd, isTopmost, out var target);
            return target;
        }

        public static ICompositionSurface CreateCompositionSurfaceForSwapChain(this Compositor compositor, IDXGISwapChain1 swapChain)
        {
            var interop = compositor.As<ICompositorInterop>();
            interop.CreateCompositionSurfaceForSwapChain(swapChain, out var raw);
            var rawPtr = Marshal.GetIUnknownForObject(raw);
            var result = MarshalInterface<ICompositionSurface>.FromAbi(rawPtr);
            Marshal.Release(rawPtr);
            return result;
        }
    }
}
