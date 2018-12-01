using System.Threading;
using System.Threading.Tasks;

namespace PixelEngine.Utilities
{
	public class Animation<T>
	{
		public T Value { get; private set; }

		public bool Running { get; private set; }
		public bool Loop { get; set; }

		private T[] values;
		private int index;

		private int interval;

		public Animation(T[] values, float duration)
		{
			this.values = values;
			interval = (int)(duration * 1000 / values.Length);
		}

		public void Start()
		{
			if (!Running)
			{
				index = 0;
				Value = values[0];

				Running = true;

				Task.Run(Animate);
			}
		}

		public void Stop() => Running = false;

		private void Animate()
		{
			while (true)
			{
				if (!Running)
					break;

				Thread.Sleep(interval);

				index++;
				if (index == values.Length)
				{
					if (Loop)
						index = 0;
					else
						break;
				}

				Value = values[index];
			}

			Running = false;
		}
	}
}