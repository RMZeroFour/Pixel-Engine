using PixelEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test
{
	public class Program : Game
	{
		static void Main(string[] args)
		{
			Program p = new Program();
			p.Construct(500, 500, 5, 5);
			p.Start();
		}

		public Program() => AppName = "Pixels";

		public override void OnUpdate(TimeSpan elapsed)
		{
			for (int i = 0; i < ScreenWidth / PixWidth; i++)
				for (int j = 0; j < ScreenHeight / PixHeight; j++)
					Draw(i, j, Pixel.Random());

			FillRect(new Point(MouseX - 1, MouseY - 1), 3, 3, Pixel.White);
		}

		public override void OnMouseScroll() => Console.WriteLine("Scrolling to {0}!", MouseScroll);
		public override void OnMousePress(Mouse b) => Console.WriteLine("Click!");
	}
}