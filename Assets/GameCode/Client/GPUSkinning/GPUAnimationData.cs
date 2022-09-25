using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace FNZ.Client.GPUSkinning 
{
    [MaterialProperty("AnimationTextureCoordinate", MaterialPropertyFormat.Float4)]
	public struct AnimationTextureCoordinate : IComponentData
    {
        public float4 Coordinate;
    }

	public struct GPUAnimationState : IComponentData
	{
		public float Time;
		public int AnimationClipIndex;

		public BlobAssetReference<BakedAnimationClipSet> AnimationClipSet;
	}

	public struct BakedAnimationClipSet
	{
		public BlobArray<BakedAnimationClip> Clips;
	}

	public struct BakedAnimationClip
	{
		public bool IsValid;
		private readonly float m_TextureOffset;
		private readonly float m_TextureRange;
		private readonly float m_OnePixelOffset;
		private readonly float m_TextureWidth;
		private readonly float m_OneOverTextureWidth;
		private readonly float m_OneOverPixelOffset;

		private readonly float m_AnimationLength;
		private bool m_Looping;

		public BakedAnimationClip(AnimationTextures animTextures, AnimationClipData clipData)
		{
			if (clipData == null)
			{
				IsValid = false;
				m_TextureOffset = 0;
				m_TextureRange = 0;
				m_OnePixelOffset = 0;
				m_TextureWidth = 0;
				m_OneOverTextureWidth = 0;
				m_OneOverPixelOffset = 0;
				m_AnimationLength = 0;
				m_Looping = false;
			}
			else
			{
				var onePixel = 1f / animTextures.Animation0.width;
				var start = (float)clipData.PixelStart / animTextures.Animation0.width;
				var end = (float)clipData.PixelEnd / animTextures.Animation0.width;

				m_TextureOffset = start;
				m_TextureRange = end - start;
				m_OnePixelOffset = onePixel;
				m_TextureWidth = animTextures.Animation0.width;
				m_OneOverTextureWidth = 1.0F / m_TextureWidth;
				m_OneOverPixelOffset = 1.0F / m_OnePixelOffset;

				m_AnimationLength = clipData.Clip.length;
				m_Looping = clipData.Clip.wrapMode == WrapMode.Loop;

				IsValid = true;
			}
		}

		public float3 ComputeCoordinate(float normalizedTime)
		{
			var texturePosition = normalizedTime * m_TextureRange + m_TextureOffset;
			var lowerPixelFloor = math.floor(texturePosition * m_TextureWidth);

			var lowerPixelCenter = lowerPixelFloor * m_OneOverTextureWidth;
			var upperPixelCenter = lowerPixelCenter + m_OnePixelOffset;
			var lerpFactor = (texturePosition - lowerPixelCenter) * m_OneOverPixelOffset;

			return new float3(lowerPixelCenter, upperPixelCenter, lerpFactor);
		}

		public float ComputeNormalizedTime(float time)
		{
			return Mathf.Repeat(time, m_AnimationLength) / m_AnimationLength;
			//if (Looping)
			//	return Mathf.Repeat(time, AnimationLength) / AnimationLength;
			//else
			//	return math.saturate(time / AnimationLength);
		}
	}
}