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
		static void Main(string[] args)
		{
			Program p = new Program();
			p.Construct(500, 500, 1, 1);
			p.Start();
		}

		public Program() => AppName = "Hello World!";

		public override void OnUpdate(TimeSpan elapsed)
		{
			Clear(Pixel.Yellow);
			DrawText(new Point(240, 240), FrameCount.ToString(), Pixel.Cyan, 2.5f);
		}
	}
}