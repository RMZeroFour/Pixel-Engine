using System;

namespace PixelEngine.Extensions
{
	public class Transform : Extension
	{
		public Transform()
		{
			matrix = new float[4, 3, 3];
			Reset();
		}

		#region Operations
		public void Reset()
		{
			targetMat = 0;
			sourceMat = 1;
			dirty = true;

			matrix[0, 0, 0] = 1.0f; matrix[0, 1, 0] = 0.0f; matrix[0, 2, 0] = 0.0f;
			matrix[0, 0, 1] = 0.0f; matrix[0, 1, 1] = 1.0f; matrix[0, 2, 1] = 0.0f;
			matrix[0, 0, 2] = 0.0f; matrix[0, 1, 2] = 0.0f; matrix[0, 2, 2] = 1.0f;

			matrix[1, 0, 0] = 1.0f; matrix[1, 1, 0] = 0.0f; matrix[1, 2, 0] = 0.0f;
			matrix[1, 0, 1] = 0.0f; matrix[1, 1, 1] = 1.0f; matrix[1, 2, 1] = 0.0f;
			matrix[1, 0, 2] = 0.0f; matrix[1, 1, 2] = 0.0f; matrix[1, 2, 2] = 1.0f;
		}

		public void Rotate(float angle)
		{
			matrix[2, 0, 0] = (float)Math.Cos(angle); matrix[2, 1, 0] = (float)Math.Sin(angle); matrix[2, 2, 0] = 0.0f;
			matrix[2, 0, 1] = -(float)Math.Sin(angle); matrix[2, 1, 1] = (float)Math.Cos(angle); matrix[2, 2, 1] = 0.0f;
			matrix[2, 0, 2] = 0.0f; matrix[2, 1, 2] = 0.0f; matrix[2, 2, 2] = 1.0f;
			Multiply();
		}

		public void Translate(float ox, float oy)
		{
			matrix[2, 0, 0] = 1.0f; matrix[2, 1, 0] = 0.0f; matrix[2, 2, 0] = ox;
			matrix[2, 0, 1] = 0.0f; matrix[2, 1, 1] = 1.0f; matrix[2, 2, 1] = oy;
			matrix[2, 0, 2] = 0.0f; matrix[2, 1, 2] = 0.0f; matrix[2, 2, 2] = 1.0f;
			Multiply();
		}

		public void Scale(float sx, float sy)
		{
			matrix[2, 0, 0] = sx; matrix[2, 1, 0] = 0.0f; matrix[2, 2, 0] = 0.0f;
			matrix[2, 0, 1] = 0.0f; matrix[2, 1, 1] = sy; matrix[2, 2, 1] = 0.0f;
			matrix[2, 0, 2] = 0.0f; matrix[2, 1, 2] = 0.0f; matrix[2, 2, 2] = 1.0f;
			Multiply();
		}

		public void Shear(float sx, float sy)
		{
			matrix[2, 0, 0] = 1.0f; matrix[2, 1, 0] = sx; matrix[2, 2, 0] = 0.0f;
			matrix[2, 0, 1] = sy; matrix[2, 1, 1] = 1.0f; matrix[2, 2, 1] = 0.0f;
			matrix[2, 0, 2] = 0.0f; matrix[2, 1, 2] = 0.0f; matrix[2, 2, 2] = 1.0f;
			Multiply();
		}

