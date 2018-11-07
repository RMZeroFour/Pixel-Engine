using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelEngine.Utilities
{
	public class StateMachine<T> where T : Enum
	{
		public T CurrentState { get; private set; }

		private readonly T[] states;

		private Action<T> transition;

		public StateMachine() => states = (T[])Enum.GetValues(typeof(T));
		public StateMachine(params T[] states)
		{
			if (states.Length > 0)
			{
				this.states = new T[states.Length];
				states.CopyTo(this.states, 0);
				Switch(states[0]);
			}
		}

		public void OnTransition(Action<T> transition) => this.transition += transition;

		public void Switch(T newState)
		{
			if (states.Contains(newState))
			{
				CurrentState = newState;
				transition?.Invoke(CurrentState);
			}
		}
	}
}