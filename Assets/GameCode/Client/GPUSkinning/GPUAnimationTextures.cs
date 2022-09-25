using System;
using UnityEngine;

namespace FNZ.Client.GPUSkinning 
{
	public struct AnimationTextures : IEquatable<AnimationTextures>
	{
		public Texture2D Animation0;
		public Texture2D Animation1;
		public Texture2D Animation2;

		public bool Equals(AnimationTextures other)
		{
			return Animation0 == other.Animation0 && Animation1 == other.Animation1 && Animation2 == other.Animation2;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (ReferenceEquals(Animation0, null) ? 0 : Animation0.GetHashCode());
				hashCode = (hashCode * 397) ^ (ReferenceEquals(Animation1, null) ? 0 : Animation1.GetHashCode());
				hashCode = (hashCode * 397) ^ (ReferenceEquals(Animation2, null) ? 0 : Animation2.GetHashCode());
				return hashCode;
			}
		}
	}
}