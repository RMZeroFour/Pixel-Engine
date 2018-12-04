using System.Collections.Generic;

using PixelEngine;

namespace Examples
{
	public class Dodger : Game
	{
		private Player player;
		private List<Enemy> enemies;

		private bool started, gameOver;

		private int score;

		private float enemyTimer;
		private float enemyElapsed;

		private static void Main(string[] args)
		{
			Dodger game = new Dodger();
			game.Construct(250, 250, 2, 2);
			game.Start();
		}

		public override void OnCreate() => Reset();

		public override void OnUpdate(float elapsed)
		{
			Clear(Pixel.Presets.Black);

			if(!started)
			{
				DrawText(new Point(ScreenWidth / 4, 100), "Dodger", Pixel.Presets.White, 3);
				DrawText(new Point(ScreenWidth / 5, 150), "Press 'Enter' To Start", Pixel.Presets.White);

				if (GetKey(Key.Enter).Pressed)
					started = true;

				return;
			}

			if(gameOver)
			{
				DrawText(new Point(ScreenWidth / 4, ScreenHeight / 2), $"Your Score: {score}", Pixel.Presets.White);
				DrawText(new Point(ScreenWidth / 5, ScreenHeight / 2 + 10), "Press 'Enter' To Restart", Pixel.Presets.White);

				if (GetKey(Key.Enter).Pressed)
					Reset();
			}

			enemyElapsed += elapsed;

			if(enemyElapsed > enemyTimer)
			{
				enemies.Add(new Enemy(Random(ScreenWidth), -50, Random(200f, 300f), Random(5, 30), Random(5, 30)));
				enemyElapsed -= enemyTimer;

				enemyTimer = 0.25f + Random(-0.125f, -0.125f);
			}

			player.Render(this, gameOver ? Pixel.Presets.Red : Pixel.Presets.White);

			for (int i = enemies.Count - 1; i >= 0; i--)
			{
				Enemy enemy = enemies[i];

				if(!gameOver)
					enemy.Update(elapsed);

				enemy.Render(this);

				if (Colliding(enemy))
					gameOver = true;

				if (enemy.Pos.Y >= ScreenHeight + enemy.Height)
				{
					score += 10;
					enemies.RemoveAt(i);
				}
			}

			if (!gameOver)
			{
				if (GetKey(Key.Left).Down)
					player.Update(-1, elapsed);
				if (GetKey(Key.Right).Down)
					player.Update(1, elapsed);
			}

			player.Constrain(this);

			DrawText(new Point(5, 5), $"Score: {score}", Pixel.Presets.White);
		}

		private void Reset()
		{
			player = new Player(ScreenWidth / 2, ScreenHeight * 3 / 4, 200f, 15, 15);

			enemyTimer = 0.75f;
			enemies = new List<Enemy>();

			score = 0;

			gameOver = false;
		}

		private bool Colliding(Enemy e)
		{
			Point pe = e.Pos;
			Point pp = player.Pos;

			return (pp.X < pe.X + e.Width)
				&& (pp.X + player.Width > pe.X)
				&& (pp.Y < pe.Y + e.Height)
				&& (pp.Y + player.Height > pe.Y);
		}

		private class Player
		{
			public Player(float x, float y, float vel, int width, int height)
			{
				this.x = x;
				this.y = y;
				Vel = vel;
				Width = width;
				Height = height;
			}

			private float x, y;

			public Point Pos => new Point((int)x, (int)y);
			public float Vel { get; private set; }

			public int Width { get; private set; }
			public int Height { get; private set; }

			public void Update(int dir, float time) => x += Vel * time * dir;
			public void Constrain(Game g) => x = x < 0 ? 0 : x > g.ScreenWidth - Width ? g.ScreenWidth - Width : x;
			public void Render(Game g, Pixel col) => g.FillRect(Pos, Width, Height, col);
		}

		private class Enemy
		{
			public Enemy(int x, int y, float vel, int width, int height)
			{
				this.x = x;
				this.y = y;
				Vel = vel;
				Width = width;
				Height = height;
			}

			private float x, y;

			public Point Pos => new Point((int)x, (int)y);
			public float Vel { get; private set; }

			public int Width { get; private set; }
			public int Height { get; private set; }

			public void Update(float time) => y += Vel * time;
			public void Render(Game g) => g.FillRect(Pos, Width, Height, Pixel.Presets.Green);
		}
	}
}