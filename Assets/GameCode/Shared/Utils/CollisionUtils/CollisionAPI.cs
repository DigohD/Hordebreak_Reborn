using UnityEngine;

namespace FNZ.Shared.Utils.CollisionUtils
{
	public static class CollisionAPI
	{
		public static bool Intersects(FNECircle circle, FNERectangle rect)
		{
			Vector2 rectCenter = rect.GetCenterPoint();

			float circleDistanceX = Mathf.Abs(circle.Position.x - rectCenter.x);
			float circleDistanceY = Mathf.Abs(circle.Position.y - rectCenter.y);

			if (circleDistanceX > (rect.Width / 2.0f + circle.Radius)) { return false; }
			if (circleDistanceY > (rect.Height / 2.0f + circle.Radius)) { return false; }

			if (circleDistanceX <= (rect.Width / 2.0f)) { return true; }
			if (circleDistanceY <= (rect.Height / 2.0f)) { return true; }

			float cornerDistanceSQ = Mathf.Pow(circleDistanceX - rect.Width / 2.0f, 2)
				+ Mathf.Pow(circleDistanceY - rect.Height / 2.0f, 2);

			return (cornerDistanceSQ <= Mathf.Pow(circle.Radius, 2));
		}
	}
}