using System;

namespace PixelEngine
{
	internal static class Randoms
	{
		private readonly static Random rnd = new Random();

		public static byte RandomByte(int count) => RandomBytes(1)[0];

		public static byte[] RandomBytes(int count)
		{
			byte[] b = new byte[count];
			rnd.NextBytes(b);
			return b;
		}
	}
}
