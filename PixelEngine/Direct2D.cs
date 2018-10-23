using System;
using System.Collections.Generic;
using Microsoft.WindowsAPICodePack.DirectX.Direct2D1;
using Microsoft.WindowsAPICodePack.DirectX.DirectWrite;
using Microsoft.WindowsAPICodePack.DirectX.WindowsImagingComponent;
using DX = Microsoft.WindowsAPICodePack.DirectX;

namespace PixelEngine
{
	public class Direct2D : IDisposable
	{
		private const float PixMult = 1f / byte.MaxValue;

		private int pixWidth;
		private int pixHeight;

		private Rect rc;

		private Dictionary<Pixel, Brush> presetBrushes;

		private D2DFactory factory;
		private HwndRenderTarget target;

		internal void Init(Display display)
		{
			rc = new Rect(display.ClientRect.Left, display.ClientRect.Top, display.ClientRect.Right, display.ClientRect.Bottom);

			pixWidth = display.PixWidth;
			pixHeight = display.PixHeight;

			if (target != null)
				target.Dispose();
			if (factory != null)
				factory.Dispose();

			factory = D2DFactory.CreateFactory(D2DFactoryType.Multithreaded);

			SizeU size = new SizeU((uint)(rc.Right - rc.Left), (uint)(rc.Bottom - rc.Top));

			RenderTargetProperties props = new RenderTargetProperties() { RenderTargetType = RenderTargetType.Hardware };
			HwndRenderTargetProperties hwndProps = new HwndRenderTargetProperties(display.Handle, size, PresentOptions.Immediately);

			target = factory.CreateHwndRenderTarget(props, hwndProps);

			presetBrushes = new Dictionary<Pixel, Brush>();
			Pixel.Presets[] pixels = (Pixel.Presets[])Enum.GetValues(typeof(Pixel.Presets));
			foreach (Pixel.Presets p in pixels)
			{
				Brush b = target.CreateSolidColorBrush(Convert(p));
				presetBrushes.Add(p, b);
			}
		}

		public void Begin() => target.BeginDraw();
		public void Draw(int x, int y, Pixel col)
		{
			int left = rc.Left + x * pixWidth;
			int right = left + pixWidth;
			int top = rc.Top + y * pixHeight;
			int bottom = top + pixHeight;
			RectF pix = new RectF(left, top, right, bottom);

			if (presetBrushes.ContainsKey(col))
			{
				Brush b = presetBrushes[col];
				target.FillRectangle(pix, b);
			}
			else
			{
				Brush b = target.CreateSolidColorBrush(Convert(col));
				target.FillRectangle(pix, b);
				b.Dispose();
			}
		}
		public void End() => target.TryEndDraw(out Tags _, out DX.ErrorCode _);

		private static ColorF Convert(Pixel p) => new ColorF(p.R * PixMult, p.G * PixMult, p.B * PixMult, p.A * PixMult);

		#region IDisposable Support
		private bool disposedValue = false;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					foreach (KeyValuePair<Pixel, Brush> pair in presetBrushes)
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