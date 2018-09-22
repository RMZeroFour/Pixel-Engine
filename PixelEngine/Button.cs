namespace PixelEngine
{
	public struct Button
	{
		public bool Pressed { get; internal set; }
		public bool Released { get; internal set; }
		public bool Down { get; internal set; }
		public bool Up => !Down;
	}
}