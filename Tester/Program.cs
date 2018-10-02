using System;
using System.Collections.Generic;
using PixelEngine;

namespace Examples
{
	public class Snake : Game
	{
		private List<SnakeSegment> snake;
		private int foodX;
		private int foodY;
		private int score;
		private int dir;
		private bool dead;

		private Random rnd;
		private bool started;

		static void Main(string[] args)
		{
			Snake s = new Snake();
			s.Construct(500, 500, 10, 10, 30);
			s.Start();
		}

		private struct SnakeSegment
		{
			public SnakeSegment(int x, int y) : this()
			{
				this.X = x;
				this.Y = y;
			}

			public int X { get; private set; }
			public int Y { get; private set; }
		}

		public Snake() => AppName = "SNAKE!";

		public override void OnCreate() => Reset();

		private void Reset()
		{
			snake = new List<SnakeSegment>();
			for (int i = 0; i < 9; i++)
				snake.Add(new SnakeSegment(i + 20, 15));

			foodX = 30;
			foodY = 15;
			score = 0;
			dir = 3;
			dead = false;

			rnd = new Random();
		}

		public override void OnUpdate(TimeSpan elapsed)
		{
			if (!started)
			{
				AppName = "SNAKE! Press 'Enter' To Start";
				if (GetKey(Key.Enter).Pressed)
				{
					Reset();
					started = true;
				}
			}

			if (dead)
				started = false;

			if (GetKey(Key.Right).Pressed)
			{
				dir++;
				if (dir == 4)
					dir = 0;
			}

			if (GetKey(Key.Left).Pressed)
			{
				dir--;
				if (dir == -1)
					dir = 3;
			}

			if (started)
			{
				switch (dir)
				{
					case 0: // UP
						snake.Insert(0, new SnakeSegment(snake[0].X, snake[0].Y - 1));
						break;
					case 1: // RIGHT
						snake.Insert(0, new SnakeSegment(snake[0].X + 1, snake[0].Y));
						break;
					case 2: // DOWN
						snake.Insert(0, new SnakeSegment(snake[0].X, snake[0].Y + 1));
						break;
					case 3: // LEFT
						snake.Insert(0, new SnakeSegment(snake[0].X - 1, snake[0].Y));
						break;
				}

				snake.RemoveAt(snake.Count - 1);

				if (snake[0].X == foodX && snake[0].Y == foodY)
				{
					score++;
					AppName = "SNAKE! Score: " + score;
					RandomizeFood();

					snake.Add(new SnakeSegment(snake[snake.Count - 1].X, snake[snake.Count - 1].Y));
				}

				if (snake[0].X < -1 || snake[0].X >= ScreenWidth / PixWidth + 1 || snake[0].Y < -1 || snake[0].Y >= ScreenHeight / PixHeight + 1)
					dead = true;

				for (int i = 1; i < snake.Count; i++)
				{
					if (snake[i].X == snake[0].X && snake[i].Y == snake[0].Y)
						dead = true;
				}
			}

			Clear(Pixel.Black);

			DrawRect(new Point(0, 0), ScreenWidth / PixWidth - 1, ScreenHeight / PixHeight - 1, Pixel.Grey);

			for (int i = 1; i < snake.Count; i++)
				Draw(snake[i].X, snake[i].Y, dead ? Pixel.Blue : Pixel.Yellow);

			Draw(snake[0].X, snake[0].Y, dead ? Pixel.Green : Pixel.Magenta);

			Draw(foodX, foodY, Pixel.Red);
		}

		private void RandomizeFood()
		{
			while (GetDrawTarget()[foodX, foodY] != Pixel.Black)
			{
				foodX = rnd.Next(ScreenWidth);
				foodY = rnd.Next(ScreenHeight);
			}
		}
	}
}