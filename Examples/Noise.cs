using System;
using PixelEngine;

namespace Examples
{
	public class Noise : Game
	{
		private Random rnd = new Random();

		private Pixel[] pixels = { Pixel.White, Pixel.Black, Pixel.Grey, Pixel.Red, Pixel.Green, Pixel.Blue, Pixel.Cyan, Pixel.Magenta, Pixel.Yellow };

		static void Main(string[] args)
		{
			Noise n = new Noise();
			n.Construct(500, 500, 10, 10);
			n.Start();
		}

		public override void OnUpdate(TimeSpan elapsed)
		{
			for (int i = 0; i < ScreenWidth / PixWidth; i++)
				for (int j = 0; j < ScreenHeight / PixHeight; j++)
					Draw(i, j, pixels[rnd.Next(pixels.Length)]);
		}
	}
}
