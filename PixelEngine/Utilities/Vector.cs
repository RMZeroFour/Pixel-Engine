using System;

namespace PixelEngine.Utilities
{
	public struct Vector : IEquatable<Vector>
	{
		public float X { get; private set; }
		public float Y { get; private set; }
		public float Z { get; private set; }

		public static Vector Zero => new Vector(0, 0, 0);
		public static Vector Unity => new Vector(1, 1, 1);
		public static Vector Right => new Vector(1, 0, 0);
		public static Vector Up => new Vector(0, 1, 0);
		public static Vector Forward => new Vector(0, 0, 1);

		public Vector(float a) : this(a, a, a) { }
		public Vector(float x, float y) : this(x, y, 0) { }
		public Vector(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		#region Calculations
		public float Magnitude() => Magnitude(this);
		public Vector Normalize() => Normalize(this);

		public static float Distance(Vector a, Vector b)
		{
			float dx = a.X - b.X;
			float dy = a.Y - b.Y;
			float dz = a.Z - b.Z;

			float length = dx * dx + dy * dy + dz * dz;

			return (float)Math.Sqrt(length);
		}

		public static Vector Normalize(Vector v)
		{
			float length = v.X * v.X + v.Y * v.Y + v.Z * v.Z;
			float invDiv = (float)(1 / Math.Sqrt(length));

			return new Vector(v.X * invDiv, v.Y * invDiv, v.Z * invDiv);
		}

		public static Vector Reflect(Vector v, Vector normal)
		{
			float dot = v.X * normal.X + v.Y * normal.Y + normal.Z * normal.Z;
			dot *= 2;
			return new Vector(v.X - dot * normal.X, v.Y - dot * normal.Y, v.Z - dot * normal.Z);
		}

		public static Vector Constrain(Vector v, Vector min, Vector max)
		{
			float x = v.X;
			x = (x > max.X) ? max.X : x;
			x = (x < min.X) ? min.X : x;

			float y = v.Y;
			y = (y > max.Y) ? max.Y : y;
			y = (y < min.Y) ? min.Y : y;

			float z = v.Y;
			z = (z > max.Z) ? max.Z : z;
			z = (z < min.Z) ? min.Z : z;

			return new Vector(x, y, z);
		}

		public static Vector Lerp(Vector a, Vector b, float amount)
		{
			return new Vector(a.X + (b.X - a.X) * amount,
				a.Y + (b.Y - a.Y) * amount,
				a.Z + (b.Z - a.Z) * amount);
		}

		public static Vector Cross(Vector a, Vector b)
		{
			float x = a.Y * b.Z - b.Y * a.Z;
			float y = a.X * b.Z - b.X * a.Z;
			float z = a.X * b.Y - b.X * a.Y;

			return new Vector(x, y, z);
		}

		public static float Magnitude(Vector v)
		{
			float length = v.X * v.X + v.Y * v.Y + v.Z * v.Z;
			return (float)Math.Sqrt(length);
		}

		public static float Dot(Vector a, Vector b) => (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z);

		public static float Angle(Vector a, Vector b) => (float)Math.Acos(Dot(a, b) / (Magnitude(a) * Magnitude(b)));

		public static bool Intersect(Vector s1, Vector e1, Vector s2, Vector e2)
		{
			bool OnSegment(Vector p, Vector q, Vector r)
			{
				return (q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
						q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y));
			}

			int Orientation(Vector p, Vector q, Vector r)
			{
				float val = (q.Y - p.Y) * (r.X - q.X) - (q.X - p.X) * (r.Y - q.Y);

				return (val != 0) ? 0 :
					(val > 0) ? 1 : 2;
			}

			int o1 = Orientation(s1, e1, s2);
			int o2 = Orientation(s1, e1, e2);
			int o3 = Orientation(s2, e2, s1);
			int o4 = Orientation(s2, e2, e1);

			if (o1 != o2 && o3 != o4)
				return true;

			if (o1 == 0 && OnSegment(s1, s2, e1))
				return true;

			if (o2 == 0 && OnSegment(s1, e2, e1))
				return true;

			if (o3 == 0 && OnSegment(s2, s1, e2))
				return true;

			if (o4 == 0 && OnSegment(s2, e1, e2))
				return true;

			return false;
		}
		#endregion

		#region Operators
		public static Vector operator +(Vector a, float b) => new Vector(a.X + b, a.Y + b, a.Z + b);
		public static Vector operator +(Vector a, Vector b) => new Vector(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

		public static Vector operator -(Vector a, float b) => new Vector(a.X - b, a.Y - b, a.Z - b);
		public static Vector operator -(Vector a, Vector b) => new Vector(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

		public static Vector operator *(Vector a, Vector b) => new Vector(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
		public static Vector operator *(Vector v, float f) => new Vector(v.X * f, v.Y * f, v.Z * f);
		public static Vector operator *(float f, Vector v) => v * f;

		public static Vector operator /(Vector a, Vector b) => new Vector(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
		public static Vector operator /(Vector v, float f)
		{
			float invDiv = 1 / f;
			return new Vector(v.X * invDiv, v.Y * invDiv, v.Z * invDiv);
		}

		public static Vector operator -(Vector v) => new Vector(-v.X, -v.Y, -v.Z);

		public static bool operator ==(Vector left, Vector right) => left.Equals(right);

		public static bool operator !=(Vector left, Vector right) => !(left == right);
		#endregion

		#region Overloads
		public bool Equals(Vector other) => X == other.X && Y == other.Y && Z == other.Z;

		public override bool Equals(object obj) => obj is Vector v ? Equals(v) : false;

		public override int GetHashCode()
		{
			int hashCode = 1861411795;
			hashCode = hashCode * -1521134295 + X.GetHashCode();
			hashCode = hashCode * -1521134295 + Y.GetHashCode();
			hashCode = hashCode * -1521134295 + Z.GetHashCode();
			return hashCode;
		}

		public override string ToString() => $"({X}, {Y}, {Z})"; 
		#endregion
	}
}