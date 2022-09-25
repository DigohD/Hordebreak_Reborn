using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Shared.Utils
{

	public static class float2Extensions
	{
		public static float Magnitude(this float2 f)
		{
			return Mathf.Sqrt((f.x * f.x) + (f.y * f.y));
		}

		public static float2 Normalize(this float2 f)
		{
			return Mathf.Sqrt((f.x * f.x) + (f.y * f.y));
		}

	}
}