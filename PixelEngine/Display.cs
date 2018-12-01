using System;
using System.Runtime.InteropServices;

using static PixelEngine.Windows;

namespace PixelEngine
{
	public class Display
	{
		private WindowProcess proc;

		private bool init;
		private string text;

		public Display() => proc = WndProc;

		// Display details
		public int ScreenWidth { get; private protected set; }
		public int ScreenHeight { get; private protected set; }
		public int PixWidth { get; private protected set; }
		public int PixHeight { get; private protected set; }

		internal int windowWidth;
		internal int windowHeight;

		// Title of the app
		public virtual string AppName
		{
			get => text;
			protected set
			{
				text = value;
				if(init)
					SetWindowText(Handle, text);
			}
		}

		// Name of class used to make a window
		private protected string ClassName => GetType().FullName;
		
		// Client area of window
		internal Rect ClientRect { get; set; }

		// Handle of the window
		internal IntPtr Handle { get; set; }

		// Assign the window details
		internal void Construct(int width = 100, int height = 100, int pixWidth = 5, int pixHeight = 5)
		{
			ScreenWidth = width;
			ScreenHeight = height;
			PixWidth = pixWidth;
			PixHeight = pixHeight;

			windowWidth = ScreenWidth * PixWidth;
			windowHeight = ScreenHeight * PixHeight;
		}

		// Start the windows message pump
		private protected void MessagePump()
		{
			while (GetMessage(out Message msg, IntPtr.Zero, 0, 0) > 0)
			{
				TranslateMessage(ref msg);
				DispatchMessage(ref msg);
			}
		}

		// Create the window using the winapi
		private protected virtual void CreateWindow()
		{
			if (string.IsNullOrWhiteSpace(AppName))
				AppName = GetType().Name;

			Handle = CreateWindowEx(0, ClassName, AppName, (uint)(WindowStyles.OverlappedWindow | WindowStyles.Visible),
					0, 0, windowWidth, windowHeight, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
				
			GetClientRect(Handle, out Rect r);
			ClientRect = r;

			init = true;
		}

		// Register the class with the winapi
		private protected virtual void RegisterClass()
		{
			WindowClassEx wcex = new WindowClassEx()
			{
				Style = DoubleClicks | VRedraw | HRedraw,
				WindowsProc = proc,
				ClsExtra = 0,
				WndExtra = 0,
				Icon = LoadIcon(IntPtr.Zero, (IntPtr)ApplicationIcon),
				Cursor = LoadCursorA(IntPtr.Zero, (IntPtr)ArrowCursor),
				IconSm = IntPtr.Zero,
				Background = (IntPtr)(ColorWindow + 1),
				MenuName = null,
				ClassName = ClassName
			};
			wcex.Size = (uint)Marshal.SizeOf(wcex);

			RegisterClassEx(ref wcex);
		}

		// Handle messages for processing
		private protected virtual IntPtr WndProc(IntPtr handle, uint msg, int wParam, int lParam)
		{
			switch (msg)
			{
				case (uint)WM.CLOSE:
					DestroyWindow(handle);
					break;
				case (uint)WM.DESTROY:
					PostQuitMessage(0);
					break;
				default:
					return DefWindowProc(handle, msg, wParam, lParam);
			}
			return IntPtr.Zero;
		}
	}
}