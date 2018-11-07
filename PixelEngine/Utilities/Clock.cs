using System;

namespace PixelEngine.Utilities
{
	public class Clock
	{
		internal Clock() => Start = DateTime.Now;

		public DateTime Start { get; internal set; }
		public TimeSpan Total => DateTime.Now - Start;
		public TimeSpan Elapsed { get; internal set; }
	}
}