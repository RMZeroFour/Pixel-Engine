using PixelEngine;
using PixelEngine.Extensions.Transforms;

namespace Examples
{
	public class Transformations : Game
	{
		private Sprite car;
		private float angle;

		static void Main(string[] args)
		{
			Transformations t = new Transformations();
			t.Construct(250, 250, 1, 1);
			t.Start();
		}

		public override void OnCreate()
		{
			car = Sprite.Load("Car.png");
			PixelMode = Pixel.Mode.Alpha;
		}

		public override void OnKeyDown(Key k)
		{
			switch (k)
			{
                		case Key.Left:
					angle += (float)Clock.Elapsed.TotalSeconds * 2;
					break;
                		case Key.Right:
					angle -= (float)Clock.Elapsed.TotalSeconds * 2;
					break;
			}
		}

		public override void OnUpdate(float elapsed)
		{
			Clear(Pixel.Presets.Cyan);

			Transform transform = new Transform();
			transform.Translate(-car.Width/2 , -car.Height/2);
			transform.Rotate(angle);
			transform.Translate(ScreenWidth/2 , ScreenHeight/2);
			
			Transform.DrawSprite(car, transform);
		}
	}
}
