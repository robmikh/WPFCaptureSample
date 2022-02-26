using Windows.Win32.Graphics.Direct3D11;
using Windows.Win32.Graphics.Dxgi;
using Windows.Win32.Graphics.Dxgi.Common;
using Robmikh.WindowsRuntimeHelpers;
using System;
using Windows.Graphics;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.UI.Composition;
using WinRT;
using System.Runtime.CompilerServices;
using static Windows.Win32.PInvoke;

namespace CaptureSampleCore
{
    public class BasicCapture : IDisposable
    {
        public BasicCapture(IDirect3DDevice device, GraphicsCaptureItem item)
        {
            _item = item;
            var itemSize = item.Size;
            _device = device;
            _d3dDevice = Direct3D11Helper.GetD3D11Device(_device);
            _d3dDevice.GetImmediateContext(out _d3dContext);

            var dxgiDevice = _d3dDevice.As<IDXGIDevice>();
            IDXGIFactory2 factory = null;
            unsafe
            {
                var adapterGuid = typeof(IDXGIAdapter).GUID;
                dxgiDevice.GetParent((Guid*)Unsafe.AsPointer(ref adapterGuid), out var rawAdapter);
                var adapter = rawAdapter.As<IDXGIAdapter>();
                var factoryGuid = typeof(IDXGIFactory2).GUID;
                adapter.GetParent((Guid*)Unsafe.AsPointer(ref factoryGuid), out var rawFactory);
                factory = rawFactory.As<IDXGIFactory2>();
            }

            var description = new DXGI_SWAP_CHAIN_DESC1()
            { 
                Width = (uint)itemSize.Width,
                Height = (uint)itemSize.Height,
                Format = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM,
                Stereo = false,
                SampleDesc = new DXGI_SAMPLE_DESC()
                {
                    Count = 1,
                    Quality = 0,
                },
                BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT,
                BufferCount = 2,
                Scaling = DXGI_SCALING.DXGI_SCALING_STRETCH,
                SwapEffect = DXGI_SWAP_EFFECT.DXGI_SWAP_EFFECT_FLIP_SEQUENTIAL,
                AlphaMode = DXGI_ALPHA_MODE.DXGI_ALPHA_MODE_PREMULTIPLIED,
                Flags = 0
            };
            unsafe
            {
                factory.CreateSwapChainForComposition(_d3dDevice, (DXGI_SWAP_CHAIN_DESC1*)Unsafe.AsPointer(ref description), null, out var swapChain);
                _swapChain = swapChain;
            }

            _framePool = Direct3D11CaptureFramePool.Create(
                _device,
                DirectXPixelFormat.B8G8R8A8UIntNormalized,
                2,
                itemSize);
            _session = _framePool.CreateCaptureSession(item);
            _lastSize = itemSize;

            _framePool.FrameArrived += OnFrameArrived;
        }

        public void Dispose()
        {
            _session?.Dispose();
            _framePool?.Dispose();
            _swapChain = null;
            _d3dDevice = null;
        }

        public void StartCapture()
        {
            _session.StartCapture();
        }

        public ICompositionSurface CreateSurface(Compositor compositor)
        {
            return compositor.CreateCompositionSurfaceForSwapChain(_swapChain);
        }

        private void OnFrameArrived(Direct3D11CaptureFramePool sender, object args)
        {
            var newSize = false;

            using (var frame = sender.TryGetNextFrame())
            {
                if (frame.ContentSize.Width != _lastSize.Width ||
                    frame.ContentSize.Height != _lastSize.Height)
                {
                    // The thing we have been capturing has changed size.
                    // We need to resize our swap chain first, then blit the pixels.
                    // After we do that, retire the frame and then recreate our frame pool.
                    newSize = true;
                    _lastSize = frame.ContentSize;
                    _swapChain.ResizeBuffers(
                        2,
                        (uint)_lastSize.Width,
                        (uint)_lastSize.Height,
                        DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM,
                        0);
                }

                ID3D11Texture2D backBuffer = null;
                unsafe
                {
                    var guid = typeof(ID3D11Texture2D).GUID;
                    _swapChain.GetBuffer(0, (Guid*)Unsafe.AsPointer(ref guid), out var rawBuffer);
                    backBuffer = rawBuffer.As<ID3D11Texture2D>();
                }
                var bitmap = Direct3D11Helper.GetD3D11Texture2D(frame.Surface);
                _d3dContext.CopyResource(backBuffer, bitmap);

            } // retire the frame

            _swapChain.Present(0, 0);

            if (newSize)
            {
                _framePool.Recreate(
                    _device,
                    DirectXPixelFormat.B8G8R8A8UIntNormalized,
                    2,
                    _lastSize);
            }
        }

        private GraphicsCaptureItem _item;
        private Direct3D11CaptureFramePool _framePool;
        private GraphicsCaptureSession _session;
        private SizeInt32 _lastSize;

        private IDirect3DDevice _device;
        private ID3D11Device _d3dDevice;
        private ID3D11DeviceContext _d3dContext;
        private IDXGISwapChain1 _swapChain;
    }
}
