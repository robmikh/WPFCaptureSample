using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Capture;

namespace WPFCompTest
{
    static class CaptureHelper
    {
        [ComImport]
        [Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComVisible(true)]
        interface IInitializeWithWindow
        {
            void Initialize(
                IntPtr hwnd);
        }

        public static void SetWindow(this GraphicsCapturePicker picker, IntPtr hwnd)
        {
            var interop = (IInitializeWithWindow)(object)picker;
            interop.Initialize(hwnd);
        }
    }
}
