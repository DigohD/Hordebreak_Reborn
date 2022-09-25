using Unity.Mathematics;

namespace FNZ.Shared.Utils.CollisionUtils
{
	public struct FNECircle
	{
		public float2 Position;
		public float Radius;

		public FNECircle(float2 position, float radius)
		{
			Position = position;
			Radius = radius;
		}
	}
}