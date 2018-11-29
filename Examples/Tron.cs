using System.Collections.Generic;
using System.Linq;

using PixelEngine;

namespace Examples
{
	public class Tron : Game
	{
		private LightCycle playerA;
		private LightCycle playerB;

		private LightCycle loser;

		private const int Size = 2;

		static void Main(string[] args)
		{
			Tron game = new Tron();
			game.Construct();
			game.Start();
		}

		public Tron() => AppName = "TRON";

		public override void OnCreate()
		{
			Enable(Subsystem.HrText);
			Reset();
		}

		private void Reset()
		{
			playerA = new LightCycle(10, 10, Direction.South);
			playerB = new LightCycle(ScreenWidth - 10, ScreenHeight - 10, Direction.North);
			loser = null;
		}

		public override void OnUpdate(float elapsed)
		{
			Delay(0.025f);

			Clear(Pixel.Presets.Black);

			foreach (Point p in playerA.Points)
				FillRect(p, Size, Size, Pixel.Presets.Orange);

			foreach (Point p in playerB.Points)
				FillRect(p, Size, Size, Pixel.Presets.Cyan);

			if (loser == null)
			{
				if (!MoveCycle(playerA, playerB))
					loser = playerA;

				if (!MoveCycle(playerB, playerA))
					loser = playerB;
			}
			else
			{
				if (loser == playerA)
					DrawTextHr(new Point(ScreenWidth * PixWidth / 3, ScreenHeight * PixHeight / 2), "Player A Loses", Pixel.Presets.White, 2);
				else if (loser == playerB)
					DrawTextHr(new Point(ScreenWidth * PixWidth / 3, ScreenHeight * PixHeight / 2), "Player B Loses", Pixel.Presets.White, 2);

				DrawTextHr(new Point(ScreenWidth * PixWidth / 3, ScreenHeight * PixHeight / 2 + 20), "Press 'Enter' To Restart", Pixel.Presets.White);

				if (GetKey(Key.Enter).Pressed)
					Reset();
			}
		}

		private bool MoveCycle(LightCycle cycle, LightCycle other)
		{
			Point prev = cycle.Points.Last();
			Point next = Point.Origin;

			switch (cycle.Direction)
			{
				case Direction.North:
					next = new Point(prev.X, prev.Y - Size);
					break;
				case Direction.East:
					next = new Point(prev.X - Size, prev.Y);
					break;
				case Direction.South:
					next = new Point(prev.X, prev.Y + Size);
					break;
				case Direction.West:
					next = new Point(prev.X + Size, prev.Y);
					break;
			}

			if (next.X < 0 || next.Y < 0 ||
				next.X > ScreenWidth || next.Y > ScreenHeight)
				return false;

			foreach (Point p in cycle.Points)
			{
				if (next.X < p.X + Size && next.X + Size > p.X
					&& next.Y < p.Y + Size && next.Y + Size > p.Y)
					return false;
			}

			foreach (Point p in other.Points)
			{
				if (next.X < p.X + Size && next.X + Size > p.X 
					&& next.Y < p.Y + Size && next.Y + Size > p.Y)
					return false;
			}

			cycle.Points.Add(next);

			return true;
		}

		public override void OnKeyPress(Key k)
		{
			int aDir = (int)playerA.Direction;
			int bDir = (int)playerB.Direction;

			switch (k)
			{
				case Key.Left:
					aDir++;
					if (aDir == 4)
						aDir = 0;
					break;

				case Key.Right:
					aDir--;
					if (aDir == -1)
						aDir = 3;
					break;

				case Key.A:
					bDir++;
					if (bDir == 4)
						bDir = 0;
					break;

				case Key.D:
					bDir--;
					if (bDir == -1)
						bDir = 3;
					break;
			}

			playerA.Direction = (Direction)aDir;
			playerB.Direction = (Direction)bDir;
		}

		private enum Direction
		{
			North,
			East,
			South,
			West
		}

		private class LightCycle
		{
			public LightCycle(int x, int y, Direction dir)
			{
				Points = new List<Point>() { new Point(x, y) };
				Direction = dir;
			}

			public List<Point> Points { get; private set; }
			public Direction Direction { get; set; }
		}
	}
}