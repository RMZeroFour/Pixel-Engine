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

		public unsafe void Initialize(Sprite drawTarget)
		{
			fixed (Pixel* ptr = drawTarget.GetData())
				GlTexImage2D((uint)GL.Texture2D, 0, (uint)GL.RGBA, game.ScreenWidth, game.ScreenHeight,
					0, (uint)GL.RGBA, (uint)GL.UnsignedByte, ptr);
		}

		public void Draw(Sprite drawTarget)
		{
			unsafe
			{
				fixed (Pixel* pix = drawTarget.GetData())
				{
					GlTexSubImage2D((uint)GL.Texture2D, 0, 0, 0,
						game.ScreenWidth, game.ScreenHeight,
						(uint)GL.RGBA, (uint)GL.UnsignedByte, pix);
				}
			}


			GlBegin((uint)GL.Quads);
			GlTexCoord2f(0.0f, 1.0f); GlVertex3f(-1.0f, -1.0f, 0.0f);
			GlTexCoord2f(0.0f, 0.0f); GlVertex3f(-1.0f, 1.0f, 0.0f);
			GlTexCoord2f(1.0f, 0.0f); GlVertex3f(1.0f, 1.0f, 0.0f);
			GlTexCoord2f(1.0f, 1.0f); GlVertex3f(1.0f, -1.0f, 0.0f);
			GlEnd();

			SwapBuffers(deviceContext);
		}

		public void Destroy()
		{
			WglDeleteContext(renderContext);
			ReleaseDC(game.Handle, deviceContext);
		}
	}
}