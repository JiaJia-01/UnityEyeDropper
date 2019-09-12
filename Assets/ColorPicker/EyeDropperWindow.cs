using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace Hank.ColorPicker
{
    internal class EyeDropperWindow
    {
        public enum EPickStatus
        {
            None,
            Picking,
            Picked,
            Canceled
        }

        private const int WINDOW_WIDTH = 2000;
        private const int WINDOW_HEIGHT = 2000;

        private const string WINDOW_NAME = "HankEyeDropperTransparentWindow";
        private const string WINDOW_CLASS = "HankEyeDropperWindowClass";

        private const int READ_SCREEN_WIDTH = 21;
        private const int READ_SCREEN_HEIGHT = 21;

        private static bool _registered = false;
        private static ushort _atom;

        private static byte[] _pixels = new byte[READ_SCREEN_WIDTH * READ_SCREEN_HEIGHT * 4];

        public static EPickStatus Status = EPickStatus.None;

        private static Sprite _currentSprite;
        private static bool _dirty = false;
        private static object _dirtyLock = new object();

        public static bool Dirty
        {
            get { return _dirty; }
            set
            {
                lock (_dirtyLock)
                {
                    _dirty = value;
                }
            }
        }

        public static void Open()
        {
            Status = EPickStatus.Picking;
            var thread = new Thread(() =>
            {
                RegisterWndClassEx();
                bool success = CreateWindow();
                if (success)
                {
                    DealMessage();
                }
            });
            thread.Start();
        }

        public static void OnExitingPlayMode()
        {
            if (_registered)
            {
                WinAPI.UnregisterClassA(_atom, Process.GetCurrentProcess().Handle);
                _registered = false;
            }
        }


        public static Sprite CurrentSprite
        {
            get
            {
                if (_dirty)
                {
                    if (_currentSprite != null)
                    {
                        UnityEngine.Object.Destroy(_currentSprite.texture);
                        UnityEngine.Object.Destroy(_currentSprite);
                    }

                    Texture2D t2d = new Texture2D(READ_SCREEN_WIDTH, READ_SCREEN_HEIGHT, TextureFormat.RGBA32, false);
                    t2d.filterMode = FilterMode.Point;

                    lock (_pixels)
                    {
                        for (int i = 0; i < READ_SCREEN_HEIGHT; i++)
                        {
                            for (int j = 0; j < READ_SCREEN_WIDTH; j++)
                            {
                                int index = (i * READ_SCREEN_WIDTH + j) * 4;
                                Color color = new Color32(_pixels[index + 2], _pixels[index + 1], _pixels[index], 255);
                                t2d.SetPixel(j, i, color);
                            }
                        }
                    }

                    t2d.Apply();

                    _currentSprite = Sprite.Create(t2d, new Rect(0, 0, READ_SCREEN_WIDTH, READ_SCREEN_HEIGHT), Vector2.one / 2f);

                    Dirty = false;
                }
                return _currentSprite;
            }
        }

        public static Color CurrentColor
        {
            get
            {
                var t2d = CurrentSprite.texture;
                return t2d.GetPixel(READ_SCREEN_WIDTH / 2, READ_SCREEN_HEIGHT / 2);
            }
        }

        private static void ReadPixelsAroundPoint(Point p)
        {
            IntPtr hScreenDC = WinAPI.GetDC(IntPtr.Zero);

            IntPtr hMyDC = WinAPI.CreateCompatibleDC(hScreenDC);

            IntPtr hBitmap = WinAPI.CreateCompatibleBitmap(hScreenDC, READ_SCREEN_WIDTH, READ_SCREEN_HEIGHT);
            IntPtr hOldBitmap = WinAPI.SelectObject(hMyDC, hBitmap);

            WinAPI.BitBlt(
                hMyDC,
                0,
                0,
                READ_SCREEN_WIDTH,
                READ_SCREEN_HEIGHT,
                hScreenDC,
                p.x - READ_SCREEN_WIDTH / 2,
                p.y - READ_SCREEN_HEIGHT / 2
            );

            BitmapInfoHeader bitmapInfoHeader = new BitmapInfoHeader()
            {
                biWidth = READ_SCREEN_WIDTH,
                biHeight = READ_SCREEN_HEIGHT,
                biPlanes = 1,
                biBitCount = 32,
                biSizeImage = 0,
                biXPelsPerMeter = 0,
                biYPelsPerMeter = 0,
                biClrUsed = 0,
                biClrImportant = 0
            };
            bitmapInfoHeader.Init();

            lock (_pixels)
            {
                WinAPI.GetDIBits(hMyDC, hBitmap, 0, READ_SCREEN_HEIGHT, _pixels, ref bitmapInfoHeader);
            }

            WinAPI.SelectObject(hMyDC, hOldBitmap);

            WinAPI.DeleteObject(hBitmap);
            WinAPI.DeleteObject(hMyDC);
            WinAPI.ReleaseDC(IntPtr.Zero, hScreenDC);

            Dirty = true;
        }

        #region Window
        private static IntPtr WindowProc(IntPtr hWnd, uint message, IntPtr wParam, IntPtr lParam)
        {
            WindowMessage wm = (WindowMessage)message;

            Point cursorPos;
            int keyCode;

            switch (wm)
            {
                case WindowMessage.DESTROY:
                    WinAPI.PostQuitMessage(0);
                    return IntPtr.Zero;

                case WindowMessage.KILLFOCUS://失去键盘焦点
                    //左键点击后关闭窗口也会触发失去焦点事件
                    if (Status != EPickStatus.Picked)
                        Status = EPickStatus.Canceled;
                    WinAPI.DestroyWindow(hWnd);
                    return IntPtr.Zero;

                case WindowMessage.KEYDOWN://不包含ALt + ？ 和 F10
                    keyCode = wParam.ToInt32();
                    if (keyCode == WinAPI.KEYCODE_ESCAPE)
                    {
                        Status = EPickStatus.Canceled;
                        WinAPI.DestroyWindow(hWnd);
                    }
                    return IntPtr.Zero;

                case WindowMessage.SYSKEYDOWN://Alt + ？ 或者 F10
                    return IntPtr.Zero;

                case WindowMessage.LBUTTONDOWN://鼠标左键按下
                    Status = EPickStatus.Picked;
                    WinAPI.DestroyWindow(hWnd);
                    return IntPtr.Zero;

                case WindowMessage.MOUSEMOVE://鼠标移动事件
                    cursorPos = WinAPI.GetCursorPos();
                    WinAPI.SetWindowPos(hWnd, cursorPos.x - WINDOW_WIDTH / 2, cursorPos.y - WINDOW_HEIGHT / 2);
                    ReadPixelsAroundPoint(cursorPos);
                    return IntPtr.Zero;
            }
            return WinAPI.DefWindowProc(hWnd, wm, wParam, lParam);
        }

        private static void RegisterWndClassEx()
        {
            if (_registered == true) return;

            WNDCLASSEX wndClass = new WNDCLASSEX
            {
                cbSize = Marshal.SizeOf(typeof(WNDCLASSEX)),
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate((WndProc)WindowProc),
                cbClsExtra = 0,
                cbWndExtra = 0,
                hInstance = Process.GetCurrentProcess().Handle,
                hIcon = WinAPI.LoadIcon(IntPtr.Zero, new IntPtr(WinAPI.IDI_APPLICATION)),
                hCursor = WinAPI.LoadCursor(IntPtr.Zero, WinAPI.IDC_CROSS),
                hbrBackground = WinAPI.GetStockObject(0),
                lpszMenuName = null,
                lpszClassName = WINDOW_CLASS
            };

            _atom = WinAPI.RegisterClassEx(ref wndClass);

            _registered = true;
        }

        private static bool CreateWindow()
        {
            var cursorPos = WinAPI.GetCursorPos();
            ReadPixelsAroundPoint(cursorPos);

            var hWnd = WinAPI.CreateWindowEx(
                WinAPI.CUSTOM_WINDOW_STYLE_EX,
                _atom,
                WINDOW_NAME,
                WinAPI.WS_POPUP,
                cursorPos.x - WINDOW_WIDTH / 2,
                cursorPos.y - WINDOW_HEIGHT / 2,
                WINDOW_WIDTH,
                WINDOW_HEIGHT,
                IntPtr.Zero,
                IntPtr.Zero,
                Process.GetCurrentProcess().Handle,
                IntPtr.Zero
            );

            if (hWnd == IntPtr.Zero)
            {
                int lastError = Marshal.GetLastWin32Error();
                string errorMessage = new Win32Exception(lastError).Message;
#if UNITY_EDITOR
                UnityEngine.Debug.LogError(lastError + " " + errorMessage);
#endif
                return false;
            }

            WinAPI.ShowWindow(hWnd);
            WinAPI.UpdateWindow(hWnd);
            return true;
        }

        private static void DealMessage()
        {
            Message msg;
            while (WinAPI.GetMessage(out msg, IntPtr.Zero) != 0)
            {
                WinAPI.TranslateMessage(ref msg);
                WinAPI.DispatchMessage(ref msg);
            }
        }

        #endregion //Window

    }
}
