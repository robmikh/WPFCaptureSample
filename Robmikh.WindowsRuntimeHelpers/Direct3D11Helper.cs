using Windows.Win32.Graphics.Direct3D;
using Windows.Win32.Graphics.Direct3D11;
using Windows.Win32.Graphics.Dxgi;
using Windows.Win32.System.WinRT;
using System;
using Windows.Graphics.DirectX.Direct3D11;
using WinRT;
using static Windows.Win32.PInvoke;
using IInspectable = Windows.Win32.System.WinRT.IInspectable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32.System.WinRT.Direct3D11;

namespace Robmikh.WindowsRuntimeHelpers
{
    public static class Direct3D11Helper
    {
        private class NullHandle : SafeHandle
        {
            public NullHandle() : base(IntPtr.Zero, false)
            {
            }

            public override bool IsInvalid => true;

            protected override bool ReleaseHandle()
            {
                // Do nothing
                return true;
            }
        }


        // TODO: Why arn't these being generated?
        static uint D3D11_SDK_VERSION = 7;
        static int DXGI_ERROR_UNSUPPORTED = -2005270524;
        public static uint DXGI_USAGE_RENDER_TARGET_OUTPUT = (uint)( 1L << (1 + 4) );

        private static ID3D11Device CreateD3DDevice(D3D_DRIVER_TYPE driverType, D3D11_CREATE_DEVICE_FLAG flags)
        {
            unsafe
            {
                D3D11CreateDevice(null, driverType, new NullHandle(), flags, null, D3D11_SDK_VERSION, out var device, null, out var context);
                return device;
            }
        }

        public static IDirect3DDevice CreateDevice()
        {
            ID3D11Device d3dDevice = null;
            var flags = D3D11_CREATE_DEVICE_FLAG.D3D11_CREATE_DEVICE_BGRA_SUPPORT;
            try
            {
                d3dDevice = CreateD3DDevice(D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_HARDWARE, flags);
            }
            catch (Exception ex)
            {
                if (ex.HResult != DXGI_ERROR_UNSUPPORTED)
                {
                    throw;
                }
            }

            if (d3dDevice == null)
            {
                d3dDevice = CreateD3DDevice(D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_WARP, flags);
            }

            return CreateDirect3DDeviceFromD3D11Device(d3dDevice);
        }

        public static IDirect3DDevice CreateDirect3DDeviceFromD3D11Device(ID3D11Device d3dDevice)
        {
            var dxgiDevice = d3dDevice.As<IDXGIDevice>();
            CreateDirect3D11DeviceFromDXGIDevice(dxgiDevice, out var raw);
            return MarshalInterface<IDirect3DDevice>.FromAbi(Marshal.GetIUnknownForObject(raw));
        }

        public static IDirect3DSurface CreateDirect3DSurfaceFromD3D11Texture2D(ID3D11Texture2D texture)
        {
            var dxgiSurface = texture.As<IDXGISurface>();
            CreateDirect3D11SurfaceFromDXGISurface(dxgiSurface, out var raw);
            return MarshalInterface<IDirect3DSurface>.FromAbi(Marshal.GetIUnknownForObject(raw));
        }

        public static T GetDXGIInterfaceFromObject<T>(object obj)
        {
            var access = obj.As<IDirect3DDxgiInterfaceAccess>();
            object result = null;
            unsafe
            {
                var guid = typeof(T).GUID;
                var guidPointer = (Guid*)Unsafe.AsPointer(ref guid);
                access.GetInterface(guidPointer, out result);
            }
            return result.As<T>();
        }

        public static ID3D11Device GetD3D11Device(IDirect3DDevice device)
        {
            return GetDXGIInterfaceFromObject<ID3D11Device>(device);
        }

        public static ID3D11Texture2D GetD3D11Texture2D(IDirect3DSurface surface)
        {
            return GetDXGIInterfaceFromObject<ID3D11Texture2D>(surface);
        }
    }
}
