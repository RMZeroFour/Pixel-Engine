using PixelEngine;
using PixelEngine.Utilities;

namespace Examples
{
	public class PerlinNoise : Game
	{
		private float time;
		private bool dir = true;

		static void Main(string[] args)
		{
			PerlinNoise pa = new PerlinNoise();
			pa.Construct(150, 150, 2, 2);
			pa.Start();
		}

		public override void OnUpdate(float elapsed)
		{
			if (GetKey(Key.Enter).Pressed)
			{
				time = 0;
				dir = true;
				Noise.Seed();
			}

			time += 0.01f * (dir ? 1 : -1);

			if (time <= 0 || time >= 360)
				dir = !dir;

			for (int i = 0; i < ScreenWidth; i++)
			{
				for (int j = 0; j < ScreenHeight; j++)
				{
					float x = (float)i / ScreenWidth;
					float y = (float)j / ScreenWidth;

					float noise = Noise.Calculate(x, y, time);
					noise = noise / 2 + 1;

					Pixel p = Pixel.FromHsv(noise * time * 360, noise, noise * 0.8f);

					Draw(i, j, p);
				}
			}
		}
	}
}