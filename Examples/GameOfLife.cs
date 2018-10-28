using System;
using PixelEngine;

namespace Examples
{
	public class GameOfLife : Game
	{
		// Current instance
		private bool[,] grid;
		// Next instance
		private bool[,] newGrid;

		// Is the game running or paused?
		private bool running = false;

		// Color of alive pixels
		private Pixel alive = Pixel.Presets.White;
		// Color of deas pixels
		private Pixel dead = Pixel.Presets.Black;

		static void Main(string[] args)
		{
			GameOfLife gol = new GameOfLife();
			gol.Construct(frameRate: 45);
			gol.Start();
		}

		// Utility function to set cells
		private void Set(int x, int y, string s)
		{
			for (int p = 0; p < s.Length; p++)
			{
				int i = y * ScreenWidth + x + p;
				int nx = i % ScreenWidth;
				int ny = i / ScreenWidth;
				grid[nx, ny] = (s[p] == '#');
			}
		}

		public override void OnCreate()
		{
			// Init the grids 
			// We need two buffers so that cells don't change due to looping in a fixed order
			grid = new bool[ScreenWidth, ScreenHeight];
			newGrid = new bool[ScreenWidth, ScreenHeight];

			Set(15, 48, "#################################################################");
			Set(15, 49, "#...............................................................#");
			Set(15, 50, "#..####......................####.........#...................#.#");
			Set(15, 51, "#.#.......###..#####.####...#....#.####...#....###.####.####..#.#");
			Set(15, 52, "#.#...##.#...#.#.#.#.#......#....#.#......#.....#..#....#.....#.#");
			Set(15, 53, "#.#....#.#####.#.#.#.####...#....#.####...#.....#..####.####..#.#");
			Set(15, 54, "#.#....#.#...#.#.#.#.#......#....#.#......#.....#..#....#.......#");
			Set(15, 55, "#..####..#...#.#.#.#.####....####..#......####.###.#....####..#.#");
			Set(15, 56, "#...............................................................#");
			Set(15, 57, "#################################################################");

			//// Make two gosper guns
			//MakeGosperGun(10, 25);
			//MakeGosperGun(50, 25);
		}

		// Utility to make a gosper glider gun
		private void MakeGosperGun(int x, int y)
		{
			Set(x, y + 0, "........................#............");
			Set(x, y + 1, "......................#.#............");
			Set(x, y + 2, "............##......##............##.");
			Set(x, y + 3, "...........#...#....##............##.");
			Set(x, y + 4, "##........#.....#...##...............");
			Set(x, y + 5, "##........#...#.##....#.#............");
			Set(x, y + 6, "..........#.....#.......#............");
			Set(x, y + 7, "...........#...#.....................");
			Set(x, y + 8, "............##.......................");
		}

		public override void OnUpdate(float elapsed)
		{
			// Clear field
			Clear(dead);

			// Draw all cells
			DrawCells();

			// If not paused
			if (running)
			{
				// Update all cells
				UpdateCells();

				// Switch buffer
				for (int i = 0; i < ScreenWidth; i++)
					for (int j = 0; j < ScreenHeight; j++)
						grid[i, j] = newGrid[i, j];
			}
		}

		// Update the cells
		private void UpdateCells()
		{
			int GetCell(int a, int b)
			{
				if (a < 0 || a >= ScreenWidth || b < 0 || b >= ScreenHeight)
					return 0;
				return grid[a, b] ? 1 : 0;
			}

			for (int x = 0; x < ScreenWidth; x++)
			{
				for (int y = 0; y < ScreenHeight; y++)
				{
					int neighbors = GetCell(x - 1, y - 1) + GetCell(x - 0, y - 1) + GetCell(x + 1, y - 1) +
									GetCell(x - 1, y + 0) + 0 + GetCell(x + 1, y + 0) +
									GetCell(x - 1, y + 1) + GetCell(x + 0, y + 1) + GetCell(x + 1, y + 1);

					newGrid[x, y] = grid[x, y] ? (neighbors == 2 || neighbors == 3) : (neighbors == 3);
				}
			}
		}

		// Draw the cells
		private void DrawCells()
		{
			for (int i = 0; i < ScreenWidth; i++)
				for (int j = 0; j < ScreenHeight; j++)
					if (grid[i, j])
						Draw(i, j, alive);
		}

		// Flip cell status
		public override void OnMousePress(Mouse m) => grid[MouseX, MouseY] = !grid[MouseX, MouseY];

		// Pause the game
		public override void OnKeyPress(Key k)
		{
			if (k == Key.Enter)
				running = !running;
		}
	}
}
