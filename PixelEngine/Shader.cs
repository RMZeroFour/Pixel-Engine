namespace PixelEngine
{
	public class Shader
	{
		private Dictionary<string, object> data;
		
		public ShaderFunc Calculate { get; private set; }

		public Shader(ShaderFunc calculate) 
		{
			Calculate = calculate;
			data = new Dictionary<string, object>();
		}
		
		public T Data<T>(string key) => data[key];
		public void Data<T>(string key, T item) => data[key] = item;
	}

	public delegate Pixel ShaderFunc(int x, int y, Pixel prev, Pixel cur);
 }
