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
			n.Construct(); // Construct the game
			n.Start(); // Start and show a window
		}

		// Uncomment to make fullscreen
		//public override void OnCreate() => Fullscreen();

		public override void OnUpdate(TimeSpan elapsed)
		{
			// Loop through all the pixels
			for (int i = 0; i < ScreenWidth; i++)
				for (int j = 0; j < ScreenHeight; j++)
					Draw(i, j, Pixel.Random()); // Draw a random pixel
		}
	}
}
