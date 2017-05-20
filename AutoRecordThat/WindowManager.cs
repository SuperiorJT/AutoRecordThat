using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace AutoRecordThat
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left;

        public int Top;

        public int Right;

        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct MonitorInfoEx
    {
        public int cbSize;
        public Rect rcMonitor;
        public Rect rcWork;
        public UInt32 dwFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string szDeviceName;
    }

    class WindowManager
    {
        [DllImport("user32.dll")]
        protected static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        protected static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        protected static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("User32")]
        protected static extern IntPtr MonitorFromWindow(IntPtr hWnd, int dwFlags);

        [DllImport("user32", EntryPoint = "GetMonitorInfo", CharSet = CharSet.Auto,
            SetLastError = true)]
        internal static extern bool GetMonitorInfoEx(IntPtr hMonitor, ref MonitorInfoEx lpmi);

        protected static bool EnumTheWindows(IntPtr hWnd, IntPtr lParam)
        {
            const int MONITOR_DEFAULTTOPRIMARY = 1;
            var mi = new MonitorInfoEx();
            mi.cbSize = Marshal.SizeOf(mi);
            GetMonitorInfoEx(MonitorFromWindow(hWnd, MONITOR_DEFAULTTOPRIMARY), ref mi);

            int size = GetWindowTextLength(hWnd);
            if (size++ > 0 && IsWindowVisible(hWnd))
            {
                GCHandle gcHandle = (GCHandle) lParam;
                List<IntPtr> windows = (gcHandle.Target as List<IntPtr>);

                windows.Add(hWnd);
            }
            
            return true;
        }

        public static bool FilterFullScreenWindows(IntPtr hWnd, MonitorInfoEx mi)
        {
            bool isFullscreen = false;
            Rect appBounds;
            GetWindowRect(hWnd, out appBounds);

            int windowHeight = appBounds.Right - appBounds.Left;
            int windowWidth = appBounds.Bottom - appBounds.Top;

            int monitorHeight = mi.rcMonitor.Right - mi.rcMonitor.Left;
            int monitorWidth = mi.rcMonitor.Bottom - mi.rcMonitor.Top;

            isFullscreen = (windowHeight == monitorHeight) && (windowWidth == monitorWidth);

            return isFullscreen;
        }

        public static List<IntPtr> FilterWindows()
        {
            List<IntPtr> windows = new List<IntPtr>();
            GCHandle gcHandle = GCHandle.Alloc(windows);
            EnumWindows(EnumTheWindows, (IntPtr) gcHandle);
            return windows;
        }

        public delegate bool WindowFilter(IntPtr hWnd, MonitorInfoEx mi);

        protected delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    }
}
