using System;

namespace PixelEngine
{
	internal class Timer
	{
		public Timer(float interval) => Interval = interval;

		public float Interval { get; private set; }

		private DateTime last;

		public bool Tick()
		{
			if ((DateTime.Now - last).TotalMilliseconds >= Interval)
			{
				last = DateTime.Now;
				return true;
			}
			return false;
		}

		public void Init() => last = DateTime.Now;
		public void Init(DateTime time) => last = time;
	}
}
