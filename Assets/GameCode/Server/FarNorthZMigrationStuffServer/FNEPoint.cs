using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Server.FarNorthZMigrationStuff
{
	public class FNEPoint
	{
		public int x;
		public int y;

		public FNEPoint()
		{
			x = 0;
			y = 0;
		}

		public FNEPoint(int iX, int iY)
		{
			this.x = iX;
			this.y = iY;
		}

		public FNEPoint(FNEPoint b)
		{
			x = b.x;
			y = b.y;
		}

		public FNEPoint(Vector2 v)
		{
			x = (int)v.x;
			y = (int)v.y;
		}

		public Vector2 ToVector2()
		{
			return new Vector2(x, y);
		}

		public float2 ToFloat2()
		{
			return new float2(x, y);
		}

		public override int GetHashCode()
		{
			return x ^ y;
		}

		public override bool Equals(System.Object obj)
		{
			// Unlikely to compare incorrect type so removed for performance
			// if (!(obj.GetType() == typeof(PathFind.Point)))
			//     return false;
			FNEPoint p = (FNEPoint)obj;

			if (ReferenceEquals(null, p))
			{
				return false;
			}

			// Return true if the fields match:
			return (x == p.x) && (y == p.y);
		}

		public bool Equals(FNEPoint p)
		{
			if (ReferenceEquals(null, p))
			{
				return false;
			}
			// Return true if the fields match:
			return (x == p.x) && (y == p.y);
		}

		public static bool operator ==(FNEPoint a, FNEPoint b)
		{
			// If both are null, or both are same instance, return true.
			if (System.Object.ReferenceEquals(a, b))
			{
				return true;
			}
			if (ReferenceEquals(null, a))
			{
				return false;
			}
			if (ReferenceEquals(null, b))
			{
				return false;
			}
			// Return true if the fields match:
			return a.x == b.x && a.y == b.y;
		}

		public static bool operator !=(FNEPoint a, FNEPoint b)
		{
			return !(a == b);
		}

		public FNEPoint Set(int iX, int iY)
		{
			this.x = iX;
			this.y = iY;
			return this;
		}

	}
}