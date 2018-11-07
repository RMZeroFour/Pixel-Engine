/*
Ported from One Lone Coder Mazes to C#
Check out the original at https://github.com/OneLoneCoder/videos/blob/master/OneLoneCoder_Mazes.cpp
*/

using System;
using System.Collections.Generic;

using PixelEngine;

namespace Examples
{
	public class Mazes : Game
	{
		[Flags]
		private enum Cells
		{
			PathNorth = 1 << 0,
			PathEast = 1 << 1,
			PathSouth = 1 << 2,
			PathWest = 1 << 3,
			Visited = 1 << 4
		}

		static void Main(string[] args)
		{
			Mazes st = new Mazes();
			st.Construct();
			st.Start();
		}

		public Mazes() => AppName = "MAZE!";

		private const int MazeWidth = 20;
		private const int MazeHeight = 20;
		private const int PathWidth = 4;

		private Cells[] maze;

		private int visited;

		private Stack<Point> pointStack = new Stack<Point>();

		public override void OnCreate() => Reset();

		private void Reset()
		{
			maze = new Cells[MazeWidth * MazeHeight];

			int x = Random(MazeWidth);
			int y = Random(MazeHeight);

			pointStack.Push(new Point(x, y));

			maze[y * MazeWidth + x] = Cells.Visited;

			visited = 1;
		}

		public override void OnKeyPress(Key k)
		{
			if (k == Key.R)
				Reset();
		}

		public override void OnUpdate(float elapsed)
		{
			int GetOffsetCellIndex(int x, int y) => (pointStack.Peek().Y + y) * MazeWidth + (pointStack.Peek().X + x);

			if (visited < MazeWidth * MazeHeight)
			{
				List<Cells> neighbours = new List<Cells>();

				Point current = pointStack.Peek();

				// North neighbour
				if (current.Y > 0 && (maze[GetOffsetCellIndex(0, -1)] & Cells.Visited) == 0)
					neighbours.Add(Cells.PathNorth);
				// East neighbour
				if (current.X < MazeWidth - 1 && (maze[GetOffsetCellIndex(1, 0)] & Cells.Visited) == 0)
					neighbours.Add(Cells.PathEast);
				// South neighbour
				if (current.Y < MazeHeight - 1 && (maze[GetOffsetCellIndex(0, 1)] & Cells.Visited) == 0)
					neighbours.Add(Cells.PathSouth);
				// West neighbour
				if (current.X > 0 && (maze[GetOffsetCellIndex(-1, 0)] & Cells.Visited) == 0)
					neighbours.Add(Cells.PathWest);

				if (neighbours.Count > 0)
				{

					Cells nextDir = neighbours[Random(neighbours.Count)];

					switch (nextDir)
					{
						case Cells.PathNorth:
							maze[GetOffsetCellIndex(0, -1)] |= Cells.Visited | Cells.PathSouth;
							maze[GetOffsetCellIndex(0, 0)] |= Cells.PathNorth;
							pointStack.Push(new Point(current.X, current.Y - 1));
							break;
						case Cells.PathEast:
							maze[GetOffsetCellIndex(1, 0)] |= Cells.Visited | Cells.PathWest;
							maze[GetOffsetCellIndex(0, 0)] |= Cells.PathEast;
							pointStack.Push(new Point(current.X + 1, current.Y));
							break;

						case Cells.PathSouth:
							maze[GetOffsetCellIndex(0, 1)] |= Cells.Visited | Cells.PathNorth;
							maze[GetOffsetCellIndex(0, 0)] |= Cells.PathSouth;
							pointStack.Push(new Point(current.X, current.Y + 1));
							break;

						case Cells.PathWest:
							maze[GetOffsetCellIndex(-1, 0)] |= Cells.Visited | Cells.PathEast;
							maze[GetOffsetCellIndex(0, 0)] |= Cells.PathWest;
							pointStack.Push(new Point(current.X - 1, current.Y));
							break;
					}

					visited++;
				}
				else
				{
					pointStack.Pop();
				}
			}


			Clear(Pixel.Presets.Black);

			for (int x = 0; x < MazeWidth; x++)
			{
				for (int y = 0; y < MazeHeight; y++)
				{
					for (int py = 0; py < PathWidth; py++)
					{
						for (int px = 0; px < PathWidth; px++)
						{
							Pixel p = (maze[y * MazeWidth + x] & Cells.Visited) != 0 ? Pixel.Presets.White : Pixel.Presets.Blue;
							Draw(x * (PathWidth + 1) + px, y * (PathWidth + 1) + py, p);
						}
					}

					for (int p = 0; p < PathWidth; p++)
					{
						if ((maze[y * MazeWidth + x] & Cells.PathSouth) != 0)
							Draw(x * (PathWidth + 1) + p, y * (PathWidth + 1) + PathWidth, Pixel.Presets.White);

						if ((maze[y * MazeWidth + x] & Cells.PathEast) != 0)
							Draw(x * (PathWidth + 1) + PathWidth, y * (PathWidth + 1) + p, Pixel.Presets.White);
					}
				}
			}

			for (int py = 0; py < PathWidth; py++)
				for (int px = 0; px < PathWidth; px++)
					Draw(pointStack.Peek().X * (PathWidth + 1) + px, pointStack.Peek().Y * (PathWidth + 1) + py, Pixel.Presets.Green);
		}
	}
}