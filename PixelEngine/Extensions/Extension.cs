namespace PixelEngine.Extensions
{
	public abstract class Extension
	{
		internal static void Init(Game game) => Game = game;

		protected static Game Game { get; private set; }
	}
}