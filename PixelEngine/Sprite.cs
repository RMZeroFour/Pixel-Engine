using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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

			unsafe
			{
				Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
				BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);

				int* scan0 = (int*)bmpData.Scan0;

				int stride = bmpData.Stride / 4;

				for (int x = 0; x < Width; x++)
				{
					for (int y = 0; y < Height; y++)
					{
						Color c = Color.FromArgb(*(scan0 + x + y * stride));
						this[x, y] = new Pixel(c.R, c.G, c.B, c.A);
					}
				}

				bmp.UnlockBits(bmpData);
			}
		}
		private static Sprite LoadFromSpr(string path)
		{
			Pixel Parse(short col)
			{
				switch (col & 0xF)
				{
					case 0x0: return Pixel.Presets.Black;	
					case 0x1: return Pixel.Presets.DarkBlue;
					case 0x2: return Pixel.Presets.DarkGreen;	
					case 0x3: return Pixel.Presets.DarkCyan;
					case 0x4: return Pixel.Presets.DarkRed; 
					case 0x5: return Pixel.Presets.DarkMagenta;	
					case 0x6: return Pixel.Presets.DarkYellow;	
					case 0x7: return Pixel.Presets.Grey; 
					case 0x8: return Pixel.Presets.DarkGrey;
					case 0x9: return Pixel.Presets.Blue; 
					case 0xA: return Pixel.Presets.Green;
					case 0xB: return Pixel.Presets.Cyan; 
					case 0xC: return Pixel.Presets.Red; 
					case 0xD: return Pixel.Presets.Magenta; 
					case 0xE: return Pixel.Presets.Yellow;
					case 0xF: return Pixel.Presets.White; 
				}

				return Pixel.Empty;
			}

			Sprite spr;

			using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
			{
				int w = reader.ReadInt32();
				int h = reader.ReadInt32();

				spr = new Sprite(w, h);

				for (int i = 0; i < h; i++)
					for (int j = 0; j < w; j++)
						spr[j, i] = Parse(reader.ReadInt16());
			}

			return spr;
		}

		public static Sprite Load(string path)
		{
			if (!File.Exists(path))
				return new Sprite(8, 8);

			if (path.EndsWith(".spr"))
				return LoadFromSpr(path);
			else
				using (Bitmap bmp = (Bitmap)Image.FromFile(path))
					return new Sprite(bmp);
		}
		public static void Save(Sprite spr, string path)
		{
			unsafe
			{
				using (Bitmap bmp = new Bitmap(spr.Width, spr.Height))
				{
					Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
					BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);

					int* scan0 = (int*)bmpData.Scan0;
					int stride = bmpData.Stride / 4;

					for (int x = 0; x < spr.Width; x++)
					{
						for (int y = 0; y < spr.Height; y++)
						{
							Pixel p = spr[x, y];
							Color c = Color.FromArgb(p.A, p.R, p.G, p.B);
							*(scan0 + x + y * stride) = c.ToArgb();
						}
					}

					bmp.UnlockBits(bmpData);
					bmp.Save(path);
				}
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
		
		internal ref Pixel[] GetData() => ref colorData;
		private Pixel[] colorData = null;
	}
}