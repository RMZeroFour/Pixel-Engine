namespace PixelEngine
{
	public struct Point
	{
		public Point(int x, int y) : this()
		{
			this.X = x;
			this.Y = y;
		}

		public int X { get; private set; }
		public int Y { get; private set; }

		public static Point Origin => new Point();
	}
}