		public void Invert()
		{
			float det = matrix[sourceMat, 0, 0] * (matrix[sourceMat, 1, 1] * matrix[sourceMat, 2, 2] - matrix[sourceMat, 1, 2] * matrix[sourceMat, 2, 1]) -
						matrix[sourceMat, 1, 0] * (matrix[sourceMat, 0, 1] * matrix[sourceMat, 2, 2] - matrix[sourceMat, 2, 1] * matrix[sourceMat, 0, 2]) +
						matrix[sourceMat, 2, 0] * (matrix[sourceMat, 0, 1] * matrix[sourceMat, 1, 2] - matrix[sourceMat, 1, 1] * matrix[sourceMat, 0, 2]);

			float idet = 1 / det;

			matrix[3, 0, 0] = (matrix[sourceMat, 1, 1] * matrix[sourceMat, 2, 2] - matrix[sourceMat, 1, 2] * matrix[sourceMat, 2, 1]) * idet;
			matrix[3, 1, 0] = (matrix[sourceMat, 2, 0] * matrix[sourceMat, 1, 2] - matrix[sourceMat, 1, 0] * matrix[sourceMat, 2, 2]) * idet;
			matrix[3, 2, 0] = (matrix[sourceMat, 1, 0] * matrix[sourceMat, 2, 1] - matrix[sourceMat, 2, 0] * matrix[sourceMat, 1, 1]) * idet;
			matrix[3, 0, 1] = (matrix[sourceMat, 2, 1] * matrix[sourceMat, 0, 2] - matrix[sourceMat, 0, 1] * matrix[sourceMat, 2, 2]) * idet;
			matrix[3, 1, 1] = (matrix[sourceMat, 0, 0] * matrix[sourceMat, 2, 2] - matrix[sourceMat, 2, 0] * matrix[sourceMat, 0, 2]) * idet;
			matrix[3, 2, 1] = (matrix[sourceMat, 0, 1] * matrix[sourceMat, 2, 0] - matrix[sourceMat, 0, 0] * matrix[sourceMat, 2, 1]) * idet;
			matrix[3, 0, 2] = (matrix[sourceMat, 0, 1] * matrix[sourceMat, 1, 2] - matrix[sourceMat, 0, 2] * matrix[sourceMat, 1, 1]) * idet;
			matrix[3, 1, 2] = (matrix[sourceMat, 0, 2] * matrix[sourceMat, 1, 0] - matrix[sourceMat, 0, 0] * matrix[sourceMat, 1, 2]) * idet;
			matrix[3, 2, 2] = (matrix[sourceMat, 0, 0] * matrix[sourceMat, 1, 1] - matrix[sourceMat, 0, 1] * matrix[sourceMat, 1, 0]) * idet;
		}

		public static void DrawSprite(Sprite spr, Transform transform)
		{
			if (spr == null)
				return;

			float ex = 0, ey = 0;
			float px, py;
			float sx, sy;

			transform.Forward(0.0f, 0.0f, out sx, out sy);
			px = ex = sx; py = ey = sy;

			transform.Forward(spr.Width, spr.Height, out px, out py);
			sx = Math.Min(sx, px);
			sy = Math.Min(sy, py);
			ex = Math.Max(ex, px);
			ey = Math.Max(ey, py);

			transform.Forward(0.0f, spr.Height, out px, out py);
			sx = Math.Min(sx, px);
			sy = Math.Min(sy, py);
			ex = Math.Max(ex, px);
			ey = Math.Max(ey, py);

			transform.Forward(spr.Width, 0.0f, out px, out py);
			sx = Math.Min(sx, px);
			sy = Math.Min(sy, py);
			ex = Math.Max(ex, px);
			ey = Math.Max(ey, py);

			if (transform.dirty)
			{
				transform.Invert();
				transform.dirty = false;
			}

			if (ex < sx)
			{
				float t = sx;
				sx = ex;
				ex = t;
			}

			if (ey < sy)
			{
				float t = sy;
				sy = ey;
				ey = t;
			}

			for (float i = sx; i < ex; i++)
			{
				for (float j = sy; j < ey; j++)
				{
					float ox, oy;
					transform.Backward(i, j, out ox, out oy);
					Game.Draw((int)i, (int)j, spr[(int)(ox + 0.5f), (int)(oy + 0.5f)]);
				}
			}
		}
		#endregion

		#region Helpers
		private void Multiply()
		{
			for (int c = 0; c < 3; c++)
			{
				for (int r = 0; r < 3; r++)
				{
					matrix[targetMat, c, r] = matrix[2, 0, r] * matrix[sourceMat, c, 0] +
												 matrix[2, 1, r] * matrix[sourceMat, c, 1] +
												 matrix[2, 2, r] * matrix[sourceMat, c, 2];
				}
			}

			int t = targetMat;
			targetMat = sourceMat;
			sourceMat = t;

			dirty = true;
		}

		private void Forward(float ix, float iy, out float ox, out float oy)
		{
			ox = ix * matrix[sourceMat, 0, 0] + iy * matrix[sourceMat, 1, 0] + matrix[sourceMat, 2, 0];
			oy = ix * matrix[sourceMat, 0, 1] + iy * matrix[sourceMat, 1, 1] + matrix[sourceMat, 2, 1];
		}

		private void Backward(float ix, float iy, out float ox, out float oy)
		{
			ox = ix * matrix[3, 0, 0] + iy * matrix[3, 1, 0] + matrix[3, 2, 0];
			oy = ix * matrix[3, 0, 1] + iy * matrix[3, 1, 1] + matrix[3, 2, 1];
		}

		private float[,,] matrix;
		private int targetMat;
		private int sourceMat;
		private bool dirty; 
		#endregion
	}
}