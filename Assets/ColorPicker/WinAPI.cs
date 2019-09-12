using System;
using System.Runtime.InteropServices;

namespace Hank.ColorPicker
{
    internal delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    #region Structs
    [StructLayout(LayoutKind.Sequential)]
    internal struct BitmapInfoHeader
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;

        public void Init()
        {
            biSize = (uint)Marshal.SizeOf(this);
            //BitmapCompressionMode.BI_RGB
            biCompression = 0u;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct Message
    {
        public IntPtr hwnd;
        public uint message;
        public UIntPtr wParam;
        public UIntPtr lParam;
        public uint time;
        public Point pt;
    }

    internal struct Point
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct WNDCLASSEX
    {
        [MarshalAs(UnmanagedType.U4)]
        public int cbSize;
        [MarshalAs(UnmanagedType.U4)]
        public int style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string lpszMenuName;
        public string lpszClassName;
        public IntPtr hIconSm;
    }
    #endregion

    internal enum WindowMessage : uint
    {
        KILLFOCUS = 0x0008,
        KEYDOWN = 0x0100,
        MOUSEMOVE = 0x0200,
        LBUTTONDOWN = 0x0201,
        DESTROY = 0x0002,
        SYSKEYDOWN = 0x0104,
    }

    internal static class WinAPI
    {
        private const string USER_32_DLL = "user32.dll";
        private const string GDI_32_DLL = "gdi32.dll";

        /// <summary>
        /// 鼠标指针箭头
        /// </summary>
        public const int IDC_ARROW = 32512;
        /// <summary>
        /// 鼠标指针十字
        /// </summary>
        public const int IDC_CROSS = 32515;
        /// <summary>
        /// Default application icon
        /// </summary>
        public const int IDI_APPLICATION = 32512;
        public const uint WS_POPUP = 0x80000000u;
        public const int KEYCODE_ESCAPE = 0x1b;
        /// <summary>
        /// WS_EX_LAYERED | WS_EX_TOPMOST | WS_EX_TOOLWINDOW
        /// </summary>
        public const uint CUSTOM_WINDOW_STYLE_EX = 0x00080000u | 0x00000008u | 0x00000080u;

        [DllImport(USER_32_DLL)]
        public static extern IntPtr DispatchMessage([In] ref Message lpmsg);
        [DllImport(USER_32_DLL)]
        public static extern bool TranslateMessage([In] ref Message lpMsg);
        [DllImport(USER_32_DLL)]
        public static extern sbyte GetMessage(out Message lpMsg, IntPtr hWnd, uint wMsgFilterMin = 0, uint wMsgFilterMax = 0);

        [DllImport(USER_32_DLL, SetLastError = true)]
        public static extern IntPtr CreateWindowEx(
           uint dwExStyle,
           ushort lpClassName,
           string lpWindowName,
           uint dwStyle,
           int x,
           int y,
           int nWidth,
           int nHeight,
           IntPtr hWndParent,
           IntPtr hMenu,
           IntPtr hInstance,
           IntPtr lpParam);

        [DllImport(USER_32_DLL, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport(USER_32_DLL)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow = 1);

        [DllImport(USER_32_DLL)]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, WindowMessage uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport(USER_32_DLL)]
        public static extern void PostQuitMessage(int nExitCode);

        [DllImport(USER_32_DLL)]
        public static extern bool UpdateWindow(IntPtr hWnd);

        [DllImport(USER_32_DLL)]
        public static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport(USER_32_DLL, SetLastError = true)]
        public static extern UInt16 RegisterClassEx([In] ref WNDCLASSEX lpwcx);

        [DllImport(USER_32_DLL, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnregisterClassA(UInt16 lpClassName, IntPtr hInstance);

        [DllImport(USER_32_DLL, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out Point lpPoint);

        public static Point GetCursorPos()
        {
            Point p;
            GetCursorPos(out p);
            return p;
        }

        [DllImport(USER_32_DLL, SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public static void SetWindowPos(IntPtr hWnd, int x, int y)
        {
            SetWindowPos(hWnd, IntPtr.Zero, x, y, 0, 0, 0x0001/*this flag indicates the window size will not change.*/);
        }

        [DllImport(USER_32_DLL)]
        public static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIConName);

        [DllImport(GDI_32_DLL)]
        public static extern IntPtr GetStockObject(int fnObject = 0/** white brush */);

        [DllImport(GDI_32_DLL, CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int BitBlt(
            IntPtr hDC,
            int x,
            int y, int
            nWidth, int
            nHeight,
            IntPtr hSrcDC,
            int xSrc = 0,
            int ySrc = 0,
            int dwRop = 0x00CC0020 /** SRCCOPY */
        );

        [DllImport(USER_32_DLL, SetLastError = true)]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport(USER_32_DLL, SetLastError = true)]
        public static extern IntPtr GetDesktopWindow();

        [DllImport(USER_32_DLL, SetLastError = true)]
        public static extern IntPtr GetWindowDC(IntPtr window);

        [DllImport(GDI_32_DLL, SetLastError = true)]
        public static extern uint GetPixel(IntPtr dc, int x, int y);

        [DllImport(USER_32_DLL)]
        public static extern int ReleaseDC(IntPtr window, IntPtr dc);

        [DllImport(USER_32_DLL)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteDC(IntPtr dc);

        [DllImport(GDI_32_DLL)]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int cx, int cy);

        [DllImport(GDI_32_DLL)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport(GDI_32_DLL)]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr h);

        [DllImport(GDI_32_DLL)]
        public static extern int GetObject(IntPtr h, int c, IntPtr pv);

        [DllImport(GDI_32_DLL)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject(IntPtr h);

        [DllImport(GDI_32_DLL)]
        public static extern int GetDIBits(
            [In] IntPtr hdc,
            [In] IntPtr hbmp,
            uint uStartScan,
            uint cScanLines,
            [Out] byte[] lpvBits,
            ref BitmapInfoHeader lpbi,
            uint uUsage = 0u /** DIB_RGB_COLORS */
        );

    }

}