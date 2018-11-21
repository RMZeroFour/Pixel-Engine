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

		private void LoadFromBitmap(Bitmap bmp)
		{
			Width = bmp.Width;
			Height = bmp.Height;

			colorData = new Pixel[Width * Height];

			unsafe
			{
				Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
				BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);

				byte* scan0 = (byte*)bmpData.Scan0;

				int depth = Image.GetPixelFormatSize(bmp.PixelFormat);

				int length = Width * Height * depth / 8;
				
				for (int x = 0; x < Width; x++)
				{
					for (int y = 0; y < Height; y++)
					{
						int i = ((y * Width) + x) * depth / 8;

						Color c = Color.Empty;

						switch (depth)
						{
							case 32:
								{
									byte b = scan0[i];
									byte g = scan0[i + 1];
									byte r = scan0[i + 2];
									byte a = scan0[i + 3];
									c = Color.FromArgb(a, r, g, b);
									break;
								}

							case 24:
								{
									byte b = scan0[i];
									byte g = scan0[i + 1];
									byte r = scan0[i + 2];
									c = Color.FromArgb(r, g, b);
									break;
								}

							case 8:
								{
									byte b = scan0[i];
									c = Color.FromArgb(b, b, b);
									break;
								}
						}

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

			using (Stream stream = File.OpenRead(path))
			using (BinaryReader reader = new BinaryReader(stream))
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
			{
				return LoadFromSpr(path);
			}
			else
			{
				using (Bitmap bmp = (Bitmap)Image.FromFile(path))
				{
					Sprite spr = new Sprite(0, 0);
					spr.LoadFromBitmap(bmp);
					return spr;
				}
			}
		}
		public static void Save(Sprite spr, string path)
		{
			unsafe
			{
				using (Bitmap bmp = new Bitmap(spr.Width, spr.Height))
				{
					Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
					BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);

					byte* scan0 = (byte*)bmpData.Scan0;

					int depth = Image.GetPixelFormatSize(bmp.PixelFormat);

					int length = spr.Width * spr.Height * depth / 8;

					for (int x = 0; x < spr.Width; x++)
					{
						for (int y = 0; y < spr.Height; y++)
						{
							Pixel p = spr[x, y];

							int i = ((y * spr.Width) + x) * depth / 8;

							scan0[i] = p.B;
							scan0[i + 1] = p.G;
							scan0[i + 2] = p.R;
							scan0[i + 3] = p.A;
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