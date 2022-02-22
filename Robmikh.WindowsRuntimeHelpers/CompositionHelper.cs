﻿using Windows.Win32.Foundation;
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
            //var ptr = Marshal.GetComInterfaceForObject<IDXGISwapChain1, IDXGISwapChain1>(swapChain);
            interop.CreateCompositionSurfaceForSwapChain(swapChain, out var raw);
            return MarshalInterface<ICompositionSurface>.FromAbi(Marshal.GetIUnknownForObject(raw));
        }
    }
}
