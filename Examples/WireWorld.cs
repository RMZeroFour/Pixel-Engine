using PixelEngine;
using System;

namespace Examples
{
	public class WireWorld : Game
	{
		// Current cells state
		private Cell[,] current;
		// Next cells state
		private Cell[,] next;

		// Is the game running?
		private bool running;

		// The last position of mouse
		private Point lastMousePos;

		static void Main()
		{
			WireWorld ww = new WireWorld();
			ww.Construct(75, 75, 10, 10, 30);
			ww.Start();
		}

		public override void OnCreate()
		{
			Enable(Subsystem.HrText);

			// Init the cell arrays
			current = new Cell[ScreenWidth, ScreenHeight];
			next = new Cell[ScreenWidth, ScreenHeight];

			// Write "WIREWORLD"
			Set(15, 28, "#########################################");
			Set(15, 29, "#.......................................#");
			Set(15, 30, "+...#####.###..###.#...#####.###..#...##.");
			Set(15, 31, "-...#..#..#..###...#...#.#.#.#..###...#.#");
			Set(15, 32, "#.#.#..#..###..###.#.#.#.#.#.###..#...#.#");
			Set(15, 33, "#.#.#..#..#.#..#...#.#.#.#.#.#.#..#...#.#");
			Set(15, 34, "##.##.#####..#.######.##.#####..#.######.");

		}

		// A utility function to set cells using text
		private void Set(int x, int y, string s)
		{
			for (int i = 0; i < s.Length; i++)
			{
				if (x > ScreenWidth - 1 || y > ScreenHeight - 1)
					return;

				Cell c = Cell.Empty;

				switch (s[i])
				{
					case '#':
						c = Cell.Conductor;
						break;
					case '+':
						c = Cell.ElectronHead;
						break;
					case '-':
						c = Cell.ElectronTail;
						break;
				}

				current[x, y] = c;

				x++;
			}
		}

		public override void OnUpdate(float elapsed)
		{
			if (!running && GetMouse(Mouse.Any).Down == true)
			{
				Cell c = Cell.Empty;

				// Left => Conductor
				if (GetMouse(Mouse.Left).Down == true)
					c = Cell.Conductor;
				// Right => Empty
				else if (GetMouse(Mouse.Right).Down == true)
					c = Cell.Empty;
				if (GetMouse(Mouse.Middle).Down == true)
				{
					// Middle + Shift => Tail
					if (GetKey(Key.Shift).Down)
						c = Cell.ElectronTail;
					// Middle => Head
					else
						c = Cell.ElectronHead;
				}

				Place(lastMousePos.X, lastMousePos.Y, MouseX, MouseY, c);
			}

			// Clear and render
			Clear(Pixel.Presets.Black);
			DrawCells();

			DrawTextHr(Point.Origin, running ? "Running" : "Paused", Pixel.Presets.White, 2);

			// Update only if running
			if (running)
				UpdateCells();

			// Capture the mouse position
			lastMousePos = new Point(MouseX, MouseY);
		}

		private void UpdateCells()
		{
			// Get total count of neighbouring cells which are electron heads
			int GetNeighbourElectronHeads(int x, int y)
			{
				int sum = 0;

				bool IsHead(int a, int b) => current[a, b] == Cell.ElectronHead;

				if (x > 0 && IsHead(x - 1, y)) // West
					sum++;
				if (x < ScreenWidth - 1 && IsHead(x + 1, y)) // East
					sum++;

				if (y > 0 && IsHead(x, y - 1)) // North
					sum++;
				if (y < ScreenHeight - 1 && IsHead(x, y + 1)) // South
					sum++;

				if (x > 0 && y > 0 && IsHead(x - 1, y - 1)) // North West
					sum++;
				if (x < ScreenWidth - 1 && y > 0 && IsHead(x + 1, y - 1)) // North East
					sum++;

				if (x > 0 && y < ScreenHeight - 1 && IsHead(x - 1, y + 1)) // South West
					sum++;
				if (x < ScreenWidth - 1 && y < ScreenHeight - 1 && IsHead(x + 1, y + 1)) // South East
					sum++;

				return sum;
			}

			for (int x = 0; x < ScreenWidth; x++)
			{
				for (int y = 0; y < ScreenHeight; y++)
				{
					switch (current[x, y])
					{
						case Cell.Empty:
							// Empty remain empty
							next[x, y] = Cell.Empty;
							break;
						case Cell.ElectronHead:
							// Heads become tails
							next[x, y] = Cell.ElectronTail;
							break;
						case Cell.ElectronTail:
							// Tails becoms conductors
							next[x, y] = Cell.Conductor;
							break;
						case Cell.Conductor:
							// Conductors become heads only if one or two
							// neighbours are heads
							int neighbourHeads = GetNeighbourElectronHeads(x, y);
							if (neighbourHeads == 1 || neighbourHeads == 2)
								next[x, y] = Cell.ElectronHead;
							else
								next[x, y] = Cell.Conductor;
							break;
					}
				}
			}

			// Swap state buffers
			for (int i = 0; i < ScreenWidth; i++)
				for (int j = 0; j < ScreenHeight; j++)
					current[i, j] = next[i, j];

		}

		private void DrawCells()
		{
			for (int i = 0; i < ScreenWidth; i++)
			{
				for (int j = 0; j < ScreenHeight; j++)
				{
					// Render according to color
					Pixel col = current[i, j] == Cell.ElectronHead ? Pixel.Presets.Blue :
						current[i, j] == Cell.ElectronTail ? Pixel.Presets.Red :
						current[i, j] == Cell.Conductor ? Pixel.Presets.Yellow : Pixel.Presets.Black;
					Draw(i, j, col);
				}
			}
		}

		// Place cells with the mouse using the Bresenham algorithm
		private void Place(int x, int y, int x2, int y2, Cell cell)
		{
			int w = x2 - x;
			int h = y2 - y;
			int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
			if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
			if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
			if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
			int longest = Math.Abs(w);
			int shortest = Math.Abs(h);
			if (!(longest > shortest))
			{
				longest = Math.Abs(h);
				shortest = Math.Abs(w);
				if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
				dx2 = 0;
			}
			int numerator = longest >> 1;
			for (int i = 0; i <= longest; i++)
			{
				current[x, y] = cell;

				numerator += shortest;
				if (!(numerator < longest))
				{
					numerator -= longest;
					x += dx1;
					y += dy1;
				}
				else
				{
					x += dx2;
					y += dy2;
				}
			}
		}

		public override void OnKeyPress(Key k)
		{
			// Pause game
			if (k == Key.Enter)
				running = !running;

			// Simulate next frame
			if (k == Key.F)
				UpdateCells();

			// Reset the field
			if (k == Key.R)
			{
				for (int i = 0; i < ScreenWidth; i++)
					for (int j = 0; j < ScreenHeight; j++)
						current[i, j] = next[i, j] = Cell.Empty;
			}
		}

		// A cell can be either of these types
		private enum Cell
		{
			Empty,
			ElectronHead,
			ElectronTail,
			Conductor
		}
	}
}
