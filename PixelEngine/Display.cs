using System;
using System.Runtime.InteropServices;

using static PixelEngine.Windows;

namespace PixelEngine
{
	public delegate IntPtr WindowProcess(IntPtr handle, uint msg, int wParam, int lParam);

	public class Display
	{
		private static WindowProcess proc;

		private bool init;
		private string text;

		public Display() => proc = WndProc;

		// Display details
		public int ScreenWidth { get; private set; }
		public int ScreenHeight { get; private set; }
		public int PixWidth { get; private set; }
		public int PixHeight { get; private set; }

		// Title of the app
		protected virtual string AppName
		{
			get => text;
			set
			{
				text = value;
				if(init)
					SetWindowText(Handle, text);
			}
		}

		// Name of class used to make a window
		private string ClassName => GetType().FullName;
		
		// Client area of window
		internal Rect ClientRect { get; set; }

		// Handle of the window
		protected internal IntPtr Handle { get; protected set; }

		// Assign the window details
		public void Construct(int width = 256, int height = 256, int pixWidth = 4, int pixHeight = 4)
		{
			this.ScreenWidth = width;
			this.ScreenHeight = height;
			this.PixWidth = pixWidth;
			this.PixHeight = pixHeight;
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
			int def = 250;

			this.Handle = CreateWindowEx(0, ClassName, AppName, (uint)(WindowStyles.OverlappedWindow | WindowStyles.Visible),
				def, def, ScreenWidth, ScreenHeight, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

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