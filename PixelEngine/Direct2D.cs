using System;
using System.Collections.Generic;

using Microsoft.WindowsAPICodePack.DirectX.Direct2D1;
using Microsoft.WindowsAPICodePack.DirectX.DirectWrite;
using DX = Microsoft.WindowsAPICodePack.DirectX;

namespace PixelEngine
{
	public class Direct2D : IDisposable
	{
		private int pixWidth;
		private int pixHeight;

		private Rect rc;

		private D2DFactory factory;
		private HwndRenderTarget target;

		private static readonly Dictionary<Pixel, Brush> brushes = new Dictionary<Pixel, Brush>();

		internal void Init(Display display)
		{
			rc = new Rect(display.ClientRect.Left, display.ClientRect.Top, display.ClientRect.Right, display.ClientRect.Bottom);

			pixWidth = display.PixWidth;
			pixHeight = display.PixHeight;

			factory = D2DFactory.CreateFactory(D2DFactoryType.Multithreaded);

			RenderTargetProperties props = new RenderTargetProperties();
			HwndRenderTargetProperties hProps = new HwndRenderTargetProperties(display.Handle,
				new SizeU((uint)(rc.Right - rc.Left), (uint)(rc.Bottom - rc.Top)), PresentOptions.Immediately);

			target = factory.CreateHwndRenderTarget(props, hProps);
		}

		public void Begin() => target.BeginDraw();
		public void Draw(int x, int y, Pixel col)
		{
			int left = rc.Left + x * pixWidth;
			int right = left + pixWidth;
			int top = rc.Top + y * pixHeight;
			int bottom = top + pixHeight;
			RectF pix = new RectF(left, top, right, bottom);
			
			Brush b;
			if (brushes.ContainsKey(col))
			{
				b = brushes[col];
			}
			else
			{
				b = target.CreateSolidColorBrush(Convert(col));
				brushes.Add(col, b);
			}
			target.FillRectangle(pix, b);
		}
		public void End() => target.TryEndDraw(out Tags _, out DX.ErrorCode _);

		#region Helpers
		private static ColorF Convert(Pixel p) => new ColorF(p.R / 255f, p.G / 255f, p.B / 255f, p.A / 255f);
		#endregion

		#region IDisposable Support
		private bool disposedValue = false;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					foreach (KeyValuePair<Pixel, Brush> pair in brushes)
						pair.Value.Dispose();

					target.Dispose();
					factory.Dispose();
				}

				disposedValue = true;
			}
		}

		public void Dispose() => Dispose(true);
		#endregion
	}
}
