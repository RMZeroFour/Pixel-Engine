using System;
using PixelEngine;

namespace Examples
{
	public class Mode7 : Game
	{
		static void Main(string[] args)
		{
			Mode7 spr = new Mode7();
			spr.Construct(500, 300, 2, 2);
			spr.Start();
		}

		public Mode7() => AppName = "Pseudo 3D Planes";

		private float worldX;
		private float worldY;
		private float worldA;
		private float near;
		private float far;
		private float foVHalf;
		private float speed = 1;

		private Sprite sprGround;
		private Sprite sprSky;

		public override void OnCreate()
		{
			sprGround = Sprite.Load("Ground.png");
			sprSky = Sprite.Load("Sky.png");

			Reset();
		}

		public override void OnUpdate(float elapsed)
		{
			Clear(Pixel.Presets.Black);

			elapsed *= 0.025f * speed;

			if (GetKey(Key.Q).Down) near += 0.01f * elapsed;
			if (GetKey(Key.W).Down) near -= 0.01f * elapsed;
			if (GetKey(Key.A).Down) far += 0.01f * elapsed;
			if (GetKey(Key.S).Down) far -= 0.01f * elapsed;
			if (GetKey(Key.Z).Down) foVHalf += 0.01f * elapsed;
			if (GetKey(Key.X).Down) foVHalf -= 0.01f * elapsed;

			if (GetKey(Key.O).Down) speed += 0.1f * elapsed;
			if (GetKey(Key.P).Down) speed -= 0.1f * elapsed;

			if (speed <= 0) speed = 0.1f;

			if (GetKey(Key.R).Pressed) Reset();

			float farX1 = worldX + Cos(worldA - foVHalf) * far;
			float farY1 = worldY + Sin(worldA - foVHalf) * far;

			float nearX1 = worldX + Cos(worldA - foVHalf) * near;
			float nearY1 = worldY + Sin(worldA - foVHalf) * near;

			float farX2 = worldX + Cos(worldA + foVHalf) * far;
			float farY2 = worldY + Sin(worldA + foVHalf) * far;

			float nearX2 = worldX + Cos(worldA + foVHalf) * near;
			float nearY2 = worldY + Sin(worldA + foVHalf) * near;

			for (int y = 0; y < ScreenHeight / 2; y++)
			{
				float sampleDepth = y / (ScreenHeight / 2.0f);

				float startX = (farX1 - nearX1) / (sampleDepth) + nearX1;
				float startY = (farY1 - nearY1) / (sampleDepth) + nearY1;
				float endX = (farX2 - nearX2) / (sampleDepth) + nearX2;
				float endY = (farY2 - nearY2) / (sampleDepth) + nearY2;

				for (int x = 0; x < ScreenWidth; x++)
				{
					float sampleWidth = (float)x / ScreenWidth;
					float sampleX = (endX - startX) * sampleWidth + startX;
					float sampleY = (endY - startY) * sampleWidth + startY;

					//Wrap sample coordinates to give "infinite" periodicity on maps
					sampleX %= 1;
					sampleY %= 1;

					Pixel col = SampleColor(sprGround, sampleX, sampleY);
					Draw(x, y + (ScreenHeight / 2), col);

					col = SampleColor(sprSky, sampleX, sampleY);
					Draw(x, (ScreenHeight / 2) - y, col);
				}
			}

			DrawLine(new Point(0, ScreenHeight / 2), new Point(ScreenWidth, ScreenHeight / 2), Pixel.Presets.Cyan);

			if (GetKey(Key.Left).Down)
				worldA -= 0.1f * elapsed;

			if (GetKey(Key.Right).Down)
				worldA += 0.1f * elapsed;

			if (GetKey(Key.Up).Down)
			{
				worldX += Cos(worldA) * 0.02f * elapsed;
				worldY += Sin(worldA) * 0.02f * elapsed;
			}

			if (GetKey(Key.Down).Down)
			{
				worldX -= Cos(worldA) * 0.02f * elapsed;
				worldY -= Sin(worldA) * 0.02f * elapsed;
			}

			DrawText(new Point(10, 10), $"X:{Round(worldX, 3)}", Pixel.Presets.Black, 1);
			DrawText(new Point(10, 25), $"Y:{Round(worldY, 3)}", Pixel.Presets.Black, 1);
			DrawText(new Point(10, 40), $"A:{Round(Degrees(worldA), 3)}", Pixel.Presets.Black, 1);
			DrawText(new Point(10, 55), $"S:{Round(speed, 3)}", Pixel.Presets.Black, 1);
		}

		private void Reset()
		{
			worldX = 1000.0f;
			worldY = 1000.0f;
			worldA = 0f;
			near = 0.005f;
			far = 0.03f;
			foVHalf = (float)Math.PI / 4.0f;
			speed = 1;
		}

		private Pixel SampleColor(Sprite spr, float x, float y)
		{
			int sx = (int)(x * spr.Width);
			int sy = (int)(y * spr.Height - 1.0f);
			if (sx < 0 || sx >= spr.Width || sy < 0 || sy >= spr.Height)
				return Pixel.Presets.Black;
			else
				return spr[sx, sy];
		}
	}
}
