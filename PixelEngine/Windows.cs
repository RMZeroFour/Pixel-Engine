using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PixelEngine
{
	internal static unsafe class Windows
	{
		public static readonly string TempPath;

		static Windows()
		{
			TempPath = Path.Combine(Path.GetTempPath(), $"{nameof(PixelEngine)}.{Assembly.GetExecutingAssembly().GetName().Version}");

			if (!Directory.Exists(TempPath))
				Directory.CreateDirectory(TempPath);

			DllHelper.LoadDll();
		}

		public static void DestroyTempPath()
		{
			DllHelper.DestroyDll();

			if (Directory.Exists(TempPath))
			{
				foreach (string file in Directory.EnumerateFiles(TempPath))
				{
					if(!file.EndsWith(".dll"))
						File.Delete(file);
				}
			}
		}

		#region Constants
		public const int DoubleClicks = 0x8;
		public const int VRedraw = 0x1;
		public const int HRedraw = 0x2;
		public const int OwnDC = 0x20;

		public const int WheelDelta = 120;

		public const int WomDone = 0x3BD;

		public const int MonitorDefaultNearest = 2;

		public static readonly IntPtr WindowTop = new IntPtr(0);

		public const int ApplicationIcon = 32512;
		public const int ArrowCursor = 32512;
		public const int ColorWindow = 5;

		public const int WaveFormatPcm = 1;
		public const int WaveMapper = -1;
		public const int WHdrPrepared = 2;

		public const int CallbackFunction = 0x30000;

		private const string User = "user32.dll";
		private const string Kernel = "kernel32.dll";
		private const string OpenGl = "opengl32.dll";
		private const string Gdi = "gdi32.dll";
		private const string Winmm = "winmm.dll";
		private const string PixGl = "PixGL.dll";
		#endregion

		#region Enums
		[Flags]
		public enum WindowStyles : uint
		{
			Border = 0x800000,
			Caption = 0xC00000,
			Child = 0x40000000,
			ClipChildren = 0x2000000,
			ClipSiblings = 0x4000000,
			Disabled = 0x8000000,
			DlgFrame = 0x400000,
			Group = 0x20000,
			HScroll = 0x100000,
			Maximize = 0x1000000,
			MaximizeBox = 0x10000,
			Minimize = 0x20000000,
			MinimizeBox = 0x20000,
			Overlapped = 0x0,
			OverlappedWindow = Overlapped | Caption | SysMenu | ThickFrame | MinimizeBox | MaximizeBox,
			Popup = 0x80000000,
			PopupWindow = Popup | Border | SysMenu,
			SysMenu = 0x80000,
			TabStop = 0x10000,
			ThickFrame = 0x40000,
			Visible = 0x10000000,
			VScroll = 0x200000
		}

		public enum WindowLongs
		{
			EXSTYLE = -20,
			HINSTANCE = -6,
			HWNDPARENT = -8,
			ID = -12,
			STYLE = -16,
			USERDATA = -21,
			WNDPROC = -4,
			USER = 0x8,
			MSGRESULT = 0x0,
			DLGPROC = 0x4
		}

		public enum PFD
		{
			TypeRGBA = 0,
			MainPlane = 0,
			DoubleBuffer = 1,
			DrawToWindow = 4,
			SupportOpenGL = 32
		}

		[Flags]
		public enum WindowStylesEx : uint
		{
			AppWindow = 0x40000,
			WindowEdge = 0x00100
		}

		[Flags]
		public enum SWP : uint
		{
			FrameChanged = 0x0020,
			HideWindow = 0x0080,
			NoActivate = 0x0010,
			NoCopyBits = 0x0100,
			NoMove = 0x0002,
			NoOwnerZOrder = 0x0200,
			NoRedraw = 0x0008,
			NoReposition = 0x200,
			NoSendChanging = 0x0400,
			NoSize = 0x0001,
			NoZOrder = 0x0004,
			ShowWindow = 0x0040,
		}

		public enum GL
		{
			Texture2D = 0x0DE1,
			TextureMagFilter = 0x2800,
			TextureMinFilter = 0x2801,
			Nearest = 0x2600,
			TextureEnv = 0x2300,
			TextureEnvMode = 0x2200,
			Decal = 0x2101,
			RGBA = 0x1908,
			UnsignedByte = 0x1401,
			Quads = 0x0007,
			Points = 0x0000,
			ModelView = 0x1700,
			Projection = 0x1701,
			ColorBufferBit = 0x4000,
			DepthBufferBit = 0x00000100
		}

		public enum WM
		{
			NULL = 0x0000,
			CREATE = 0x0001,
			DESTROY = 0x0002,
			MOVE = 0x0003,
			SIZE = 0x0005,
			ACTIVATE = 0x0006,
			SETFOCUS = 0x0007,
			KILLFOCUS = 0x0008,
			ENABLE = 0x000A,
			SETREDRAW = 0x000B,
			SETTEXT = 0x000C,
			GETTEXT = 0x000D,
			GETTEXTLENGTH = 0x000E,
			PAINT = 0x000F,
			CLOSE = 0x0010,
			QUERYENDSESSION = 0x0011,
			QUERYOPEN = 0x0013,
			ENDSESSION = 0x0016,
			QUIT = 0x0012,
			ERASEBKGND = 0x0014,
			SYSCOLORCHANGE = 0x0015,
			SHOWWINDOW = 0x0018,
			WININICHANGE = 0x001A,
			SETTINGCHANGE = WININICHANGE,
			DEVMODECHANGE = 0x001B,
			ACTIVATEAPP = 0x001C,
			FONTCHANGE = 0x001D,
			TIMECHANGE = 0x001E,
			CANCELMODE = 0x001F,
			SETCURSOR = 0x0020,
			MOUSEACTIVATE = 0x0021,
			CHILDACTIVATE = 0x0022,
			QUEUESYNC = 0x0023,
			GETMINMAXINFO = 0x0024,
			PAINTICON = 0x0026,
			ICONERASEBKGND = 0x0027,
			NEXTDLGCTL = 0x0028,
			SPOOLERSTATUS = 0x002A,
			DRAWITEM = 0x002B,
			MEASUREITEM = 0x002C,
			DELETEITEM = 0x002D,
			VKEYTOITEM = 0x002E,
			CHARTOITEM = 0x002F,
			SETFONT = 0x0030,
			GETFONT = 0x0031,
			SETHOTKEY = 0x0032,
			GETHOTKEY = 0x0033,
			QUERYDRAGICON = 0x0037,
			COMPAREITEM = 0x0039,
			GETOBJECT = 0x003D,
			COMPACTING = 0x0041,
			COMMNOTIFY = 0x0044,
			WINDOWPOSCHANGING = 0x0046,
			WINDOWPOSCHANGED = 0x0047,
			POWER = 0x0048,
			COPYDATA = 0x004A,
			CANCELJOURNAL = 0x004B,
			NOTIFY = 0x004E,
			INPUTLANGCHANGEREQUEST = 0x0050,
			INPUTLANGCHANGE = 0x0051,
			TCARD = 0x0052,
			HELP = 0x0053,
			USERCHANGED = 0x0054,
			NOTIFYFORMAT = 0x0055,
			CONTEXTMENU = 0x007B,
			STYLECHANGING = 0x007C,
			STYLECHANGED = 0x007D,
			DISPLAYCHANGE = 0x007E,
			GETICON = 0x007F,
			SETICON = 0x0080,
			NCCREATE = 0x0081,
			NCDESTROY = 0x0082,
			NCCALCSIZE = 0x0083,
			NCHITTEST = 0x0084,
			NCPAINT = 0x0085,
			NCACTIVATE = 0x0086,
			GETDLGCODE = 0x0087,
			SYNCPAINT = 0x0088,
			NCMOUSEMOVE = 0x00A0,
			NCLBUTTONDOWN = 0x00A1,
			NCLBUTTONUP = 0x00A2,
			NCLBUTTONDBLCLK = 0x00A3,
			NCRBUTTONDOWN = 0x00A4,
			NCRBUTTONUP = 0x00A5,
			NCRBUTTONDBLCLK = 0x00A6,
			NCMBUTTONDOWN = 0x00A7,
			NCMBUTTONUP = 0x00A8,
			NCMBUTTONDBLCLK = 0x00A9,
			NCXBUTTONDOWN = 0x00AB,
			NCXBUTTONUP = 0x00AC,
			NCXBUTTONDBLCLK = 0x00AD,
			INPUT_DEVICE_CHANGE = 0x00FE,
			INPUT = 0x00FF,
			KEYFIRST = 0x0100,
			KEYDOWN = 0x0100,
			KEYUP = 0x0101,
			CHAR = 0x0102,
			DEADCHAR = 0x0103,
			SYSKEYDOWN = 0x0104,
			SYSKEYUP = 0x0105,
			SYSCHAR = 0x0106,
			SYSDEADCHAR = 0x0107,
			UNICHAR = 0x0109,
			KEYLAST = 0x0109,
			IME_STARTCOMPOSITION = 0x010D,
			IME_ENDCOMPOSITION = 0x010E,
			IME_COMPOSITION = 0x010F,
			IMELAST = 0x010F,
			INITDIALOG = 0x0110,
			COMMAND = 0x0111,
			SYSCOMMAND = 0x0112,
			TIMER = 0x0113,
			HSCROLL = 0x0114,
			VSCROLL = 0x0115,
			INITMENU = 0x0116,
			INITMENUPOPUP = 0x0117,
			MENUSELECT = 0x011F,
			MENUCHAR = 0x0120,
			ENTERIDLE = 0x0121,
			MENURBUTTONUP = 0x0122,
			MENUDRAG = 0x0123,
			MENUGETOBJECT = 0x0124,
			UNINITMENUPOPUP = 0x0125,
			MENUCOMMAND = 0x0126,
			CHANGEUISTATE = 0x0127,
			UPDATEUISTATE = 0x0128,
			QUERYUISTATE = 0x0129,
			CTLCOLORMSGBOX = 0x0132,
			CTLCOLOREDIT = 0x0133,
			CTLCOLORLISTBOX = 0x0134,
			CTLCOLORBTN = 0x0135,
			CTLCOLORDLG = 0x0136,
			CTLCOLORSCROLLBAR = 0x0137,
			CTLCOLORSTATIC = 0x0138,
			GETHMENU = 0x01E1,
			MOUSEFIRST = 0x0200,
			MOUSEMOVE = 0x0200,
			LBUTTONDOWN = 0x0201,
			LBUTTONUP = 0x0202,
			LBUTTONDBLCLK = 0x0203,
			RBUTTONDOWN = 0x0204,
			RBUTTONUP = 0x0205,
			RBUTTONDBLCLK = 0x0206,
			MBUTTONDOWN = 0x0207,
			MBUTTONUP = 0x0208,
			MBUTTONDBLCLK = 0x0209,
			MOUSEWHEEL = 0x020A,
			XBUTTONDOWN = 0x020B,
			XBUTTONUP = 0x020C,
			XBUTTONDBLCLK = 0x020D,
			MOUSEHWHEEL = 0x020E,
			PARENTNOTIFY = 0x0210,
			ENTERMENULOOP = 0x0211,
			EXITMENULOOP = 0x0212,
			NEXTMENU = 0x0213,
			SIZING = 0x0214,
			CAPTURECHANGED = 0x0215,
			MOVING = 0x0216,
			POWERBROADCAST = 0x0218,
			DEVICECHANGE = 0x0219,
			MDICREATE = 0x0220,
			MDIDESTROY = 0x0221,
			MDIACTIVATE = 0x0222,
			MDIRESTORE = 0x0223,
			MDINEXT = 0x0224,
			MDIMAXIMIZE = 0x0225,
			MDITILE = 0x0226,
			MDICASCADE = 0x0227,
			MDIICONARRANGE = 0x0228,
			MDIGETACTIVE = 0x0229,
			MDISETMENU = 0x0230,
			ENTERSIZEMOVE = 0x0231,
			EXITSIZEMOVE = 0x0232,
			DROPFILES = 0x0233,
			MDIREFRESHMENU = 0x0234,
			IME_SETCONTEXT = 0x0281,
			IME_NOTIFY = 0x0282,
			IME_CONTROL = 0x0283,
			IME_COMPOSITIONFULL = 0x0284,
			IME_SELECT = 0x0285,
			IME_CHAR = 0x0286,
			IME_REQUEST = 0x0288,
			IMEDOWN = 0x0290,
			IMEUP = 0x0291,
			MOUSEHOVER = 0x02A1,
			MOUSELEAVE = 0x02A3,
			NCMOUSEHOVER = 0x02A0,
			NCMOUSELEAVE = 0x02A2,
			WTSSESSION_CHANGE = 0x02B1,
			TABLET_FIRST = 0x02c0,
			TABLET_LAST = 0x02df,
			CUT = 0x0300,
			COPY = 0x0301,
			PASTE = 0x0302,
			CLEAR = 0x0303,
			UNDO = 0x0304,
			RENDERFORMAT = 0x0305,
			RENDERALLFORMATS = 0x0306,
			DESTROYCLIPBOARD = 0x0307,
			DRAWCLIPBOARD = 0x0308,
			PAINTCLIPBOARD = 0x0309,
			VSCROLLCLIPBOARD = 0x030A,
			SIZECLIPBOARD = 0x030B,
			ASKCBFORMATNAME = 0x030C,
			CHANGECBCHAIN = 0x030D,
			HSCROLLCLIPBOARD = 0x030E,
			QUERYNEWPALETTE = 0x030F,
			PALETTEISCHANGING = 0x0310,
			PALETTECHANGED = 0x0311,
			HOTKEY = 0x0312,
			PRINT = 0x0317,
			PRINTCLIENT = 0x0318,
			APPCOMMAND = 0x0319,
			THEMECHANGED = 0x031A,
			CLIPBOARDUPDATE = 0x031D,
			DWMCOMPOSITIONCHANGED = 0x031E,
			DWMNCRENDERINGCHANGED = 0x031F,
			DWMCOLORIZATIONCOLORCHANGED = 0x0320,
			DWMWINDOWMAXIMIZEDCHANGE = 0x0321,
			GETTITLEBARINFOEX = 0x033F,
			HANDHELDFIRST = 0x0358,
			HANDHELDLAST = 0x035F,
			AFXFIRST = 0x0360,
			AFXLAST = 0x037F,
			PENWINFIRST = 0x0380,
			PENWINLAST = 0x038F,
			APP = 0x8000,
			USER = 0x0400,
			REFLECT = USER + 0x1C00,
		}

		public enum VK
		{
			LBUTTON = 1,
			RBUTTON = 2,
			CANCEL = 3,
			MBUTTON = 4,
			XBUTTON1 = 5,
			XBUTTON2 = 6,
			BACK = 8,
			TAB = 9,
			CLEAR = 12,
			RETURN = 13,
			SHIFT = 16,
			CONTROL = 17,
			MENU = 18,
			PAUSE = 19,
			CAPITAL = 20,
			KANA = 21,
			HANGUL = 21,
			JUNJA = 23,
			FINAL = 24,
			HANJA = 25,
			KANJI = 25,
			ESCAPE = 27,
			CONVERT = 28,
			NONCONVERT = 29,
			ACCEPT = 30,
			MODECHANGE = 31,
			SPACE = 32,
			PRIOR = 33,
			NEXT = 34,
			END = 35,
			HOME = 36,
			LEFT = 37,
			UP = 38,
			RIGHT = 39,
			DOWN = 40,
			SELECT = 41,
			PRINT = 42,
			EXECUTE = 43,
			SNAPSHOT = 44,
			INSERT = 45,
			DELETE = 46,
			HELP = 47,
			K0 = 48,
			K1 = 49,
			K2 = 50,
			K3 = 51,
			K4 = 52,
			K5 = 53,
			K6 = 54,
			K7 = 55,
			K8 = 56,
			K9 = 57,
			A = 65,
			B = 66,
			C = 67,
			D = 68,
			E = 69,
			F = 70,
			G = 71,
			H = 72,
			I = 73,
			J = 74,
			K = 75,
			L = 76,
			M = 77,
			N = 78,
			O = 79,
			P = 80,
			Q = 81,
			R = 82,
			S = 83,
			T = 84,
			U = 85,
			V = 86,
			W = 87,
			X = 88,
			Y = 89,
			Z = 90,
			LWIN = 91,
			RWIN = 92,
			APPS = 93,
			SLEEP = 95,
			NUMPAD0 = 96,
			NUMPAD1 = 97,
			NUMPAD2 = 98,
			NUMPAD3 = 99,
			NUMPAD4 = 100,
			NUMPAD5 = 101,
			NUMPAD6 = 102,
			NUMPAD7 = 103,
			NUMPAD8 = 104,
			NUMPAD9 = 105,
			MULTIPLY = 106,
			ADD = 107,
			SEPARATOR = 108,
			SUBTRACT = 109,
			DECIMAL = 110,
			DIVIDE = 111,
			F1 = 112,
			F2 = 113,
			F3 = 114,
			F4 = 115,
			F5 = 116,
			F6 = 117,
			F7 = 118,
			F8 = 119,
			F9 = 120,
			F10 = 121,
			F11 = 122,
			F12 = 123,
			F13 = 124,
			F14 = 125,
			F15 = 126,
			F16 = 127,
			F17 = 128,
			F18 = 129,
			F19 = 130,
			F20 = 131,
			F21 = 132,
			F22 = 133,
			F23 = 134,
			F24 = 135,
			NUMLOCK = 144,
			SCROLL = 145,
			LSHIFT = 160,
			RSHIFT = 161,
			LCONTROL = 162,
			RCONTROL = 163,
			LMENU = 164,
			RMENU = 165,
			BROWSER_BACK = 166,
			BROWSER_FORWARD = 167,
			BROWSER_REFRESH = 168,
			BROWSER_STOP = 169,
			BROWSER_SEARCH = 170,
			BROWSER_FAVORITES = 171,
			BROWSER_HOME = 172,
			VOLUME_MUTE = 173,
			VOLUME_DOWN = 174,
			VOLUME_UP = 175,
			MEDIA_NEXT_TRACK = 176,
			MEDIA_PREV_TRACK = 177,
			MEDIA_STOP = 178,
			MEDIA_PLAY_PAUSE = 179,
			LAUNCH_MAIL = 180,
			LAUNCH_MEDIA_SELECT = 181,
			LAUNCH_APP1 = 182,
			LAUNCH_APP2 = 183,
			OEM_1 = 186,
			OEM_PLUS = 187,
			OEM_COMMA = 188,
			OEM_MINUS = 189,
			OEM_PERIOD = 190,
			OEM_2 = 191,
			OEM_3 = 192,
			OEM_4 = 219,
			OEM_5 = 220,
			OEM_6 = 221,
			OEM_7 = 222,
			OEM_8 = 223,
			OEM_102 = 226,
			PROCESSKEY = 229,
			PACKET = 231,
			ATTN = 246,
			CRSEL = 247,
			EXSEL = 248,
			EREOF = 249,
			PLAY = 250,
			ZOOM = 251,
			NONAME = 252,
			PA1 = 253,
			OEM_CLEAR = 254
		}
		#endregion

		#region Delegates
		public delegate void WaveDelegate(IntPtr hdrvr, int uMsg, int dwUser, ref WaveHdr wavhdr, int dwParam2);
		public delegate IntPtr WindowProcess(IntPtr handle, uint msg, int wParam, int lParam);
		public delegate bool SwapInterval(int interval);
		#endregion

		#region Methods

		#region User
		[DllImport(User)]
		public static extern void PostQuitMessage(int exitCode);

		[DllImport(User, SetLastError = true, CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool SetWindowText(IntPtr hwnd, string text);

		[DllImport(User, SetLastError = true)]
		public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		[DllImport(User, SetLastError = true)]
		public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport(User, SetLastError = true)]
		public static extern IntPtr CreateWindowEx(uint dwExStyle, [MarshalAs(UnmanagedType.LPStr)] string lpClassName, [MarshalAs(UnmanagedType.LPStr)] string lpWindowName,
			uint dwStyle, int x, int y, int width, int height, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

		[DllImport(User)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool GetClientRect(IntPtr hWnd, out Rect lpRect);

		[DllImport(User)]
		public static extern IntPtr LoadCursorA(IntPtr handle, IntPtr cursor);

		[DllImport(User, SetLastError = true, CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		[DllImport(User, SetLastError = true)]
		public static extern int RegisterClassEx(ref WindowClassEx wcex);

		[DllImport(User)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool AdjustWindowRectEx(ref Rect lpRect, uint dwStyle, bool bMenu, uint dwExStyle);

		[DllImport(User, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool DestroyWindow(IntPtr hWnd);

		[DllImport(User)]
		public static extern int GetMessage(out Message msg, IntPtr hwnd, uint maxFilter, uint minFilter);

		[DllImport(User)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool TranslateMessage(ref Message msg);

		[DllImport(User)]
		public static extern IntPtr DispatchMessage(ref Message lpmsg);

		[DllImport(User)]
		public static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

		[DllImport(User, CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);

		[DllImport(User)]
		public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

		[DllImport(User, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

		[DllImport(User)]
		public static extern IntPtr DefWindowProc(IntPtr handle, uint msg, int min, int max);

		[DllImport(User, SetLastError = true)]
		public static extern IntPtr GetDC(IntPtr hWnd);

		[DllImport(User, SetLastError = true)]
		public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
		#endregion

		#region Kernel
		[DllImport(Kernel, CharSet = CharSet.Auto)]
		public static extern IntPtr GetModuleHandle(string lpModuleName);

		[DllImport(Kernel, CharSet = CharSet.Auto)]
		public static extern IntPtr LoadLibrary(string lpLibraryName);

		[DllImport(Kernel, CharSet = CharSet.Auto)]
		public static extern bool FreeLibrary(IntPtr hLibModule);
		#endregion

		#region Winmm
		[DllImport(Winmm, EntryPoint = "waveOutPrepareHeader", SetLastError = true)]
		public static extern int WaveOutPrepareHeader(IntPtr hWaveOut, ref WaveHdr lpWaveOutHdr, int uSize);

		[DllImport(Winmm, EntryPoint = "waveOutUnprepareHeader", SetLastError = true)]
		public static extern int WaveOutUnprepareHeader(IntPtr hWaveOut, ref WaveHdr lpWaveOutHdr, int uSize);

		[DllImport(Winmm, EntryPoint = "waveOutOpen", SetLastError = true)]
		public static extern int WaveOutOpen(out IntPtr hWaveOut, int uDeviceID, WaveFormatEx lpFormat, WaveDelegate dwCallback, int dwInstance, int dwFlags);

		[DllImport(Winmm, EntryPoint = "waveOutWrite", SetLastError = true)]
		public static extern int WaveOutWrite(IntPtr hWaveOut, ref WaveHdr lpWaveOutHdr, int uSize);
		#endregion

		#region PixGL
		[DllImport(PixGl, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SetValues(float pw, float ph, float ww, float wh);

		[DllImport(PixGl, CallingConvention = CallingConvention.Cdecl)]
		public static extern void CreateCoords(int pixW, int pixH, int scrW, int scrH);

		[DllImport(PixGl, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DestroyCoords();

		[DllImport(PixGl, CallingConvention = CallingConvention.Cdecl)]
		public unsafe static extern void RenderUnitPixels(int width, int height, Pixel* pixels);

		[DllImport(PixGl, CallingConvention = CallingConvention.Cdecl)]
		public unsafe static extern void RenderPixels(int width, int height, Pixel* pixels);

		[DllImport(PixGl, CallingConvention = CallingConvention.Cdecl)]
		public unsafe static extern void RenderText(int scrW, int scrH, int width, int height, Pixel* pixels);

		#region PixMp3
		[DllImport(PixGl, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Convert")]
		public static extern bool ConvertToMp3([MarshalAs(UnmanagedType.LPWStr)] string source, [MarshalAs(UnmanagedType.LPWStr)] string target);
		#endregion
		#endregion

		#region OpenGL
		[DllImport(OpenGl, SetLastError = true, EntryPoint = "wglCreateContext")]
		public static extern IntPtr WglCreateContext(IntPtr hdc);

		[DllImport(OpenGl, SetLastError = true, EntryPoint = "wglDeleteContext")]
		public static extern IntPtr WglDeleteContext(IntPtr hdc);

		[DllImport(OpenGl, SetLastError = true, EntryPoint = "wglGetProcAddress")]
		public static extern IntPtr WglGetProcAddress(string name);

		[DllImport(OpenGl, SetLastError = true, EntryPoint = "wglMakeCurrent")]
		public static extern int WglMakeCurrent(IntPtr hdc, IntPtr hrc);

		[DllImport(OpenGl, SetLastError = true, EntryPoint = "glEnable")]
		public static extern void GlEnable(uint cap);

		[DllImport(OpenGl, SetLastError = true, EntryPoint = "glGenTextures")]
		public static extern void GlGenTextures(int n, [MarshalAs(UnmanagedType.LPArray)] uint[] textures);

		[DllImport(OpenGl, SetLastError = true, EntryPoint = "glBindTexture")]
		public static extern void GlBindTexture(uint target, uint texture);

		[DllImport(OpenGl, SetLastError = true, EntryPoint = "glTexParameteri")]
		public static extern void GlTexParameteri(uint target, uint pname, int param);

		[DllImport(OpenGl, SetLastError = true, EntryPoint = "glTexEnvf")]
		public static extern void GlTexEnvf(uint target, uint pname, float param);

		[DllImport(OpenGl, SetLastError = true, EntryPoint = "glTexImage2D")]
		public static extern void GlTexImage2D(uint target, int level, uint internalformat, int width, int height, int border, uint format, uint type, Pixel* pixels);

		[DllImport(OpenGl, SetLastError = true, EntryPoint = "glTexSubImage2D")]
		public static extern void GlTexSubImage2D(uint target, int level, int xoffset, int yoffset, int width, int height, uint format, uint type, Pixel* pixels);

		//[SuppressUnmanagedCodeSecurity]
		[DllImport(OpenGl, SetLastError = true, EntryPoint = "glBegin")]
		public static extern void GlBegin(uint mode);

		//[SuppressUnmanagedCodeSecurity]
		[DllImport(OpenGl, SetLastError = true, EntryPoint = "glEnd")]
		public static extern void GlEnd();

		[DllImport(OpenGl, SetLastError = true, EntryPoint = "glTexCoord2f")]
		public static extern void GlTexCoord2f(float s, float t);

		[DllImport(OpenGl, SetLastError = true, EntryPoint = "glVertex3f")]
		public static extern void GlVertex3f(float x, float y, float z);

		//[SuppressUnmanagedCodeSecurity]
		[DllImport(OpenGl, SetLastError = true, EntryPoint = "glVertex2f")]
		public static extern void GlVertex2f(float x, float y);

		//[SuppressUnmanagedCodeSecurity]
		[DllImport(OpenGl, SetLastError = true, EntryPoint = "glColor3ub")]
		public static extern void GlColor3ub(byte red, byte green, byte blue);
		#endregion

		#region Gdi
		[DllImport(Gdi, SetLastError = true)]
		public unsafe static extern int ChoosePixelFormat(IntPtr hDC, [In] ref PixelFormatDesc ppfd);

		[DllImport(Gdi, SetLastError = true)]
		public unsafe static extern int SetPixelFormat(IntPtr hDC, int iPixelFormat, [In] ref PixelFormatDesc ppfd);

		[DllImport(Gdi, SetLastError = true)]
		public static extern bool SwapBuffers(IntPtr hDC);
		#endregion

		#endregion

		#region Structs
		[StructLayout(LayoutKind.Sequential)]
		public struct Rect
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct Message
		{
			public IntPtr Handle;
			public uint Msg;
			public IntPtr WParam;
			public IntPtr LParam;
			public int Time;
			public Point Point;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct Point
		{
			public int X;
			public int Y;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		internal struct MonitorInfo
		{
			public int Size;
			public Rect Monitor;
			public Rect WorkArea;
			public uint Flags;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WaveHdr
		{
			public IntPtr Data;
			public int BufferLength;
			public int BytesRecorded;
			public IntPtr User;
			public int Flags;
			public int Loops;
			public IntPtr Next;
			public int Reserved;
		}

		[StructLayout(LayoutKind.Sequential)]
		public class WaveFormatEx
		{
			public short FormatTag;
			public short Channels;
			public int SamplesPerSec;
			public int AvgBytesPerSec;
			public short BlockAlign;
			public short BitsPerSample;
			public short Size;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct WindowClassEx
		{
			[MarshalAs(UnmanagedType.U4)]
			public uint Size;
			[MarshalAs(UnmanagedType.U4)]
			public int Style;
			public WindowProcess WindowsProc;
			public int ClsExtra;
			public int WndExtra;
			public IntPtr Instance;
			public IntPtr Icon;
			public IntPtr Cursor;
			public IntPtr Background;
			[MarshalAs(UnmanagedType.LPStr)]
			public string MenuName;
			[MarshalAs(UnmanagedType.LPStr)]
			public string ClassName;
			public IntPtr IconSm;
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct PixelFormatDesc
		{
			[FieldOffset(0)]
			public ushort Size;
			[FieldOffset(2)]
			public ushort Version;
			[FieldOffset(4)]
			public uint Flags;
			[FieldOffset(8)]
			public byte PixelType;
			[FieldOffset(9)]
			public byte ColorBits;
			[FieldOffset(10)]
			public byte RedBits;
			[FieldOffset(11)]
			public byte RedShift;
			[FieldOffset(12)]
			public byte GreenBits;
			[FieldOffset(13)]
			public byte GreenShift;
			[FieldOffset(14)]
			public byte BlueBits;
			[FieldOffset(15)]
			public byte BlueShift;
			[FieldOffset(16)]
			public byte AlphaBits;
			[FieldOffset(17)]
			public byte AlphaShift;
			[FieldOffset(18)]
			public byte AccumBits;
			[FieldOffset(19)]
			public byte AccumRedBits;
			[FieldOffset(20)]
			public byte AccumGreenBits;
			[FieldOffset(21)]
			public byte AccumBlueBits;
			[FieldOffset(22)]
			public byte AccumAlphaBits;
			[FieldOffset(23)]
			public byte DepthBits;
			[FieldOffset(24)]
			public byte StencilBits;
			[FieldOffset(25)]
			public byte AuxBuffers;
			[FieldOffset(26)]
			public sbyte LayerType;
			[FieldOffset(27)]
			public byte Reserved;
			[FieldOffset(28)]
			public uint LayerMask;
			[FieldOffset(32)]
			public uint VisibleMask;
			[FieldOffset(36)]
			public uint DamageMask;

			public PixelFormatDesc(ushort version, uint flags, byte pixelType, byte colorBits,
				byte redBits, byte redShift, byte greenBits, byte greenShift, byte blueBits, byte blueShift, byte alphaBits, byte alphaShift,
				byte accumBits, byte accumRedBits, byte accumGreenBits, byte accumBlueBits, byte accumAlphaBits,
				byte depthBits, byte stencilBits, byte auxBuffers, sbyte layerType, byte reserved,
				uint layerMask, uint visibleMask, uint damageMask) : this()
			{
				Size = (ushort)Marshal.SizeOf(this);
				Version = version;
				Flags = flags;
				PixelType = pixelType;
				ColorBits = colorBits;
				RedBits = redBits;
				RedShift = redShift;
				GreenBits = greenBits;
				GreenShift = greenShift;
				BlueBits = blueBits;
				BlueShift = blueShift;
				AlphaBits = alphaBits;
				AlphaShift = alphaShift;
				AccumBits = accumBits;
				AccumRedBits = accumRedBits;
				AccumGreenBits = accumGreenBits;
				AccumBlueBits = accumBlueBits;
				AccumAlphaBits = accumAlphaBits;
				DepthBits = depthBits;
				StencilBits = stencilBits;
				AuxBuffers = auxBuffers;
				LayerType = layerType;
				Reserved = reserved;
				LayerMask = layerMask;
				VisibleMask = visibleMask;
				DamageMask = damageMask;
			}
		}
		#endregion
	}
}