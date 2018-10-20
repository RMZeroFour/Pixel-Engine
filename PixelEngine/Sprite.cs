using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace PixelEngine
{
	public class Sprite
	{
		public Sprite(int w, int h)
		{
			Width = w;
			Height = h;

			colorData = new Pixel[Width * Height];
		}

		public Sprite(Bitmap bmp) => LoadFromBitmap(bmp);

		private void LoadFromBitmap(Bitmap bmp)
		{
			Width = bmp.Width;
			Height = bmp.Height;

			colorData = new Pixel[Width * Height];

			for (int i = 0; i < Width; i++)
			{
				for (int j = 0; j < Height; j++)
				{
					Color c = bmp.GetPixel(i, j);
					this[i, j] = new Pixel(c.R, c.G, c.B, c.A);
				}
			}
		}

		public static Sprite Load(string path)
		{
			if (!File.Exists(path))
				return new Sprite(8, 8);

			using (Bitmap bmp = (Bitmap)Image.FromFile(path))
				return new Sprite(bmp);
		}
		public static void Save(Sprite spr, string path)
		{
			using (Bitmap bmp = new Bitmap(spr.Width, spr.Height))
			{
				for (int i = 0; i < spr.Width; i++)
				{
					for (int j = 0; j < spr.Height; j++)
					{
						Pixel p = spr[i, j];
						bmp.SetPixel(i, j, Color.FromArgb(p.A, p.R, p.G, p.B));
					}
				}

				bmp.Save(path);
			}
		}
		public static void Copy(Sprite src, Sprite dest)
		{
			if (src.colorData.Length != dest.colorData.Length)
				return;

			src.colorData.CopyTo(dest.colorData, 0);
		}

		public int Width { get; private set; }
		public int Height { get; private set; }

		public Pixel this[int x, int y]
		{
			get => GetPixel(x, y);
			set => SetPixel(x, y, value);
		}

		private Pixel GetPixel(int x, int y)
		{
			if (x >= 0 && x < Width && y >= 0 && y < Height)
				return colorData[y * Width + x];
			else
				return Pixel.Empty;
		}
		private void SetPixel(int x, int y, Pixel p)
		{
			if (x >= 0 && x < Width && y >= 0 && y < Height)
				colorData[y * Width + x] = p;
		}
		
		internal Pixel[] GetData() => colorData;
		private Pixel[] colorData = null;
	}
}