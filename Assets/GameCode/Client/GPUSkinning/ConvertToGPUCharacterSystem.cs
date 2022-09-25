using Unity.Entities;
using UnityEngine;

namespace FNZ.Client.GPUSkinning 
{
	[UpdateInGroup(typeof(GameObjectBeforeConversionGroup))]
	public class ConvertToGPUCharacterSystem : GameObjectConversionSystem
	{
		protected override void OnUpdate()
		{
			foreach (var system in World.Systems)
			{
				if (system.GetType().Name == "SkinnedMeshRendererConversion")
				{
					system.Enabled = false;
				}
			}

			Entities.ForEach((ConvertToGPUCharacter character) =>
			{
				GPUAnimationCharacterUtility.AddCharacterComponents(
					DstEntityManager, 
					GetPrimaryEntity(character), 
					character.gameObject, 
					character.AnimationClips,
					character.Id
				);
			});
		}
	}
}