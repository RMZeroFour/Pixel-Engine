using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using PixelEngine.Extensions;
using PixelEngine.Utilities;

using static PixelEngine.Windows;

namespace PixelEngine
{
	public abstract class Game : Display
	{
		#region Members
		protected Pixel.Mode PixelMode { get; set; } = Pixel.Mode.Normal;
		protected float PixelBlend { get => pixBlend; set => pixBlend = Constrain(value, 0, 1); }
		protected long FrameCount { get; private set; }
		protected bool Focus { get; private set; }
		protected int FrameRate { get; private set; }
		protected int MouseX { get; private set; }
		protected int MouseY { get; private set; }
		protected Scroll MouseScroll { get; private set; }
		protected Clock Clock { get; private set; }
		protected float Volume
		{
			get
			{
				return audio != null ? audio.Volume : 0;
			}
			set
			{
				if (audio != null)
					audio.Volume = Constrain(value, 0, 1);
			}
		}
		public float AudioTime
		{
			get
			{
				if (audio == null)
					return 0;
				return audio.GlobalTime;
			}
		}
		protected Shader Shader
		{
			get => shader;
			set
			{
				shader = value;
				PixelMode = shader != null ? Pixel.Mode.Custom : Pixel.Mode.Normal;
			}
		}
		protected Sprite DrawTarget
		{
			get => drawTarget;
			set => drawTarget = value ?? defDrawTarget;
		}

		public override string AppName
		{
			get => base.AppName;
			protected set
			{
				base.AppName = value;
				if (Handle != IntPtr.Zero)
					SetWindowText(Handle, AppName);
			}
		}

		private Thread gameLoop;

		private AudioEngine audio;
		private OpenGL canvas;

		private float pixBlend = 1;

		private Timer frameTimer;

		private static WindowProcess proc;

		private bool hrText;

		private bool active;
		private bool paused;

		private readonly Dictionary<uint, Key> mapKeys = new Dictionary<uint, Key>();

		private Sprite drawTarget;
		private Sprite textTarget;
		private Sprite defDrawTarget;

		private Sprite fontSprite;

		private Button anyKey;
		private Button noneKey;
		private Button anyMouse;
		private Button noneMouse;

		private bool delaying;
		private float delayTime;
		private Shader shader;
		private readonly Button[] keyboard = new Button[256];
		private readonly bool[] newKeyboard = new bool[256];
		private readonly bool[] oldKeyboard = new bool[256];

		private readonly Button[] mouse = new Button[3];
		private readonly bool[] newMouse = new bool[3];
		private readonly bool[] oldMouse = new bool[3];

		private const int CharWidth = 8;
		private const int CharHeight = 8;
		#endregion

