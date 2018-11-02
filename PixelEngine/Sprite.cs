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
		private static Sprite LoadFromSpr(string path)
		{
			Pixel Parse(short col)
			{
				Pixel color = Pixel.Empty;

				switch (col & 0xF)
				{
					case 0x0: return Pixel.Presets.Black;       // FG_BLACK			
					case 0x1: return Pixel.Presets.DarkBlue;    // FG_DARK_BLUE		
					case 0x2: return Pixel.Presets.DarkGreen;   // FG_DARK_GREEN		
					case 0x3: return Pixel.Presets.DarkCyan;    // FG_DARK_CYAN		
					case 0x4: return Pixel.Presets.DarkRed;     // FG_DARK_RED		
					case 0x5: return Pixel.Presets.DarkMagenta; // FG_DARK_MAGENTA	
					case 0x6: return Pixel.Presets.DarkYellow;  // FG_DARK_YELLOW		
					case 0x7: return Pixel.Presets.Grey;        // FG_GREY			
					case 0x8: return Pixel.Presets.DarkGrey;    // FG_DARK_GREY		
					case 0x9: return Pixel.Presets.Blue;        // FG_BLUE			
					case 0xA: return Pixel.Presets.Green;       // FG_GREEN			
					case 0xB: return Pixel.Presets.Cyan;        // FG_CYAN			
					case 0xC: return Pixel.Presets.Red;         // FG_RED				
					case 0xD: return Pixel.Presets.Magenta;     // FG_MAGENTA			
					case 0xE: return Pixel.Presets.Yellow;      // FG_YELLOW			
					case 0xF: return Pixel.Presets.White;       // FG_WHITE
				}

				return Pixel.Empty;
			}

			Sprite spr;

			using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
			{
				int w = reader.ReadInt32();
				int h = reader.ReadInt32();

				spr = new Sprite(w, h);

				for (int i = 0; i < w; i++)
					for (int j = 0; j < h; j++)
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