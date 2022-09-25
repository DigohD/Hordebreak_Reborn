using Unity.Mathematics;

namespace FNZ.Shared.Utils.CollisionUtils
{

	public struct FNERectangle
	{
		public float2 Position;

		public float Width;
		public float Height;

		public FNERectangle(float x, float y, float width, float height)
		{
			Position = new float2(x, y);
			this.Width = width;
			this.Height = height;
		}

		public float2 GetCenterPoint()
		{
			return new float2
			{
				x = Position.x + Width / 2.0f,
				y = Position.y + Height / 2.0f
			};
		}
	}
}