		#region Working
		public void Start()
		{
			RegisterClass();
			CreateWindow();

			active = true;

			gameLoop = new Thread(GameLoop);
			gameLoop.Start();

			MessagePump();
		}
		public void Construct(int width = 100, int height = 100, int pixWidth = 5, int pixHeight = 5, int frameRate = -1)
		{
			base.Construct(width, height, pixWidth, pixHeight);

			if (frameRate != -1)
			{
				FrameRate = frameRate;
				frameTimer = new Timer(1000.0f / FrameRate);
			}

			ConstructFontSheet();
			HandleDrawTarget();
		}
		private void GameLoop()
		{
			Clock = new Clock();

			Extension.Init(this);

			OnCreate();

			canvas = new OpenGL();
			canvas.Create(this);
			canvas.Initialize(defDrawTarget, textTarget);

			DateTime t1, t2;
			t1 = t2 = DateTime.Now;

			if (frameTimer != null)
				frameTimer.Init(t1);

			while (active)
			{
				while (active)
				{
					t2 = DateTime.Now;
					Clock.Elapsed = t2 - t1;
					float elapsed = (float)Clock.Elapsed.TotalSeconds;
					t1 = t2;

					if (frameTimer != null && !frameTimer.Tick())
						continue;

					if (delaying)
					{
						delayTime -= elapsed;

						if (delayTime <= 0)
						{
							delayTime = 0;
							delaying = false;
						}
						else
						{
							continue;
						}
					}

					if (paused)
						continue;

					OnUpdate(elapsed);

					HandleKeyboard();
					HandleMouse();

					FrameCount++;

					canvas.Draw(defDrawTarget, textTarget);
				}

				OnDestroy();
			}

			if (audio != null)
				audio.DestroyAudio();

			canvas.Destroy();

			PostMessage(Handle, (uint)WM.DESTROY, IntPtr.Zero, IntPtr.Zero);

			DestroyTempPath();
		}
		private void HandleMouse()
		{
			for (int i = 0; i < 3; i++)
			{
				mouse[i].Pressed = false;
				mouse[i].Released = false;

				if (newMouse[i] != oldMouse[i])
				{
					if (newMouse[i])
					{
						mouse[i].Pressed = !mouse[i].Down;
						mouse[i].Down = true;
					}
					else
					{
						mouse[i].Released = true;
						mouse[i].Down = false;
					}
				}

				if (mouse[i].Down)
					OnMouseDown((Mouse)i);

				oldMouse[i] = newMouse[i];
			}

			anyMouse.Pressed = mouse.Any(m => m.Pressed);
			anyMouse.Down = mouse.Any(m => m.Down);
			anyMouse.Released = mouse.Any(m => m.Released);

			noneMouse.Pressed = !anyMouse.Pressed;
			noneMouse.Down = !anyMouse.Down;
			noneMouse.Released = !anyMouse.Released;

			MouseScroll = Scroll.None;
		}
		private void HandleKeyboard()
		{
			for (int i = 0; i < 256; i++)
			{
				keyboard[i].Pressed = false;
				keyboard[i].Released = false;

				if (newKeyboard[i] != oldKeyboard[i])
				{
					if (newKeyboard[i])
					{
						keyboard[i].Pressed = !keyboard[i].Down;
						keyboard[i].Down = true;
					}
					else
					{
						keyboard[i].Released = true;
						keyboard[i].Down = false;
					}
				}

				if (keyboard[i].Down)
					OnKeyDown((Key)i);

				oldKeyboard[i] = newKeyboard[i];
			}

			anyKey.Pressed = keyboard.Any(k => k.Pressed);
			anyKey.Down = keyboard.Any(k => k.Down);
			anyKey.Released = keyboard.Any(k => k.Released);

			noneKey.Pressed = !anyKey.Pressed;
			noneKey.Down = !anyKey.Down;
			noneKey.Released = !anyKey.Released;
		}
		private void HandleDrawTarget()
		{
			defDrawTarget = new Sprite(ScreenWidth, ScreenHeight);
			DrawTarget = defDrawTarget;
		}
		private protected override IntPtr WndProc(IntPtr handle, uint msg, int wParam, int lParam)
		{
			uint LoWord(uint val) => val & 0xFFFF;
			uint HiWord(uint val) => val >> 16;

			switch (msg)
			{
				case (uint)WM.MOUSEMOVE:
					UpdateMouse(LoWord((uint)lParam), HiWord((uint)lParam));
					break;
				case (uint)WM.SETFOCUS:
					Focus = true;
					break;
				case (uint)WM.KILLFOCUS:
					Focus = false;
					break;
				case (uint)WM.KEYDOWN:
					if (!mapKeys.ContainsKey((uint)wParam))
						break;
					Key kd = mapKeys[(uint)wParam];
					newKeyboard[(int)kd] = true;
					OnKeyPress(kd);
					break;
				case (uint)WM.KEYUP:
					if (!mapKeys.ContainsKey((uint)wParam))
						break;
					Key ku = mapKeys[(uint)wParam];
					newKeyboard[(int)ku] = false;
					OnKeyRelease(ku);
					break;
				case (uint)WM.MOUSEWHEEL:
					short wheel = (short)(wParam >> 16);
					MouseScroll = (Scroll)(wheel / WheelDelta);
					OnMouseScroll();
					break;
				case (uint)WM.LBUTTONDOWN:
					newMouse[(int)Mouse.Left] = true;
					OnMousePress(Mouse.Left);
					break;
				case (uint)WM.LBUTTONUP:
					newMouse[(int)Mouse.Left] = false;
					OnMouseRelease(Mouse.Left);
					break;
				case (uint)WM.RBUTTONDOWN:
					newMouse[(int)Mouse.Right] = true;
					OnMousePress(Mouse.Right);
					break;
				case (uint)WM.RBUTTONUP:
					newMouse[(int)Mouse.Right] = false;
					OnMouseRelease(Mouse.Right);
					break;
				case (uint)WM.MBUTTONDOWN:
					newMouse[(int)Mouse.Middle] = true;
					OnMousePress(Mouse.Middle);
					break;
				case (uint)WM.MBUTTONUP:
					newMouse[(int)Mouse.Middle] = false;
					OnMouseRelease(Mouse.Middle);
					break;
				case (uint)WM.CLOSE:
					Finish();
					break;
				case (uint)WM.DESTROY:
					PostQuitMessage(0);
					break;
				default:
					return DefWindowProc(handle, msg, wParam, lParam);
			}
			return IntPtr.Zero;
		}
		#endregion

		#region Helpers
		#region Engine
		protected void Delay(float time)
		{
			if (!delaying)
				delaying = true;
			delayTime += time;
		}

		protected void Continue() => active = true;
		protected void Finish() => active = false;

		protected void NoLoop() => paused = true;
		protected void Loop() => paused = false;

		protected Button GetKey(Key k)
		{
			if (k == Key.Any)
				return anyKey;
			if (k == Key.None)
				return noneKey;
			return keyboard[(int)k];
		}
		protected Button GetMouse(Mouse m)
		{
			if (m == Mouse.Any)
				return anyMouse;
			if (m == Mouse.None)
				return noneMouse;
			return mouse[(int)m];
		}

		protected Pixel GetScreenPixel(int x, int y) => defDrawTarget[x, y];
		protected Pixel[,] GetScreenPixels()
		{
			Pixel[,] screen = new Pixel[ScreenWidth, ScreenHeight];
			for (int i = 0; i < ScreenWidth* ScreenHeight; i++)
			{
				int x = i % ScreenWidth;
				int y = i / ScreenWidth;
				screen[x, y] = defDrawTarget[x, y];
			}
			return screen;
		}
		#endregion

		#region Math
		protected static readonly float PI = (float)Math.PI;

		protected float Sin(float val) => (float)Math.Sin(val);
		protected float Cos(float val) => (float)Math.Cos(val);
		protected float Tan(float val) => (float)Math.Tan(val);

