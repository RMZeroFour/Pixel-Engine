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
		}

		public void Draw(Sprite drawTarget, Sprite textTarget)
		{
			RenderPixels(drawTarget);
			RenderText(textTarget);

			SwapBuffers(deviceContext);
		}

		private void RenderPixels(Sprite drawTarget)
		{
			if (game.PixWidth == 1 && game.PixHeight == 1)
			{
				float nx = 0;
				float ny = 0;

				GlBegin((uint)GL.Points);
				for (int i = 0; i < drawTarget.Width; i++)
				{
					nx += sw;
					ny = 0;

					float px = nx * 2 - 1;

					for (int j = 0; j < drawTarget.Height; j++)
					{
						Pixel p = drawTarget[i, j];

						ny += sh;

						float py = ny * 2 - 1;

						GlColor3ub(p.R, p.G, p.B);
						GlVertex2f(px, -py);
					}
				}
				GlEnd();
			}
			else
			{
				float x1 = -sw;
				float x2 = 0;

				GlBegin((uint)GL.Quads);
				for (int i = 0; i < drawTarget.Width; i++)
				{
					x1 += sw;
					x2 += sw;

					float nx1 = x1 * 2 - 1;
					float nx2 = x2 * 2 - 1;

					float y1 = -sh;
					float y2 = 0;

					for (int j = 0; j < drawTarget.Height; j++)
					{
						Pixel p = drawTarget[i, j];

						y1 += sh;
						y2 += sh;

						float ny1 = y1 * 2 - 1;
						float ny2 = y2 * 2 - 1;

						GlColor3ub(p.R, p.G, p.B);

						GlVertex2f(nx1, -ny1);
						GlVertex2f(nx1, -ny2);
						GlVertex2f(nx2, -ny2);
						GlVertex2f(nx2, -ny1);
					}
				}
				GlEnd();
			}
		}

		private void RenderText(Sprite textTarget)
		{
			if (textTarget == null)
				return;

			GlBegin((uint)GL.Points);

			float nx = 0;
			float ny = 0;

			for (int i = 0; i < textTarget.Width; i++)
			{
				nx += ww;
				ny = 0;
				float kx = nx * 2 - 1;

				for (int j = 0; j < textTarget.Height; j++)
				{
					ny += wh;
					float ky = ny * 2 - 1;

					Pixel p = textTarget[i, j];

					if (p == Pixel.Empty)
						continue;

					GlColor3ub(p.R, p.G, p.B);
					GlVertex2f(kx, -(ky));
				}
			}
			GlEnd();
		}

		public void Destroy()
		{
			WglDeleteContext(renderContext);
			ReleaseDC(game.Handle, deviceContext);
		}
	}
}