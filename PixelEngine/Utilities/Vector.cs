using System;

namespace PixelEngine.Utilities
{
	public struct Vector : IEquatable<Vector>
	{
		public float X { get; private set; }
		public float Y { get; private set; }

		public static Vector Zero => new Vector(0, 0);
		public static Vector One => new Vector(1, 1);
		public static Vector Right => new Vector(1, 0);
		public static Vector Up => new Vector(0, 1);

		public Vector(float x, float y)
		{
			X = x;
			Y = y;
		}

		#region Calculations
		public float Magnitude()
		{
			float ls = X * X + Y * Y;
			return (float)Math.Sqrt((double)ls);
		}

		public static float Distance(Vector a, Vector b)
		{
			float dx = a.X - b.X;
			float dy = a.Y - b.Y;

			float ls = dx * dx + dy * dy;

			return (float)Math.Sqrt((double)ls);
		}

		public static Vector Normalize(Vector v)
		{
			float ls = v.X * v.X + v.Y * v.Y;
			float invNorm = (float)(1 / Math.Sqrt(ls));

			return new Vector(v.X * invNorm, v.Y * invNorm);
		}

		public static Vector Reflect(Vector vector, Vector normal)
		{
			float dot = vector.X * normal.X + vector.Y * normal.Y;
			dot *= 2;
			return new Vector(vector.X - dot * normal.X, vector.Y - dot * normal.Y);
		}

		public static Vector Constrain(Vector value1, Vector min, Vector max)
		{
			float x = value1.X;
			x = (x > max.X) ? max.X : x;
			x = (x < min.X) ? min.X : x;

			float y = value1.Y;
			y = (y > max.Y) ? max.Y : y;
			y = (y < min.Y) ? min.Y : y;

			return new Vector(x, y);
		}

		public static Vector Lerp(Vector value1, Vector value2, float amount)
		{
			return new Vector(
				value1.X + (value2.X - value1.X) * amount,
				value1.Y + (value2.Y - value1.Y) * amount);
		}

		public static float Dot(Vector a, Vector b) => a.X * b.X + a.Y * b.Y;
		#endregion

		#region Operators
		public static Vector operator +(Vector a, Vector b) => new Vector(a.X + b.X, a.Y + b.Y);

		public static Vector operator -(Vector a, Vector b) => new Vector(a.X - b.X, a.Y - b.Y);

		public static Vector operator *(Vector a, Vector b) => new Vector(a.X * b.X, a.Y * b.Y);

		public static Vector operator *(float f, Vector v) => new Vector(v.X * f, v.Y * f);

		public static Vector operator *(Vector v, float f) => new Vector(v.X * f, v.Y * f);

		public static Vector operator /(Vector a, Vector b) => new Vector(a.X / b.X, a.Y / b.Y);

		public static Vector operator /(Vector v, float f)
		{
			float invDiv = 1 / f;
			return new Vector(v.X * invDiv, v.Y * invDiv);
		}

		public static Vector operator -(Vector v) => new Vector(-v.X, -v.Y);

		public static bool operator ==(Vector left, Vector right) => left.Equals(right);

		public static bool operator !=(Vector left, Vector right) => !(left == right);
		#endregion

		#region Overloads
		public bool Equals(Vector other) => X == other.X && Y == other.Y;

		public override bool Equals(object obj) => obj is Vector v ? Equals(v) : false;

		public override int GetHashCode()
		{
			int hashCode = 1861411795;
			hashCode = hashCode * -1521134295 + X.GetHashCode();
			hashCode = hashCode * -1521134295 + Y.GetHashCode();
			return hashCode;
		}

		public override string ToString() => $"({X}, {Y})"; 
		#endregion
	}
}