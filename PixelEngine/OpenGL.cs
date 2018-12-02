using System;
using System.Runtime.InteropServices;

using static PixelEngine.Windows;

namespace PixelEngine
{
	internal class OpenGL
	{
		private Game game;

		private IntPtr deviceContext;
		private IntPtr renderContext;

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

			ReleaseDC(game.Handle, deviceContext);
		}

		public unsafe void Initialize(Sprite drawTarget, Sprite textTarget)
		{
			GlTexImage2D((uint)GL.Texture2D, 0, (uint)GL.RGBA,
				game.ScreenWidth, game.ScreenHeight,
				0, (uint)GL.RGBA, (uint)GL.UnsignedByte, null);

			ww = 1f / game.windowWidth;
			wh = 1f / game.windowHeight;

			pw = game.PixWidth * ww;
			ph = game.PixHeight * wh;

			SetValues(pw, ph, ww, wh);
			CreateCoords(game.PixWidth, game.PixHeight, game.ScreenWidth, game.ScreenHeight);
		}

		public unsafe void Draw(Sprite drawTarget, Sprite textTarget)
		{
			deviceContext = GetDC(game.Handle);

			fixed (Pixel* ptr = drawTarget.GetData())
			{
				if (game.PixWidth == 1 && game.PixHeight == 1)
					RenderUnitPixels(drawTarget.Width, drawTarget.Height, ptr);
				else
					RenderPixels(drawTarget.Width, drawTarget.Height, ptr);
			}

			if (textTarget != null)
				fixed (Pixel* ptr = textTarget.GetData())
					RenderText(game.windowWidth, game.windowHeight, textTarget.Width, textTarget.Height, ptr);

			SwapBuffers(deviceContext);

			ReleaseDC(game.Handle, deviceContext);
		}

		public void Destroy()
		{
			WglDeleteContext(renderContext);
			DestroyCoords();
		}
	}
}