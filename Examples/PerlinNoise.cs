using System;
using PixelEngine;

namespace Examples
{
	public class PerlinNoise : Game
	{
		// Store a map sprite to render offscreen
		private Sprite map;

		// Change the hue
		private float hueDivisor = 1;

		// Set the title
		public PerlinNoise() => AppName = "Perlin Noise";

		static void Main(string[] args)
		{
			PerlinNoise nt = new PerlinNoise();
			nt.Construct(500, 350, 2, 2);
			nt.Start();
		}

		public override void OnCreate()
		{
			// Init the map
			map = new Sprite(ScreenWidth, ScreenHeight);
			// Calculate noise
			RecalculateMap();
		}
		
		public override void OnUpdate(TimeSpan elapsed)
		{
			// Adjust hue divisor based on input
			if (GetKey(Key.Up).Down)
				hueDivisor += 0.1f;
			if (GetKey(Key.Down).Down)
				hueDivisor -= 0.1f;

			// Prepare a new map
			if (GetKey(Key.Enter).Pressed)
			{
				// Set a random seed to noise
				Noise.Seed();
				RecalculateMap();
			}

			// Round and clamp hueDivisor
			hueDivisor = Round(hueDivisor, 1);
			hueDivisor = hueDivisor > 0 ? hueDivisor : 0.1f;
			
			// Reset draw target
			DrawTarget = null;
			// Draw the map
			DrawSprite(new Point(0, 0), map);
			// Inform user of Hue Mode
			DrawText(new Point(0, 0), "Hue Mode: " + hueDivisor, Pixel.Presets.Black);
		}

		// Prepare a map
		private void RecalculateMap()
		{
			// Switch to the map to draw
			DrawTarget = map;
			
			for (int i = 0; i < ScreenWidth; i++)
			{
				for (int j = 0; j < ScreenHeight; j++)
				{
					// Calc noise for each pixel
					float noise = Noise.Calculate((float)i / ScreenWidth, (float)j / ScreenHeight, 0f, 5, 1.5f);
					// Map noise to hue
					float f = Map(noise, -1, 1, 0, 255);
					// Convert hue to rgb
					// Divide by hueDivisor to get different hues
					Pixel p = HSVToRGB(f / hueDivisor, 1, 1);
					// Draw the pixel
					Draw(i, j, p);
				}
			}
		}

		// Algorithm to convert HSV to RGB
		private Pixel HSVToRGB(float h, float s, float v)
		{
			float c = v * s;
			float hPrime = (h / 60) % 6;
			float x = c * (1 - Math.Abs(hPrime % 2 - 1));
			float m = v - c;

			float r, g, b;

			if (0 <= hPrime && hPrime < 1)
			{
				r = c;
				g = x;
				b = 0;
			}
			else if (1 <= hPrime && hPrime < 2)
			{
				r = x;
				g = c;
				b = 0;
			}
			else if (2 <= hPrime && hPrime < 3)
			{
				r = 0;
				g = c;
				b = x;
			}
			else if (3 <= hPrime && hPrime < 4)
			{
				r = 0;
				g = x;
				b = c;
			}
			else if (4 <= hPrime && hPrime < 5)
			{
				r = x;
				g = 0;
				b = c;
			}
			else if (5 <= hPrime && hPrime < 6)
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
	}
}