using PixelEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tester
{
	public class Program : Game
	{
		static void Main(string[] args)
		{
			Program p = new Program();
			p.Construct(500, 500, 5, 5);
			p.Start();
		}
	}
}