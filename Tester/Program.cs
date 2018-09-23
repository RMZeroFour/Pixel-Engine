using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixelEngine;

namespace Tester
{
	public class Program : Game
	{
		private Random rnd = new Random();

		static void Main(string[] args)
		{
			Program p = new Program();
			p.Construct(500, 500, 10, 10);
			p.Start();
		}

		public Program() => AppName = "Hello World!";

		private Pixel[] pixels = { Pixel.White, Pixel.Red, Pixel.Green, Pixel.Blue, Pixel.Cyan, Pixel.Magenta, Pixel.Yellow, Pixel.Grey };

		public override void OnUpdate(TimeSpan elapsed) => Clear(pixels[rnd.Next(pixels.Length)]);

		public override void OnMousePress(Mouse b) => NoLoop();
		public override void OnMouseRelease(Mouse b) => Loop();
	}
}