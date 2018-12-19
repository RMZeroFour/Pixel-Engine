namespace PixelEngine
{
	public struct Input
	{
		public bool Pressed { get; internal set; }
		public bool Released { get; internal set; }
		public bool Down { get; internal set; }
		public bool Up => !Down;
	}
}