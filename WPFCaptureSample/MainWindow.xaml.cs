using CaptureSampleCore;
using Robmikh.WindowsRuntimeHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            // force grpahicscapture.dll to load
            var picker = new GraphicsCapturePicker();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StopCapture();
            StartCapture();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var interopWindow = new WindowInteropHelper(this);
            _hwnd = interopWindow.Handle;

            InitComposition();
        }

        private void InitComposition()
        {
            // Create our compositor
            _compositor = new Compositor();

            // Create a target for our window
            _target = _compositor.CreateDesktopWindowTarget(_hwnd, true);

            // Attach our root visual
            _root = _compositor.CreateContainerVisual();
            _root.RelativeSizeAdjustment = Vector2.One;
            _target.Root = _root;

            // Setup the rest of our sample application
            _sample = new BasicSampleApplication(_compositor);
            _root.Children.InsertAtTop(_sample.Visual);
        }

        private void StartCapture()
        {
            var processes = Process.GetProcesses();
            var matchingProcesses = from p in processes
                                    where p.MainWindowTitle.Contains("Visual Studio")
                                    select p;
            var visualStudioHwnd = matchingProcesses.First().MainWindowHandle;

            var item = CaptureHelper.CreateItemForWindow(visualStudioHwnd);
            if (item != null)
            {
                _sample.StartCaptureFromItem(item);
            }
        }

        private void StopCapture()
        {
            _sample.StopCapture();
        }

        private IntPtr _hwnd;
        private Compositor _compositor;
        private CompositionTarget _target;
        private ContainerVisual _root;

        private BasicSampleApplication _sample;
    }
}
