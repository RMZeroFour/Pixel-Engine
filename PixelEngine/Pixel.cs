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
			Alpha
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

		public static readonly Pixel White = new Pixel(255, 255, 255);
		public static readonly Pixel Grey = new Pixel(192, 192, 192);
		public static readonly Pixel Red = new Pixel(255, 0, 0);
		public static readonly Pixel Yellow = new Pixel(255, 255, 0);
		public static readonly Pixel Green = new Pixel(0, 255, 0);
		public static readonly Pixel Cyan = new Pixel(0, 255, 255);
		public static readonly Pixel Blue = new Pixel(0, 0, 255);
		public static readonly Pixel Magenta = new Pixel(255, 0, 255);
		public static readonly Pixel Black = new Pixel(0, 0, 0);
		public static readonly Pixel Empty = new Pixel(0, 0, 0, 0);

		public static bool operator ==(Pixel a, Pixel b) => (a.A == b.A) && (a.R == b.R) && (a.G == b.G) && (a.B == b.B);
		public static bool operator !=(Pixel a, Pixel b) => !(a == b);

		public override bool Equals(object obj)
		{
			if (obj is Pixel p)
				return this == p;
			return false;
		}
		public override int GetHashCode()
		{
			int hashCode = 1960784236;
			hashCode = hashCode * -1521134295 + R.GetHashCode();
			hashCode = hashCode * -1521134295 + G.GetHashCode();
			hashCode = hashCode * -1521134295 + B.GetHashCode();
			hashCode = hashCode * -1521134295 + A.GetHashCode();
			return hashCode;
		}
	}
}