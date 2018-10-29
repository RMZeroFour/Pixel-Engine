using System;
using static PixelEngine.Windows;

namespace PixelEngine
{
	internal class OpenGL
	{
		private Game game;

		private IntPtr deviceContext;
		private IntPtr renderContext;

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
			fixed (Pixel* ptr = drawTarget.GetData())
				GlTexImage2D((uint)GL.Texture2D, 0, (uint)GL.RGBA, 
					game.ScreenWidth, game.ScreenHeight,
					0, (uint)GL.RGBA, (uint)GL.UnsignedByte, ptr);
		}

		public void Draw(Sprite drawTarget, Sprite textTarget)
		{
			void MakeQuad(Pixel p, float nx, float ny, float ex, float ey)
			{
				GlPushMatrix();
				GlColor4ub(p.R, p.G, p.B, p.A);
				GlVertex2f(nx, -ny);
				GlVertex2f(nx, -ey);
				GlVertex2f(ex, -ey);
				GlVertex2f(ex, -ny);
				GlPopMatrix();
			}

			float sw = 1f / game.ScreenWidth;
			float sh = 1f / game.ScreenHeight;
			float ww = 1f / game.windowWidth;
			float wh = 1f / game.windowHeight;
		
			GlBegin((uint)GL.Quads);
			for (int i = 0; i < drawTarget.Width; i++)
			{
				for (int j = 0; j < drawTarget.Height; j++)
				{
					Pixel p = drawTarget[i, j];

					if (p != Pixel.Empty)
					{
						float nx = i * sw;
						float ny = j * sh;
						float ex = (i + game.PixWidth) * sw;
						float ey = (j + game.PixHeight) * sh;

						nx = nx * 2 - 1;
						ny = ny * 2 - 1;
						ex = ex * 2 - 1;
						ey = ey * 2 - 1;

						MakeQuad(p, nx, ny, ex, ey);
					}
				}
			}
			GlEnd();

			if (textTarget != null)
			{
				GlBegin((uint)GL.Points);
				for (int i = 0; i < textTarget.Width; i++)
				{
					for (int j = 0; j < textTarget.Height; j++)
					{
						Pixel p = textTarget[i, j];

						if (p != Pixel.Empty)
						{
							float nx = i * ww;
							float ny = j * wh;

							nx = nx * 2 - 1;
							ny = ny * 2 - 1;

							GlPushMatrix();
							GlColor4ub(p.R, p.G, p.B, p.A);
							GlVertex2f(nx, -ny);
							GlPopMatrix();
						}
					}
				}
				GlEnd();
			}

			SwapBuffers(deviceContext);
		}

		public void Clear()
		{
			GlClearColor(0, 0, 0, 1);
			GlClear((uint)GL.ColorBufferBit | (uint)GL.DepthBufferBit);
		}

		public void Destroy()
		{
			WglDeleteContext(renderContext);
			ReleaseDC(game.Handle, deviceContext);
		}
	}
}