		protected float Power(float val, float pow) => (float)Math.Pow(val, pow);
		protected float Round(float val, int digits = 0) => (float)Math.Round(val, digits);

		protected float Map(float val, float oMin, float oMax, float nMin, float nMax) => (val - oMin) / (oMax - oMin) * (nMax - nMin) + nMin;
		protected float Constrain(float val, float min, float max) => Math.Max(Math.Min(max, val), min);
		protected float Lerp(float start, float end, float amt) => Map(amt, 0, 1, start, end);
		protected float Wrap(float val, float min, float max)
		{
			if (val > max)
				return val - min;
			if (val < min)
				return val - max;
			return val;
		}
		protected float Distance(float x1, float y1, float x2, float y2) => Power(Power(x2 - x1, 2) + Power(y2 - y1, 2), 1 / 2);
		protected float Magnitude(float x, float y) => Power(Power(x, 2) + Power(y, 2), 1 / 2);
		protected bool Between(float val, float min, float max) => val > min && val < max;

		protected void Seed() => Randoms.Seed = Environment.TickCount % int.MaxValue;
		protected void Seed(int s) => Randoms.Seed = s;
		protected int Random(int max) => Random(0, max);
		protected int Random(int min, int max) => Randoms.RandomInt(min, max);
		protected float Random() => Random(0f, 1f);
		protected float Random(float max) => Random(0, max);
		protected float Random(float min, float max) => Randoms.RandomFloat(min, max);
		protected T Random<T>(params T[] list) => list[Random(list.Length)];
		protected T Random<T>(List<T> list) => list[Random(list.Count)];
		protected T Random<T>(IEnumerable<T> list) => Random(list.ToArray());

		protected float Degrees(float radians) => (float)(radians * 180 / Math.PI);
		protected float Radians(float degrees) => (float)(degrees * Math.PI / 180);
		#endregion

		#region Collections
		protected T[] MakeArray<T>(params T[] items) => items;
		protected T[] MakeArray<T>(int count, Func<int, T> selector)
		{
			T[] arr = new T[count];
			for (int i = 0; i < count; i++)
				arr[i] = selector(i);
			return arr;
		}

		protected List<T> MakeList<T>(params T[] items) => items.ToList();
		protected List<T> MakeList<T>(int count, Func<int, T> selector)
		{
			List<T> list = new List<T>(count);
			for (int i = 0; i < count; i++)
				list.Add(selector(i));
			return list;
		}
		#endregion
		#endregion

