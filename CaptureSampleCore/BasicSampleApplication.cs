using Robmikh.WindowsRuntimeHelpers;
using System;
using System.Numerics;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.UI.Composition;

namespace CaptureSampleCore
{
    public class BasicSampleApplication : IDisposable
    {
        public BasicSampleApplication(Compositor compositor)
        {
            _compositor = compositor;
            _device = Direct3D11Helper.CreateDevice();

            // Setup our root
            _root = _compositor.CreateContainerVisual();
            _root.RelativeSizeAdjustment = Vector2.One;

            // Setup our content
            _brush = _compositor.CreateSurfaceBrush();
            _brush.HorizontalAlignmentRatio = 0.5f;
            _brush.VerticalAlignmentRatio = 0.5f;
            _brush.Stretch = CompositionStretch.Uniform;

            var shadow = _compositor.CreateDropShadow();
            shadow.Mask = _brush;

            _content = _compositor.CreateSpriteVisual();
            _content.AnchorPoint = new Vector2(0.5f);
            _content.RelativeOffsetAdjustment = new Vector3(0.5f, 0.5f, 0);
            _content.RelativeSizeAdjustment = Vector2.One;
            _content.Size = new Vector2(-80, -80);
            _content.Brush = _brush;
            _content.Shadow = shadow;
            _root.Children.InsertAtTop(_content);
        }

        public Visual Visual => _root;

        public void Dispose()
        {
            StopCapture();
            _compositor = null;
            _root.Dispose();
            _content.Dispose();
            _brush.Dispose();
            _device.Dispose();
        }

        public void StartCaptureFromItem(GraphicsCaptureItem item)
        {
            StopCapture();
            _capture = new BasicCapture(_device, item);

            var surface = _capture.CreateSurface(_compositor);
            _brush.Surface = surface;

            _capture.StartCapture();
        }

        public void StopCapture()
        {
            _capture?.Dispose();
            _brush.Surface = null;
        }

        private Compositor _compositor;
        private ContainerVisual _root;

        private SpriteVisual _content;
        private CompositionSurfaceBrush _brush;

        private IDirect3DDevice _device;
        private BasicCapture _capture;
    }
}
