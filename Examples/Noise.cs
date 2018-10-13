using System;
using PixelEngine;

namespace Examples
{
	public class Noise : Game
	{
		static void Main(string[] args)
		{
			// Create an instance
			Noise n = new Noise();
			n.Construct(500, 500, 10, 10); // Construct the game
			n.Start(); // Start and show a window
		}

		// Uncomment to make fullscreen
		//public override void OnCreate() => Fullscreen();

		public override void OnUpdate(TimeSpan elapsed)
		{
			// Loop through all the pixels
			for (int i = 0; i < ScreenWidth / PixWidth; i++)
				for (int j = 0; j < ScreenHeight / PixHeight; j++)
					Draw(i, j, Pixel.Random()); // Draw a random pixel
		}
	}
}
