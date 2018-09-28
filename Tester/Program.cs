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

		private int size;
		private Pixel col = Pixel.White;

		public Program() => AppName = "Pixels";

		public override void OnUpdate(TimeSpan elapsed)
		{
			Clear(Pixel.Black);
			FillCircle(new Point(MouseX, MouseY), size * 2 + 1, col);
		}

		public override void OnMouseScroll() => size = Math.Abs(MouseScroll);
		public override void OnMouseDown(Mouse m) => col = Pixel.Random();
		public override void OnMouseRelease(Mouse m) => col = Pixel.White;
	}
}