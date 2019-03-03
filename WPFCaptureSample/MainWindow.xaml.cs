using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.System;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Composition.Core;

namespace WPFCaptureSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            _device = Direct3D11Helper.CreateDevice();
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            StopCapture();
            var ignored = StartCaptureAsync();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var interopWindow = new WindowInteropHelper(this);
            _hwnd = interopWindow.Handle;

            InitComposition();
            await StartCaptureAsync();
        }

        private void InitComposition()
        {
            // Create our compositor
            _compositor = new Compositor();

            // Create a target for our window
            _target = _compositor.CreateDesktopWindowTarget(_hwnd, true);

            // Attach our root visual
            _root = _compositor.CreateSpriteVisual();
            _root.RelativeSizeAdjustment = Vector2.One;
            _root.Brush = _compositor.CreateColorBrush(Colors.White);
            _target.Root = _root;

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

        private async Task StartCaptureAsync()
        {
            var picker = new GraphicsCapturePicker();
            picker.SetWindow(_hwnd);

            var item = await picker.PickSingleItemAsync();
            if (item != null)
            {
                _capture = new BasicCapture(_device, item);

                var surface = _capture.CreateSurface(_compositor);
                _brush.Surface = surface;

                _capture.StartCapture();
            }
        }

        private void StopCapture()
        {
            _capture?.Dispose();
            _brush.Surface = null;
        }

        private IntPtr _hwnd;
        private Compositor _compositor;
        private CompositionTarget _target;
        private SpriteVisual _root;

        private SpriteVisual _content;
        private CompositionSurfaceBrush _brush;

        private IDirect3DDevice _device;
        private BasicCapture _capture;
    }
}
