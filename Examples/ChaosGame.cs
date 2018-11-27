/*
Ported from The Coding Train Chaos Game Challenge to C#
Check out the original at https://www.youtube.com/watch?v=v8lm5XZ2V6M
*/

using PixelEngine;

namespace Examples
{
	public class ChaosGame : Game
	{
		private Point[] vertices;
		private Point current;

		private Pixel[] colors;

		static void Main(string[] args)
		{
			ChaosGame game = new ChaosGame();
			game.Construct(250, 250, 2, 2);
			game.Start();
		}

		public override void OnCreate() => Reset();

		private void Reset()
		{
			vertices = new Point[]
			{
				new Point(ScreenWidth / 2, 0),
				new Point(0, ScreenHeight),
				new Point(ScreenWidth, ScreenHeight)
			};

			colors = MakeArray(vertices.Length, i => new Pixel((byte)Random(128, 256), (byte)Random(128, 256), (byte)Random(128, 256)));

			current = new Point(Random(ScreenWidth), Random(ScreenHeight));

			foreach (Point p in vertices)
				Draw(p, Pixel.Presets.White);
		}

		public override void OnUpdate(float elapsed)
		{
			if(GetKey(Key.Enter).Pressed)
			{
				Clear(Pixel.Presets.Black);
				Reset();
			}

			for (int i = 0; i < 1000; i++)
			{
				int x = 0;
				int y = 0;

				int r = Random(vertices.Length);
				Pixel col = Pixel.Presets.White;

				for (int v = 0; v < vertices.Length; v++)
				{
					if (r == v)
					{
						col = colors[v];
						x = (int)Lerp(current.X, vertices[v].X, 0.5f);
						y = (int)Lerp(current.Y, vertices[v].Y, 0.5f);
					}
				}

				current = new Point(x, y);
				Draw(current, col);
			}
		}
	}
}