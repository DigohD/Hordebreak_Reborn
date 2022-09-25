using FNZ.Client.GPUSkinning;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace FNZ.Client.Systems.Hordes
{
	[UpdateInGroup(typeof(UpdatePresentationSystemGroup))]
	public class UpdateAnimationTextureCoordinateSystem : SystemBase
	{
		protected override void OnUpdate()
		{
			if (Camera.main is null) return;
			
			var cullingReferencePosition = (float3)(GameClient.LocalPlayerView != null 
				? GameClient.LocalPlayerView.transform.position : Camera.main.transform.position);

			const float maxCullingDistance = 32.0f;

			Dependency = Entities
				.WithName("ComputeAnimationTextureCoordinatesJob")
				.WithBurst()
				.ForEach((
					ref AnimationTextureCoordinate textureCoordinate, 
					ref Translation position, 
					in GPUAnimationState animationState) =>
				{
					var distance = math.distance(position.Value, cullingReferencePosition);

					if (!(distance < maxCullingDistance)) return;
					ref var clips = ref animationState.AnimationClipSet.Value.Clips;

					if ((uint) animationState.AnimationClipIndex >= (uint) clips.Length) return;
					
					var clip = clips[animationState.AnimationClipIndex];
					if (!clip.IsValid) return;
					
					var normalizedTime = clip.ComputeNormalizedTime(animationState.Time);
					textureCoordinate.Coordinate.xyz = clip.ComputeCoordinate(normalizedTime);

				}).ScheduleParallel(Dependency);
		}
	}
}