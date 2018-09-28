using System;

namespace PixelEngine
{
	internal static class MathHelper
	{
		private readonly static Random rnd = new Random();

		public static byte RandomByte(int count) => RandomBytes(1)[0];

		public static byte[] RandomBytes(int count)
		{
			byte[] b = new byte[count];
			rnd.NextBytes(b);
			return b;
		}

		public static float Map(float val, float min, float max, float newMin, float newMax) => ((val - min) / (max - min) * (newMax - newMin)) + newMin;
	}
}
