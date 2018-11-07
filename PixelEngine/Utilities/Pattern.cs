using System.Collections.Generic;

namespace PixelEngine.Utilities
{
	public class Pattern
	{
		private Sprite spr;

		public static readonly Dictionary<char, Pixel> DefaultPallete 
			= new Dictionary<char, Pixel>()
		{
			[' '] = Pixel.Presets.White,
			['k'] = Pixel.Presets.Black,
			['r'] = Pixel.Presets.Red,
			['g'] = Pixel.Presets.Green,
			['b'] = Pixel.Presets.Blue,
			['c'] = Pixel.Presets.Cyan,
			['m'] = Pixel.Presets.Magenta,
			['y'] = Pixel.Presets.Yellow,
			['.'] = Pixel.Presets.Grey
		};

		public Pattern(int w, int h, string pattern) : this(w, h, pattern, DefaultPallete) { }
		public Pattern(int w, int h, string pattern, Dictionary<char, Pixel> pallete)
		{
			spr = new Sprite(w, h);

			Pixel SelectPixel(char c)
			{
				if (pallete.ContainsKey(c))
					return pallete[c];
				return Pixel.Presets.Black;
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