using PixelEngine;

namespace Examples
{
	public class RandomPixels : Game
	{
		static void Main(string[] args)
		{
			// Create an instance
			RandomPixels rp = new RandomPixels();
			rp.Construct(); // Construct the game
			rp.Start(); // Start and show a window
		}

		// Uncomment to make fullscreen
		//public override void OnCreate() => Enable(Subsystem.Fullscreen);

		public override void OnUpdate(float elapsed)
		{
			// Loop through all the pixels
			for (int i = 0; i < ScreenWidth; i++)
				for (int j = 0; j < ScreenHeight; j++)
					Draw(i, j, Pixel.Random()); // Draw a random pixel
		}
	}
}