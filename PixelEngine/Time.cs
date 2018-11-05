using System;

namespace PixelEngine
{
	public class Time
	{
		internal Time() => Start = DateTime.Now;

		public DateTime Start { get; internal set; }
		public TimeSpan Total => DateTime.Now - Start;
		public TimeSpan Elapsed { get; internal set; }
	}
}