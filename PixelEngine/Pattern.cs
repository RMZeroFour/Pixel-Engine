using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelEngine
{
	public class Pattern
	{
		private Sprite spr;

		public static readonly Dictionary<char, Pixel> DefaultPallete
			= new Dictionary<char, Pixel>()
			{
				[' '] = Pixel.White,
				['k'] = Pixel.Black,
				['r'] = Pixel.Red,
				['g'] = Pixel.Green,
				['b'] = Pixel.Blue,
				['c'] = Pixel.Cyan,
				['m'] = Pixel.Magenta,
				['y'] = Pixel.Yellow,
				['.'] = Pixel.Grey
			};

		public Pattern(int w, int h, string pattern, Dictionary<char, Pixel> pallete)
		{
			spr = new Sprite(w, h);

			Pixel SelectPixel(char c)
			{
				if (pallete.ContainsKey(c))
					return pallete[c];
				return Pixel.Black;
			}

			if (pallete == null)
				return;

			for (int i = 0; i < w; i++)
				for (int j = 0; j < h; j++)
					spr[i, j] = SelectPixel(pattern[j * w + i]);
		}

		public Sprite ToSprite() => spr;
	}
}