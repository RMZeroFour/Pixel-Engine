using System;
using PixelEngine;

namespace Examples
{
	public class Noise : Game
	{
		private Random rnd = new Random();

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
					Draw(i, j, Pixel.Random());
		}
	}
}
