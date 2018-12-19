using System.Collections.Generic;

using PixelEngine;

namespace Examples
{
	public class Matrix : Game
	{
		private const int MaxStreamers = 300;

		private List<Streamer> streamers;

		private static void Main(string[] args)
		{
			Matrix game = new Matrix();
			game.Construct(500, 500, 1, 1);
			game.Start();
		}

		public override void OnCreate()
		{
			streamers = new List<Streamer>(MaxStreamers);
			for (int n = 0; n < MaxStreamers; n++)
			{
				Streamer s = new Streamer();
				PrepareStreamer(ref s, Random(-ScreenHeight / 6, ScreenHeight * 5 / 6));
				streamers.Add(s);
			}
		}

		public override void OnUpdate(float elapsed)
		{
			Clear(Pixel.Presets.Black);

			for (int k = 0; k < streamers.Count; k++)
			{
				Streamer s = streamers[k];

				s.Position += elapsed * s.Speed * 10;

				for (int i = 0; i < s.Text.Length; i++)
				{
					Pixel col = s.Speed > 10 ? Pixel.Presets.Green : Pixel.Presets.DarkGreen;

					int index = (i - (int)s.Position) % s.Text.Length;

					DrawText(new Point(s.Column * 8, (int)s.Position - i * 8), s.Text[i].ToString(), col);

					if (Random(1000) < 5)
						s.Text = s.Text.Remove(i, 1).Insert(i, RandomChar().ToString());
				}

				if (s.Position - s.Text.Length * 8 >= ScreenHeight)
					PrepareStreamer(ref s, 0);

				streamers[k] = s;
			}
		}

		private char RandomChar() => (char)Random(32, 128);

		private void PrepareStreamer(ref Streamer s, int pos)
		{
			s.Column = Random(ScreenWidth / 8);
			s.Position = pos;
			s.Speed = Random(10, 25);
			s.Text = string.Concat(MakeArray(Random(10, 20), i => RandomChar()));
		}

		private struct Streamer
		{
			public int Column { get; set; }
			public float Position { get; set; }
			public float Speed { get; set; }
			public string Text { get; set; }
		}
	}
}