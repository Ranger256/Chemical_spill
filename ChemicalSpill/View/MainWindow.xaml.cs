using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ChemicalSpill.View
{
    /// <summary>
    /// Оболочка приложения. Кроме управления окном логики не содержит:
    /// всё поведение задаётся моделями представления и сервисами.
    /// </summary>
    public partial class MainWindow : Window
    {
        private WindowState _stateBeforeFullScreen = WindowState.Normal;
        private bool _isFullScreen;

        public MainWindow()
        {
            InitializeComponent();

            StateChanged += OnWindowStateChanged;
            SourceInitialized += OnSourceInitialized;
        }

        // ------------------------------------------------------------------
        // Корректное разворачивание окна без системной рамки: без перехвата
        // WM_GETMINMAXINFO развёрнутое окно закрывает панель задач и выходит
        // за границы экрана, из-за чего строка состояния оказывается не видна.
        // ------------------------------------------------------------------

        private const int WmGetMinMaxInfo = 0x0024;
        private const int MonitorDefaultToNearest = 0x00000002;

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            var source = PresentationSource.FromVisual(this) as HwndSource;
            if (source != null) source.AddHook(WindowProc);
        }

        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != WmGetMinMaxInfo) return IntPtr.Zero;

            LimitMaximizedSize(hwnd, lParam);
            handled = true;
            return IntPtr.Zero;
        }

        private static void LimitMaximizedSize(IntPtr hwnd, IntPtr lParam)
        {
            var info = (MinMaxInfo)Marshal.PtrToStructure(lParam, typeof(MinMaxInfo));
            var monitor = MonitorFromWindow(hwnd, MonitorDefaultToNearest);
            if (monitor == IntPtr.Zero) return;

            var monitorInfo = new MonitorInfo();
            monitorInfo.Size = Marshal.SizeOf(typeof(MonitorInfo));
            if (!GetMonitorInfo(monitor, ref monitorInfo)) return;

            var work = monitorInfo.WorkArea;
            var screen = monitorInfo.Monitor;

            info.MaxPosition.X = work.Left - screen.Left;
            info.MaxPosition.Y = work.Top - screen.Top;
            info.MaxSize.X = work.Right - work.Left;
            info.MaxSize.Y = work.Bottom - work.Top;

            Marshal.StructureToPtr(info, lParam, true);
        }

        private void OnWindowStateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                MaximizeButton.Content = "❐";
                MaximizeButton.ToolTip = "Восстановить";
            }
            else
            {
                MaximizeButton.Content = "□";
                MaximizeButton.ToolTip = "Развернуть";
            }
        }

        private void OnMinimizeClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void OnMaximizeClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnFullScreenClick(object sender, RoutedEventArgs e)
        {
            if (_isFullScreen)
            {
                WindowState = _stateBeforeFullScreen;
                ResizeMode = ResizeMode.CanResize;
                _isFullScreen = false;
                return;
            }

            _stateBeforeFullScreen = WindowState;
            WindowState = WindowState.Normal;
            ResizeMode = ResizeMode.NoResize;
            WindowState = WindowState.Maximized;
            _isFullScreen = true;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, int flags);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr monitor, ref MonitorInfo info);

        [StructLayout(LayoutKind.Sequential)]
        private struct NativePoint
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MinMaxInfo
        {
            public NativePoint Reserved;
            public NativePoint MaxSize;
            public NativePoint MaxPosition;
            public NativePoint MinTrackSize;
            public NativePoint MaxTrackSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MonitorInfo
        {
            public int Size;
            public NativeRect Monitor;
            public NativeRect WorkArea;
            public int Flags;
        }
    }
}
