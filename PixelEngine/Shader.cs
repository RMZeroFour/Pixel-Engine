namespace PixelEngine
{
	public class Shader
	{
		public ShaderFunc Calculate { get; private set; }

		public Shader(ShaderFunc calculate) => Calculate = calculate;
	}

	public delegate Pixel ShaderFunc(int x, int y, Pixel prev, Pixel cur);
}