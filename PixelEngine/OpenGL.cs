using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using static PixelEngine.Windows;

namespace PixelEngine
{
	internal class OpenGL
	{
		private Game game;

		private IntPtr deviceContext;
		private IntPtr renderContext;

		private float sw, sh;
		private float pw, ph;
		private float ww, wh;

		public void Create(Game game)
		{
			this.game = game;

			deviceContext = GetDC(game.Handle);
			PixelFormatDesc pfd = new PixelFormatDesc(1,
				(uint)PFD.DrawToWindow | (uint)PFD.SupportOpenGL | (uint)PFD.DoubleBuffer,
				(byte)PFD.TypeRGBA, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				(sbyte)PFD.MainPlane, 0, 0, 0, 0);

			int pf = ChoosePixelFormat(deviceContext, ref pfd);
			if (pf == 0)
				return;

			SetPixelFormat(deviceContext, pf, ref pfd);

			renderContext = WglCreateContext(deviceContext);
			if (renderContext == IntPtr.Zero)
				return;

			WglMakeCurrent(deviceContext, renderContext);

			uint glBuffer = 0;

			GlEnable((uint)GL.Texture2D);
			GlGenTextures(1, new uint[] { glBuffer });
			GlBindTexture((uint)GL.Texture2D, glBuffer);
			GlTexParameteri((uint)GL.Texture2D, (uint)GL.TextureMagFilter, (int)GL.Nearest);
			GlTexParameteri((uint)GL.Texture2D, (uint)GL.TextureMinFilter, (int)GL.Nearest);
			GlTexEnvf((uint)GL.TextureEnv, (uint)GL.TextureEnvMode, (float)GL.Decal);
		}

		public unsafe void Initialize(Sprite drawTarget, Sprite textTarget)
		{
			GlTexImage2D((uint)GL.Texture2D, 0, (uint)GL.RGBA,
				game.ScreenWidth, game.ScreenHeight,
				0, (uint)GL.RGBA, (uint)GL.UnsignedByte, null);

			IntPtr proc = WglGetProcAddress("wglSwapIntervalEXT");
			SwapInterval si = Marshal.GetDelegateForFunctionPointer<SwapInterval>(proc);
			si?.Invoke(0);

			sw = 1f / game.ScreenWidth;
			sh = 1f / game.ScreenHeight;

			pw = game.PixWidth * sw;
			ph = game.PixHeight * sh;

			ww = 1f / game.windowWidth;
			wh = 1f / game.windowHeight;

			SetValues(pw, ph, sw, sh, ww, wh);
		}

		public unsafe void Draw(Sprite drawTarget, Sprite textTarget)
		{
			if (game.PixWidth == 1 && game.PixHeight == 1)
				fixed (Pixel* ptr = drawTarget.GetData())
					RenderUnitPixels(drawTarget.Width, drawTarget.Height, ptr);
			else
				fixed (Pixel* ptr = drawTarget.GetData())
					RenderPixels(drawTarget.Width, drawTarget.Height, game.PixWidth, game.PixHeight, ptr);

			if (textTarget != null)
				fixed (Pixel* ptr = textTarget.GetData())
					RenderText(textTarget.Width, textTarget.Height, ptr);

			SwapBuffers(deviceContext);
		}

		public void Destroy()
		{
			WglDeleteContext(renderContext);
			ReleaseDC(game.Handle, deviceContext);
		}
	}
}