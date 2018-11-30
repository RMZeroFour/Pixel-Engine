using System;
using System.Collections.Generic;
using System.Linq;

namespace PixelEngine
{
	public struct Pixel
	{
		public byte R { get; private set; }
		public byte G { get; private set; }
		public byte B { get; private set; }
		public byte A { get; private set; }

		public Pixel(byte red, byte green, byte blue, byte alpha = 255)
		{
			R = red;
			G = green;
			B = blue;
			A = alpha;
		}

		public enum Mode
		{
			Normal,
			Mask,
			Alpha,
			Custom
		}

		public static Pixel Random()
		{
			byte[] vals = Randoms.RandomBytes(3);
			return new Pixel(vals[0], vals[1], vals[2]);
		}
		public static Pixel RandomAlpha()
		{
			byte[] vals = Randoms.RandomBytes(4);
			return new Pixel(vals[0], vals[1], vals[2], vals[3]);
		}

		#region Presets
		public enum Presets : uint
		{
			White = 0xffffff,
			Grey = 0xa9a9a9,
			Red = 0xff0000,
			Yellow = 0xffff00,
			Green = 0x00ff00,
			Cyan = 0x00ffff,
			Blue = 0x0000ff,
			Magenta = 0xff00ff,
			Brown = 0x9a6324,
			Orange = 0xf58231,
			Purple = 0x911eb4,
			Lime = 0xbfef45,
			Pink = 0xfabebe,
			Snow = 0xFFFAFA,
			Teal = 0x469990,
			Lavender = 0xe6beff,
			Beige = 0xfffac8,
			Maroon = 0x800000,
			Mint = 0xaaffc3,
			Olive = 0x808000,
			Apricot = 0xffd8b1,
			Navy = 0x000075,
			Black = 0x000000,
			DarkGrey = 0x8B8B8B,
			DarkRed = 0x8B0000,
			DarkYellow = 0x8B8B00,
			DarkGreen = 0x008B00,
			DarkCyan = 0x008B8B,
			DarkBlue = 0x00008B,
			DarkMagenta = 0x8B008B
		}

		public static readonly Pixel Empty = new Pixel(0, 0, 0, 0);

		private static Dictionary<Presets, Pixel> presetPixels;
		public static Pixel[] PresetPixels => presetPixels.Values.ToArray();
		#endregion

		public static Pixel FromRgb(uint rgb)
		{
			byte a = (byte)(rgb & 0xFF);
			byte b = (byte)((rgb >> 8) & 0xFF);
			byte g = (byte)((rgb >> 16) & 0xFF);
			byte r = (byte)((rgb >> 24) & 0xFF);

			return new Pixel(r, g, b, a);
		}
		public static Pixel FromHsv(float h, float s, float v)
		{
			float c = v * s;
			float nh = (h / 60) % 6;
			float x = c * (1 - Math.Abs(nh % 2 - 1));
			float m = v - c;

			float r, g, b;

			if (0 <= nh && nh < 1)
			{
				r = c;
				g = x;
				b = 0;
			}
			else if (1 <= nh && nh < 2)
			{
				r = x;
				g = c;
				b = 0;
			}
			else if (2 <= nh && nh < 3)
			{
				r = 0;
				g = c;
				b = x;
			}
			else if (3 <= nh && nh < 4)
			{
				r = 0;
				g = x;
				b = c;
			}
			else if (4 <= nh && nh < 5)
			{
				r = x;
				g = 0;
				b = c;
			}
			else if (5 <= nh && nh < 6)
			{
				r = c;
				g = 0;
				b = x;
			}
			else
			{
				r = 0;
				g = 0;
				b = 0;
			}

			r += m;
			g += m;
			b += m;

			return new Pixel((byte)Math.Floor(r * 255), (byte)Math.Floor(g * 255), (byte)Math.Floor(b * 255));
		}

		static Pixel()
		{
			Pixel ToPixel(Presets p)
			{
				string hex = p.ToString("X");

				byte r = (byte)Convert.ToUInt32(hex.Substring(2, 2), 16);
				byte g = (byte)Convert.ToUInt32(hex.Substring(4, 2), 16);
				byte b = (byte)Convert.ToUInt32(hex.Substring(6, 2), 16);

				return new Pixel(r, g, b);
			}

			Presets[] presets = (Presets[])Enum.GetValues(typeof(Presets));
			presetPixels = presets.ToDictionary(p => p, p => ToPixel(p));
		}

		public static bool operator ==(Pixel a, Pixel b)
		{
			return (a.R == b.R) && (a.G == b.G) && (a.B == b.B) && (a.A == b.A);
		}

		public static bool operator !=(Pixel a, Pixel b) => !(a == b);

		public static implicit operator Pixel(Presets p)
		{
			if (presetPixels.TryGetValue(p, out Pixel pix))
				return pix;
			return Empty;
		}

		public override bool Equals(object obj)
		{
			if (obj is Pixel p)
				return this == p;
			return false;
		}
		public override int GetHashCode()
		{
			int hashCode = 196078;
			hashCode = hashCode * -152113 + R.GetHashCode();
			hashCode = hashCode * -152113 + G.GetHashCode();
			hashCode = hashCode * -152113 + B.GetHashCode();
			hashCode = hashCode * -152113 + A.GetHashCode();
			return hashCode;
		}
	}
}