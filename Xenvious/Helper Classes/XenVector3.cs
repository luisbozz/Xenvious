using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Xenvious
{

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct XenVector3
	{
		public XenVector3(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public float X;
		public float Y;
		public float Z;

		public XenVector3 Zero => new XenVector3(0, 0, 0);
		public float Max => (X > Y) ? ((X > Z) ? X : Z) : ((Y > Z) ? Y : Z);
		public float Min => (X < Y) ? ((X < Z) ? X : Z) : ((Y < Z) ? Y : Z);
		public float EuclideanNorm => (float)Math.Sqrt(X * X + Y * Y + Z * Z);
		public float Square => X * X + Y * Y + Z * Z;
		public float Magnitude => (float)Math.Sqrt(SumComponentSqrs());
		public float Distance3D(XenVector3 v1, XenVector3 v2)
		{
			return
				(float)Math.Sqrt
				(
					(v1.X - v2.X) * (v1.X - v2.X) +
					(v1.Y - v2.Y) * (v1.Y - v2.Y) +
					(v1.Z - v2.Z) * (v1.Z - v2.Z)
				);
		}
		public float Distance3D(XenVector3 other)
		{
			return Distance3D(this, other);
		}

		public float Normalize()
		{
			float norm = (float)System.Math.Sqrt(X * X + Y * Y + Z * Z);
			float invNorm = 1.0f / norm;

			X *= invNorm;
			Y *= invNorm;
			Z *= invNorm;

			return norm;
		}
		public XenVector3 Inverse()
		{
			return new XenVector3(
				(X == 0) ? 0 : 1.0f / X,
				(Y == 0) ? 0 : 1.0f / Y,
				(Z == 0) ? 0 : 1.0f / Z);
		}
		public XenVector3 Abs()
		{
			return new XenVector3(Math.Abs(X), Math.Abs(Y), Math.Abs(Z));
		}
		public XenVector3 CrossProduct(XenVector3 vector1, XenVector3 vector2)
		{
			return new XenVector3(
				vector1.Y * vector2.Z - vector1.Z * vector2.Y,
				vector1.Z * vector2.X - vector1.X * vector2.Z,
				vector1.X * vector2.Y - vector1.Y * vector2.X);
		}
		public float DotProduct(XenVector3 vector1, XenVector3 vector2)
		{
			return vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;
		}


		public override string ToString()
		{
			return string.Format(System.Globalization.CultureInfo.InvariantCulture,
				"{0}, {1}, {2}", X, Y, Z);
		}
		public float[] ToArray()
		{
			return new float[3] { X, Y, Z };
		}

		public float this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: { return X; }
					case 1: { return Y; }
					case 2: { return Z; }
					default: throw new IndexOutOfRangeException($"Range is from 0 to 2");
				}
			}
		}

		public static XenVector3 operator +(XenVector3 vector, float value)
		{
			return new XenVector3(vector.X + value, vector.Y + value, vector.Z + value);
		}
		public static XenVector3 operator +(XenVector3 vector1, XenVector3 vector2)
		{
			return new XenVector3(vector1.X + vector2.X, vector1.Y + vector2.Y, vector1.Z + vector2.Z);
		}
		public XenVector3 Add(XenVector3 vector1, XenVector3 vector2)
		{
			return vector1 + vector2;
		}
		public XenVector3 Add(XenVector3 vector, float value)
		{
			return vector + value;
		}

		private XenVector3 SqrComponents(XenVector3 v1)
		{
			return
			(
				new XenVector3
				(
					v1.X * v1.X,
					v1.Y * v1.Y,
					v1.Z * v1.Z
				)
			);
		}
		private double SumComponentSqrs(XenVector3 v1)
		{
			XenVector3 v2 = SqrComponents(v1);
			return v2.SumComponents();
		}
		private double SumComponentSqrs()
		{
			return SumComponentSqrs(this);
		}
		private double SumComponents(XenVector3 v1)
		{
			return (v1.X + v1.Y + v1.Z);
		}
		private double SumComponents()
		{
			return SumComponents(this);
		}

		public static XenVector3 operator -(XenVector3 vector1, XenVector3 vector2)
		{
			return new XenVector3(vector1.X - vector2.X, vector1.Y - vector2.Y, vector1.Z - vector2.Z);
		}
		public XenVector3 Subtract(XenVector3 vector1, XenVector3 vector2)
		{
			return vector1 - vector2;
		}
		public static XenVector3 operator -(XenVector3 vector, float value)
		{
			return new XenVector3(vector.X - value, vector.Y - value, vector.Z - value);
		}
		public XenVector3 Subtract(XenVector3 vector, float value)
		{
			return vector - value;
		}

		public static XenVector3 operator *(XenVector3 vector1, XenVector3 vector2)
		{
			return new XenVector3(vector1.X * vector2.X, vector1.Y * vector2.Y, vector1.Z * vector2.Z);
		}
		public XenVector3 Multiply(XenVector3 vector1, XenVector3 vector2)
		{
			return vector1 * vector2;
		}
		public static XenVector3 operator *(XenVector3 vector, float factor)
		{
			return new XenVector3(vector.X * factor, vector.Y * factor, vector.Z * factor);
		}
		public XenVector3 Multiply(XenVector3 vector, float factor)
		{
			return vector * factor;
		}

		public static XenVector3 operator /(XenVector3 vector1, XenVector3 vector2)
		{
			return new XenVector3(vector1.X / vector2.X, vector1.Y / vector2.Y, vector1.Z / vector2.Z);
		}
		public XenVector3 Divide(XenVector3 vector1, XenVector3 vector2)
		{
			return vector1 / vector2;
		}
		public static XenVector3 operator /(XenVector3 vector, float factor)
		{
			return new XenVector3(vector.X / factor, vector.Y / factor, vector.Z / factor);
		}
		public XenVector3 Divide(XenVector3 vector, float factor)
		{
			return vector / factor;
		}

		public static bool operator ==(XenVector3 vector1, XenVector3 vector2)
		{
			return ((vector1.X == vector2.X) && (vector1.Y == vector2.Y) && (vector1.Z == vector2.Z));
		}
		public static bool operator !=(XenVector3 vector1, XenVector3 vector2)
		{
			return ((vector1.X != vector2.X) || (vector1.Y != vector2.Y) || (vector1.Z != vector2.Z));
		}

		public static bool operator <(XenVector3 v1, XenVector3 v2)
		{
			return v1.SumComponentSqrs() < v2.SumComponentSqrs();
		}
		public static bool operator <=(XenVector3 v1, XenVector3 v2)
		{
			return v1.SumComponentSqrs() <= v2.SumComponentSqrs();
		}

		public static bool operator >=(XenVector3 v1, XenVector3 v2)
		{
			return v1.SumComponentSqrs() >= v2.SumComponentSqrs();
		}
		public static bool operator >(XenVector3 v1, XenVector3 v2)
		{
			return v1.SumComponentSqrs() > v2.SumComponentSqrs();
		}


		public bool Equals(XenVector3 vector)
		{
			return ((vector.X == X) && (vector.Y == Y) && (vector.Z == Z));
		}
		public override bool Equals(object obj)
		{
			if (obj is XenVector3 vector3)
			{
				return Equals(vector3);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode();
		}
	}
}
