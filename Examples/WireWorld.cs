using PixelEngine;

namespace Examples
{
	public class WireWorld : Game
	{
		private Cell[,] current;
		private Cell[,] next;

		private bool running;

		static void Main(string[] args)
		{
			WireWorld ww = new WireWorld();
			ww.Construct(75, 75, 10, 10, 25);
			ww.Start();
		}

		private enum Cell
		{
			Empty,
			ElectronHead,
			ElectronTail,
			Conductor
		}

		public override void OnCreate()
		{
			current = new Cell[ScreenWidth, ScreenHeight];
			next = new Cell[ScreenWidth, ScreenHeight];

			Set(15, 28, "#########################################");
			Set(15, 29, "#.......................................#");
			Set(15, 30, "+...#####.###..###.#...#####.###..#...##.");
			Set(15, 31, "-...#..#..#..###...#...#.#.#.#..###...#.#");
			Set(15, 32, "#.#.#..#..###..###.#.#.#.#.#.###..#...#.#");
			Set(15, 33, "#.#.#..#..#.#..#...#.#.#.#.#.#.#..#...#.#");
			Set(15, 34, "##.##.#####..#.######.##.#####..#.######.");
		}

		public override void OnUpdate(float elapsed)
		{
			Clear(Pixel.Presets.Black);
			DrawCells();

			if (running)
				UpdateCells();
		}

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

		private void UpdateCells()
		{
			int GetNeighbourElectronHeads(int x, int y)
			{
				int sum = 0;

				bool IsHead(int a, int b) => current[a, b] == Cell.ElectronHead;

				if (x >= 0 && IsHead(x - 1, y)) // West
					sum++;
				if (x < ScreenWidth - 1 && IsHead(x + 1, y)) // East
					sum++;

				if (y >= 0 && IsHead(x, y - 1)) // North
					sum++;
				if (y < ScreenHeight - 1 && IsHead(x, y + 1)) // South
					sum++;

				if (x >= 0 && y >= 0 && IsHead(x - 1, y - 1)) // North West
					sum++;
				if (x < ScreenWidth - 1 && y >= 0 && IsHead(x + 1, y - 1)) // North East
					sum++;

				if (x >= 0 && y < ScreenHeight - 1 && IsHead(x - 1, y + 1)) // South West
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
							next[x, y] = Cell.Empty;
							break;
						case Cell.ElectronHead:
							next[x, y] = Cell.ElectronTail;
							break;
						case Cell.ElectronTail:
							next[x, y] = Cell.Conductor;
							break;
						case Cell.Conductor:
							int neighbourHeads = GetNeighbourElectronHeads(x, y);
							if (neighbourHeads == 1 || neighbourHeads == 2)
								next[x, y] = Cell.ElectronHead;
							else
								next[x, y] = Cell.Conductor;
							break;
					}
				}
			}

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
					Pixel col = current[i, j] == Cell.ElectronHead ? Pixel.Presets.Blue :
						current[i, j] == Cell.ElectronTail ? Pixel.Presets.Red :
						current[i, j] == Cell.Conductor ? Pixel.Presets.Yellow : Pixel.Presets.Black;
					Draw(i, j, col);
				}
			}
		}

		public override void OnMouseDown(Mouse m)
		{
			if(!running)
			{
				if (m == Mouse.Left)
					current[MouseX, MouseY] = Cell.Conductor;
				else if (m == Mouse.Right)
					current[MouseX, MouseY] = Cell.Empty;
				if (m == Mouse.Middle)
				{
					if (GetKey(Key.Shift).Down)
						current[MouseX, MouseY] = Cell.ElectronTail;
					else
						current[MouseX, MouseY] = Cell.ElectronHead;
				}
			}
		}

		public override void OnKeyPress(Key k)
		{
			if (k == Key.Enter)
			{
				running = !running;
				AppName = "WireWorld " + (running ? "Running" : "Paused");
			}

			if (k == Key.F)
				UpdateCells();

			if (k == Key.R)
			{
				for (int i = 0; i < ScreenWidth; i++)
					for (int j = 0; j < ScreenHeight; j++)
						current[i, j] = next[i, j] = Cell.Empty;
			}
		}
	}
}