		#region Inner
		private void UpdateMouse(uint x, uint y)
		{
			MouseX = (int)x / PixWidth;
			MouseY = (int)y / PixHeight;
		}
		private void ConstructFontSheet()
		{
			StringBuilder data = new StringBuilder(1024);
			data.Append("?Q`0001oOch0o01o@F40o0<AGD4090LAGD<090@A7ch0?00O7Q`0600>00000000");
			data.Append("O000000nOT0063Qo4d8>?7a14Gno94AA4gno94AaOT0>o3`oO400o7QN00000400");
			data.Append("Of80001oOg<7O7moBGT7O7lABET024@aBEd714AiOdl717a_=TH013Q>00000000");
			data.Append("720D000V?V5oB3Q_HdUoE7a9@DdDE4A9@DmoE4A;Hg]oM4Aj8S4D84@`00000000");
			data.Append("OaPT1000Oa`^13P1@AI[?g`1@A=[OdAoHgljA4Ao?WlBA7l1710007l100000000");
			data.Append("ObM6000oOfMV?3QoBDD`O7a0BDDH@5A0BDD<@5A0BGeVO5ao@CQR?5Po00000000");
			data.Append("Oc``000?Ogij70PO2D]??0Ph2DUM@7i`2DTg@7lh2GUj?0TO0C1870T?00000000");
			data.Append("70<4001o?P<7?1QoHg43O;`h@GT0@:@LB@d0>:@hN@L0@?aoN@<0O7ao0000?000");
			data.Append("OcH0001SOglLA7mg24TnK7ln24US>0PL24U140PnOgl0>7QgOcH0K71S0000A000");
			data.Append("00H00000@Dm1S007@DUSg00?OdTnH7YhOfTL<7Yh@Cl0700?@Ah0300700000000");
			data.Append("<008001QL00ZA41a@6HnI<1i@FHLM81M@@0LG81?O`0nC?Y7?`0ZA7Y300080000");
			data.Append("O`082000Oh0827mo6>Hn?Wmo?6HnMb11MP08@C11H`08@FP0@@0004@000000000");
			data.Append("00P00001Oab00003OcKP0006@6=PMgl<@440MglH@000000`@000001P00000000");
			data.Append("Ob@8@@00Ob@8@Ga13R@8Mga172@8?PAo3R@827QoOb@820@0O`0007`0000007P0");
			data.Append("O`000P08Od400g`<3V=P0G`673IP0`@3>1`00P@6O`P00g`<O`000GP800000000");
			data.Append("?P9PL020O`<`N3R0@E4HC7b0@ET<ATB0@@l6C4B0O`H3N7b0?P01L3R000000020");

			fontSprite = new Sprite(128, 48);
			int px = 0, py = 0;
			for (int b = 0; b < 1024; b += 4)
			{
				uint sym1 = (uint)data[b + 0] - 48;
				uint sym2 = (uint)data[b + 1] - 48;
				uint sym3 = (uint)data[b + 2] - 48;
				uint sym4 = (uint)data[b + 3] - 48;
				uint r = sym1 << 18 | sym2 << 12 | sym3 << 6 | sym4;

				for (int i = 0; i < 24; i++)
				{
					Pixel p = (r & (1 << i)) != 0 ? Pixel.Presets.White : Pixel.Presets.Black;
					fontSprite[px, py] = p;
					if (++py == 48)
					{
						px++;
						py = 0;
					}
				}
			}
		}
		private protected override void CreateWindow()
		{
			uint styleEx = (uint)(WindowStylesEx.AppWindow | WindowStylesEx.WindowEdge);
			uint style = (uint)(WindowStyles.Caption | WindowStyles.SysMenu | WindowStyles.Visible);
			Rect winRect = new Rect() { Left = 0, Top = 0, Right = windowWidth, Bottom = windowHeight };

			AdjustWindowRectEx(ref winRect, style, false, styleEx);

			int width = winRect.Right - winRect.Left;
			int height = winRect.Bottom - winRect.Top;

			if (string.IsNullOrWhiteSpace(AppName))
				AppName = GetType().Name;

			WindowStyles winStyle = WindowStyles.Overlapped | WindowStyles.Visible | WindowStyles.Caption | WindowStyles.SysMenu | WindowStyles.MinimizeBox;
			Handle = CreateWindowEx(0, ClassName, AppName, (uint)winStyle,
					0, 0, width, height, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

			GetClientRect(Handle, out Rect r);
			ClientRect = r;

			MapKeyboard();
		}
		private protected override void RegisterClass()
		{
			WindowClassEx wc = new WindowClassEx
			{
				Icon = LoadIcon(IntPtr.Zero, (IntPtr)ApplicationIcon),
				Cursor = LoadCursorA(IntPtr.Zero, (IntPtr)ArrowCursor),
				Style = HRedraw | VRedraw | OwnDC,
				Instance = GetModuleHandle(null),
				WindowsProc = (proc = WndProc),
				ClsExtra = 0,
				WndExtra = 0,
				MenuName = null,
				Background = IntPtr.Zero,
				ClassName = GetType().FullName
			};
			wc.Size = (uint)Marshal.SizeOf(wc);

			RegisterClassEx(ref wc);
		}
		private void MapKeyboard()
		{
			mapKeys[0x41] = Key.A; mapKeys[0x42] = Key.B; mapKeys[0x43] = Key.C; mapKeys[0x44] = Key.D; mapKeys[0x45] = Key.E;
			mapKeys[0x46] = Key.F; mapKeys[0x47] = Key.G; mapKeys[0x48] = Key.H; mapKeys[0x49] = Key.I; mapKeys[0x4A] = Key.J;
			mapKeys[0x4B] = Key.K; mapKeys[0x4C] = Key.L; mapKeys[0x4D] = Key.M; mapKeys[0x4E] = Key.N; mapKeys[0x4F] = Key.O;
			mapKeys[0x50] = Key.P; mapKeys[0x51] = Key.Q; mapKeys[0x52] = Key.R; mapKeys[0x53] = Key.S; mapKeys[0x54] = Key.T;
			mapKeys[0x55] = Key.U; mapKeys[0x56] = Key.V; mapKeys[0x57] = Key.W; mapKeys[0x58] = Key.X; mapKeys[0x59] = Key.Y;
			mapKeys[0x5A] = Key.Z;

			mapKeys[(uint)VK.F1] = Key.F1; mapKeys[(uint)VK.F2] = Key.F2; mapKeys[(uint)VK.F3] = Key.F3; mapKeys[(uint)VK.F4] = Key.F4;
			mapKeys[(uint)VK.F5] = Key.F5; mapKeys[(uint)VK.F6] = Key.F6; mapKeys[(uint)VK.F7] = Key.F7; mapKeys[(uint)VK.F8] = Key.F8;
			mapKeys[(uint)VK.F9] = Key.F9; mapKeys[(uint)VK.F10] = Key.F10; mapKeys[(uint)VK.F11] = Key.F11; mapKeys[(uint)VK.F12] = Key.F12;

			mapKeys[(uint)VK.DOWN] = Key.Down; mapKeys[(uint)VK.LEFT] = Key.Left; mapKeys[(uint)VK.RIGHT] = Key.Right; mapKeys[(uint)VK.UP] = Key.Up;

			mapKeys[(uint)VK.BACK] = Key.Back; mapKeys[(uint)VK.ESCAPE] = Key.Escape; mapKeys[(uint)VK.RETURN] = Key.Enter; mapKeys[(uint)VK.PAUSE] = Key.Pause;
			mapKeys[(uint)VK.SCROLL] = Key.Scroll; mapKeys[(uint)VK.TAB] = Key.Tab; mapKeys[(uint)VK.DELETE] = Key.Delete; mapKeys[(uint)VK.HOME] = Key.Home;
			mapKeys[(uint)VK.END] = Key.End; mapKeys[(uint)VK.PRIOR] = Key.PageUp; mapKeys[(uint)VK.NEXT] = Key.PageDown; mapKeys[(uint)VK.INSERT] = Key.Insert;
			mapKeys[(uint)VK.SHIFT] = Key.Shift; mapKeys[(uint)VK.CONTROL] = Key.Control;
			mapKeys[(uint)VK.SPACE] = Key.Space;

			mapKeys[0x30] = Key.K0; mapKeys[0x31] = Key.K1; mapKeys[0x32] = Key.K2; mapKeys[0x33] = Key.K3; mapKeys[0x34] = Key.K4;
			mapKeys[0x35] = Key.K5; mapKeys[0x36] = Key.K6; mapKeys[0x37] = Key.K7; mapKeys[0x38] = Key.K8; mapKeys[0x39] = Key.K9;
		}
		#endregion

		#region Drawing
		public void Draw(int x, int y, Pixel col) => Draw(new Point(x, y), col);
		public void Draw(Point p, Pixel col)
		{
			if (drawTarget == null)
				return;

			void MakePixel(int a, int b, Pixel pix)
			{
				if (a >= 0 && a < drawTarget.Width && b >= 0 && b < drawTarget.Height)
					drawTarget[a, b] = pix;
			}

			switch (PixelMode)
			{
				case Pixel.Mode.Normal:
					MakePixel(p.X, p.Y, col);
					break;
				case Pixel.Mode.Mask:
					if (col.A == 255)
						MakePixel(p.X, p.Y, col);
					break;
				case Pixel.Mode.Alpha:
					Pixel d = drawTarget[p.X, p.Y];
					float a = col.A / 255.0f * PixelBlend;
					float c = 1.0f - a;
					float r = a * col.R + c * d.R;
					float g = a * col.G + c * d.G;
					float b = a * col.B + c * d.B;
					Pixel pix = new Pixel((byte)r, (byte)g, (byte)b);
					MakePixel(p.X, p.Y, pix);
					break;
				case Pixel.Mode.Custom:
					MakePixel(p.X, p.Y, Shader.Calculate(p.X, p.Y, drawTarget[p.X, p.Y], col));
					break;
			}
		}
		public void DrawLine(Point p1, Point p2, Pixel col)
		{
			int x, y, dx, dy, dx1, dy1, px, py, xe, ye, i;
			dx = p2.X - p1.X; dy = p2.Y - p1.Y;
			dx1 = Math.Abs(dx); dy1 = Math.Abs(dy);
			px = 2 * dy1 - dx1; py = 2 * dx1 - dy1;
			if (dy1 <= dx1)
			{
				if (dx >= 0)
				{
					x = p1.X; y = p1.Y; xe = p2.X;
				}
				else
				{
					x = p2.X; y = p2.Y; xe = p1.X;
				}

				Draw(x, y, col);

				for (i = 0; x < xe; i++)
				{
					x = x + 1;
					if (px < 0)
						px = px + 2 * dy1;
					else
					{
						if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0)) y = y + 1; else y = y - 1;
						px = px + 2 * (dy1 - dx1);
					}
					Draw(x, y, col);
				}
			}
			else
			{
				if (dy >= 0)
				{
					x = p1.X; y = p1.Y; ye = p2.Y;
				}
				else
				{
					x = p2.X; y = p2.Y; ye = p1.Y;
				}

				Draw(x, y, col);

				for (i = 0; y < ye; i++)
				{
					y = y + 1;
					if (py <= 0)
						py = py + 2 * dx1;
					else
					{
						if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0)) x = x + 1; else x = x - 1;
						py = py + 2 * (dx1 - dy1);
					}
					Draw(x, y, col);
				}
			}
		}
		public void DrawCircle(Point p, int radius, Pixel col)
		{
			int x0 = 0;
			int y0 = radius;
			int d = 3 - 2 * radius;

			if (radius == 0)
				return;

			while (y0 >= x0)
			{
				Draw(new Point(p.X - x0, p.Y - y0), col);
				Draw(new Point(p.X - y0, p.Y - x0), col);
				Draw(new Point(p.X + y0, p.Y - x0), col);
				Draw(new Point(p.X + x0, p.Y - y0), col);
				Draw(new Point(p.X - x0, p.Y + y0), col);
				Draw(new Point(p.X - y0, p.Y + x0), col);
				Draw(new Point(p.X + y0, p.Y + x0), col);
				Draw(new Point(p.X + x0, p.Y + y0), col);

				if (d < 0)
					d += 4 * x0++ + 6;
				else
					d += 4 * (x0++ - y0--) + 10;
			}
		}
		public void FillCircle(Point p, int radius, Pixel col)
		{
			int x0 = 0;
			int y0 = radius;
			int d = 3 - 2 * radius;

			if (radius == 0)
				return;

			void MakeLine(int sx, int ex, int ny)
			{
				for (int i = sx; i <= ex; i++)
					Draw(new Point(i, ny), col);
			}

			while (y0 >= x0)
			{
				MakeLine(p.X - x0, p.X + x0, p.Y - y0);
				MakeLine(p.X - y0, p.X + y0, p.Y - x0);
				MakeLine(p.X - x0, p.X + x0, p.Y + y0);
				MakeLine(p.X - y0, p.X + y0, p.Y + x0);

				if (d < 0)
					d += 4 * x0++ + 6;
				else
					d += 4 * (x0++ - y0--) + 10;
			}
		}
		public void DrawRect(Point p, int w, int h, Pixel col)
		{
			DrawLine(new Point(p.X, p.Y), new Point(p.X + w, p.Y), col);
			DrawLine(new Point(p.X + w, p.Y), new Point(p.X + w, p.Y + h), col);
			DrawLine(new Point(p.X + w, p.Y + h), new Point(p.X, p.Y + h), col);
			DrawLine(new Point(p.X, p.Y + h), new Point(p.X, p.Y), col);
		}
		public void DrawRect(Point p1, Point p2, Pixel col)
		{
			if (p1.X > p2.X && p1.Y > p2.Y)
			{
				Point temp = p1;
				p1 = p2;
				p2 = temp;
			}

			DrawRect(p1, Math.Abs(p2.X - p1.X - 1), Math.Abs(p2.Y - p1.Y - 1), col);
		}
		public void FillRect(Point p, int w, int h, Pixel col)
		{
			int Clip(int val, int min, int max)
			{
				if (val < min)
					val = min;
				if (val > max)
					val = max;
				return val;
			}

			int x2 = p.X + w;
			int y2 = p.Y + h;

			Clip(p.X, 0, ScreenWidth);
			Clip(p.Y, 0, ScreenHeight);

			Clip(x2, 0, ScreenWidth);
			Clip(y2, 0, ScreenHeight);

			for (int i = p.X; i < x2; i++)
				for (int j = p.Y; j < y2; j++)
					Draw(i, j, col);
		}
		public void FillRect(Point p1, Point p2, Pixel col)
		{
			if (p1.X > p2.X && p1.Y > p2.Y)
			{
				Point temp = p1;
				p1 = p2;
				p2 = temp;
			}

			FillRect(p1, Math.Abs(p2.X - p1.X - 1), Math.Abs(p2.Y - p1.Y - 1), col);
		}
		public void DrawTriangle(Point p1, Point p2, Point p3, Pixel col)
		{
			DrawLine(p1, p2, col);
			DrawLine(p2, p3, col);
			DrawLine(p3, p1, col);
		}
		public void FillTriangle(Point p1, Point p2, Point p3, Pixel col)
		{
			void Swap(ref int a, ref int b) { int t = a; a = b; b = t; }
			void MakeLine(int sx, int ex, int ny) { for (int i = sx; i <= ex; i++) Draw(i, ny, col); }

			int x1 = p1.X, y1 = p1.Y;
			int x2 = p2.X, y2 = p2.Y;
			int x3 = p3.X, y3 = p3.Y;

			int t1x, t2x, y, minx, maxx, t1xp, t2xp;
			bool changed1 = false;
			bool changed2 = false;
			int signx1, signx2, dx1, dy1, dx2, dy2;
			int e1, e2;
			// Sort vertices
			if (y1 > y2) { Swap(ref y1, ref y2); Swap(ref x1, ref x2); }
			if (y1 > y3) { Swap(ref y1, ref y3); Swap(ref x1, ref x3); }
			if (y2 > y3) { Swap(ref y2, ref y3); Swap(ref x2, ref x3); }

			t1x = t2x = x1; y = y1;   // Starting points
			dx1 = x2 - x1; if (dx1 < 0) { dx1 = -dx1; signx1 = -1; }
			else signx1 = 1;
			dy1 = y2 - y1;

			dx2 = x3 - x1; if (dx2 < 0) { dx2 = -dx2; signx2 = -1; }
			else signx2 = 1;
			dy2 = y3 - y1;

			if (dy1 > dx1)
			{   // swap values
				Swap(ref dx1, ref dy1);
				changed1 = true;
			}
			if (dy2 > dx2)
			{   // swap values
				Swap(ref dy2, ref dx2);
				changed2 = true;
			}

			e2 = dx2 >> 1;
			// Flat top, just process the second half
			if (y1 == y2) goto next;
			e1 = dx1 >> 1;

			for (int i = 0; i < dx1;)
			{
				t1xp = 0; t2xp = 0;
				if (t1x < t2x) { minx = t1x; maxx = t2x; }
				else { minx = t2x; maxx = t1x; }
				// process first line until y value is about to change
				while (i < dx1)
				{
					i++;
					e1 += dy1;
					while (e1 >= dx1)
					{
						e1 -= dx1;
						if (changed1) t1xp = signx1;//t1x += signx1;
						else goto next1;
					}
					if (changed1) break;
					else t1x += signx1;
				}
				// Move line
				next1:
				// process second line until y value is about to change
				while (true)
				{
					e2 += dy2;
					while (e2 >= dx2)
					{
						e2 -= dx2;
						if (changed2) t2xp = signx2;//t2x += signx2;
						else goto next2;
					}
					if (changed2) break;
					else t2x += signx2;
				}
				next2:
				if (minx > t1x) minx = t1x; if (minx > t2x) minx = t2x;
				if (maxx < t1x) maxx = t1x; if (maxx < t2x) maxx = t2x;
				MakeLine(minx, maxx, y);    // Draw line from min to max points found on the y
											// Now increase y
				if (!changed1) t1x += signx1;
				t1x += t1xp;
				if (!changed2) t2x += signx2;
				t2x += t2xp;
				y += 1;
				if (y == y2) break;

			}
			next:
			// Second half
			dx1 = x3 - x2; if (dx1 < 0) { dx1 = -dx1; signx1 = -1; }
			else signx1 = 1;
			dy1 = y3 - y2;
			t1x = x2;

			if (dy1 > dx1)
			{   // swap values
				Swap(ref dy1, ref dx1);
				changed1 = true;
			}
			else changed1 = false;

			e1 = dx1 >> 1;

			for (int i = 0; i <= dx1; i++)
			{
				t1xp = 0; t2xp = 0;
				if (t1x < t2x) { minx = t1x; maxx = t2x; }
				else { minx = t2x; maxx = t1x; }
				// process first line until y value is about to change
				while (i < dx1)
				{
					e1 += dy1;
					while (e1 >= dx1)
					{
						e1 -= dx1;
						if (changed1) { t1xp = signx1; break; }//t1x += signx1;
						else goto next3;
					}
					if (changed1) break;
					else t1x += signx1;
					if (i < dx1) i++;
				}
				next3:
				// process second line until y value is about to change
				while (t2x != x3)
				{
					e2 += dy2;
					while (e2 >= dx2)
					{
						e2 -= dx2;
						if (changed2) t2xp = signx2;
						else goto next4;
					}
					if (changed2) break;
					else t2x += signx2;
				}
				next4:

				if (minx > t1x) minx = t1x; if (minx > t2x) minx = t2x;
				if (maxx < t1x) maxx = t1x; if (maxx < t2x) maxx = t2x;
				MakeLine(minx, maxx, y);
				if (!changed1) t1x += signx1;
				t1x += t1xp;
				if (!changed2) t2x += signx2;
				t2x += t2xp;
				y += 1;
				if (y > y3) return;
			}
		}
		public void DrawPolygon(Point[] verts, Pixel col)
		{
			for (int i = 0; i < verts.Length - 1; i++)
				DrawLine(verts[i], verts[i + 1], col);
			DrawLine(verts[verts.Length - 1], verts[0], col);
		}
		public void FillPolygon(Point[] verts, Pixel col)
		{
			for (int i = 1; i < verts.Length - 1; i++)
				FillTriangle(verts[0], verts[i], verts[i + 1], col);
		}
		public void DrawPath(Point[] points, Pixel col)
		{
			for (int i = 0; i < points.Length - 1; i++)
				DrawLine(points[i], points[i + 1], col);
		}
		public void DrawSprite(Point p, Sprite spr)
		{
			if (spr == null)
				return;

			for (int i = 0; i < spr.Width; i++)
				for (int j = 0; j < spr.Height; j++)
					Draw(p.X + i, p.Y + j, spr[i, j]);
		}
		public void DrawPartialSprite(Point p, Sprite spr, Point op, int w, int h)
		{
			if (spr == null)
				return;

			for (int i = 0; i < w; i++)
				for (int j = 0; j < h; j++)
					Draw(p.X + i, p.Y + j, spr[i + op.X, j + op.Y]);
		}
		public void Clear(Pixel p)
		{
			Pixel[] pixels = drawTarget.GetData();
			for (int i = 0; i < pixels.Length; i++)
				pixels[i] = p;

			if (hrText)
			{
				pixels = textTarget.GetData();
				for (int i = 0; i < pixels.Length; i++)
					pixels[i] = Pixel.Empty;
			}
		}
		#endregion

		#region Subsystems
		public enum Subsystem
		{
			Fullscreen,
			Audio,
			HrText
		}
		public void Enable(Subsystem subsystem)
		{
			switch (subsystem)
			{
				case Subsystem.Fullscreen:
					MonitorInfo mi = new MonitorInfo();
					mi.Size = Marshal.SizeOf(mi);

					if (GetMonitorInfo(MonitorFromWindow(Handle, MonitorDefaultNearest), ref mi))
					{
						int style = GetWindowLong(Handle, (int)WindowLongs.STYLE);

						SetWindowLong(Handle, (int)WindowLongs.STYLE, style & ~(int)WindowStyles.OverlappedWindow);

						SetWindowPos(Handle, WindowTop,
									 mi.Monitor.Left, mi.Monitor.Top,
									 mi.Monitor.Right - mi.Monitor.Left,
									 mi.Monitor.Bottom - mi.Monitor.Top,
									 (uint)(SWP.NoOwnerZOrder | SWP.FrameChanged));
					}

					GetClientRect(Handle, out Rect r);
					ClientRect = r;

					windowWidth = r.Right - r.Left;
					windowHeight = r.Bottom - r.Top;

					ScreenWidth = windowWidth / PixWidth;
					ScreenHeight = windowHeight / PixHeight;

					HandleDrawTarget();
					break;

				case Subsystem.Audio:
					if (audio == null)
						audio = new AudioEngine
						{
							OnSoundCreate = this.OnSoundCreate,
							OnSoundFilter = this.OnSoundFilter
						};
					audio.CreateAudio();
					break;

				case Subsystem.HrText:
					hrText = true;
					textTarget = new Sprite(0, 0);
					break;
			}
		}

		#region Text
		public void DrawText(Point p, string text, Pixel col, int scale = 1)
		{
			if (string.IsNullOrWhiteSpace(text))
				return;

			Pixel.Mode prev = PixelMode;
			if (PixelMode != Pixel.Mode.Custom)
			{
				if (col.A != 255)
					PixelMode = Pixel.Mode.Alpha;
				else
					PixelMode = Pixel.Mode.Mask;
			}

			int sx = 0;
			int sy = 0;

			foreach (char c in text)
			{
				if (c == '\n')
				{
					sx = 0;
					sy += 8 * scale;
				}
				else
				{
					int ox = (c - 32) % 16;
					int oy = (c - 32) / 16;

					if (scale > 1)
					{
						for (int i = 0; i < 8; i++)
							for (int j = 0; j < 8; j++)
								if (fontSprite[i + ox * 8, j + oy * 8].R > 0)
									for (int ni = 0; ni < scale; ni++)
										for (int nj = 0; nj < scale; nj++)
											Draw(p.X + sx + i * scale + ni, p.Y + sy + j * scale + nj, col);
					}
					else
					{
						for (int i = 0; i < 8; i++)
							for (int j = 0; j < 8; j++)
								if (fontSprite[i + ox * 8, j + oy * 8].R > 0)
									Draw(p.X + sx + i, p.Y + sy + j, col);
					}
					sx += 8 * scale;
				}
			}

			if(PixelMode != Pixel.Mode.Custom)
				PixelMode = prev;
		}
		public void DrawTextHr(Point p, string text, Pixel col, int scale = 1)
		{
			const int TargetSizeStep = 25;

			if (!hrText || string.IsNullOrWhiteSpace(text))
				return;

			void SetPixel(int i, int j)
			{
				if (i > textTarget.Width && i < windowWidth)
				{
					Sprite temp = new Sprite(Math.Min(textTarget.Width + TargetSizeStep, windowWidth), textTarget.Height);

					for (int y = 0; y < textTarget.Height; y++)
					{
						int index = y * textTarget.Width;
						int indexTemp = y * temp.Width;
						Array.Copy(textTarget.GetData(), index, temp.GetData(), indexTemp, textTarget.Width);
					}

					textTarget = temp;
				}

				if (j > textTarget.Height && j < windowHeight)
				{
					Sprite temp = new Sprite(textTarget.Width, Math.Min(textTarget.Height + TargetSizeStep, windowHeight));

					for (int y = 0; y < textTarget.Height; y++)
					{
						int index = y * textTarget.Width;
						int indexTemp = y * temp.Width;
						Array.Copy(textTarget.GetData(), index, temp.GetData(), indexTemp, textTarget.Width);
					}

					textTarget = temp;
				}

				switch (PixelMode)
				{
					case Pixel.Mode.Normal:
						if (i >= 0 && i < textTarget.Width && j >= 0 && j < textTarget.Height)
							textTarget[i, j] = col;
						break;
					case Pixel.Mode.Mask:
						if (col.A == 255)
							if (i >= 0 && i < textTarget.Width && j >= 0 && j < textTarget.Height)
								textTarget[i, j] = col;
						break;
					case Pixel.Mode.Alpha:
						Pixel d = drawTarget[p.X, p.Y];
						float a = col.A / 255.0f * PixelBlend;
						float c = 1.0f - a;
						float r = a * col.R + c * d.R;
						float g = a * col.G + c * d.G;
						float b = a * col.B + c * d.B;
						Pixel pix = new Pixel((byte)r, (byte)g, (byte)b);
						if (i >= 0 && i < textTarget.Width && j >= 0 && j < textTarget.Height)
							textTarget[i, j] = col;
						break;
				}
			}

			int sx = 0;
			int sy = 0;

			foreach (char c in text)
			{
				if (c == '\n')
				{
					sx = 0;
					sy += 8 * scale;
				}
				else
				{
					int ox = (c - 32) % 16;
					int oy = (c - 32) / 16;

					if (scale > 1)
					{
						for (int i = 0; i < 8; i++)
							for (int j = 0; j < 8; j++)
								if (fontSprite[i + ox * 8, j + oy * 8].R > 0)
									for (int ni = 0; ni < scale; ni++)
										for (int nj = 0; nj < scale; nj++)
											SetPixel(p.X + sx + i * scale + ni, p.Y + sy + j * scale + nj);
					}
					else
					{
						for (int i = 0; i < 8; i++)
							for (int j = 0; j < 8; j++)
								if (fontSprite[i + ox * 8, j + oy * 8].R > 0)
									SetPixel(p.X + sx + i, p.Y + sy + j);
					}
					sx += 8 * scale;
				}
			}
		}
		#endregion

		#region Audio
		public virtual float OnSoundCreate(int channels, float globalTime, float timeStep) => 0;
		public virtual float OnSoundFilter(int channels, float globalTime, float sample) => sample;

		public Sound LoadSound(string path)
		{
			if (audio != null)
				return audio.LoadSound(path);
			return null;
		}
		public void PlaySound(Sound s)
		{
			if (audio != null)
				audio.PlaySound(s);
		}
		public void StopSound(Sound s)
		{
			if (audio != null)
				audio.StopSound(s);
		}
		#endregion
		#endregion

		#region Functionality
		public virtual void OnCreate() { }
		public virtual void OnUpdate(float elapsed) { }
		public virtual void OnMousePress(Mouse m) { }
		public virtual void OnMouseRelease(Mouse m) { }
		public virtual void OnMouseDown(Mouse m) { }
		public virtual void OnMouseScroll() { }
		public virtual void OnKeyPress(Key k) { }
		public virtual void OnKeyRelease(Key k) { }
		public virtual void OnKeyDown(Key k) { }
		public virtual void OnDestroy() { }
		#endregion
	}
}