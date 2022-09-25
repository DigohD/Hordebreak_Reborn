using FNZ.Shared.Model.Items.Components;
using System.Collections.Generic;
using Unity.Mathematics;

namespace FNZ.Client.Net.API
{

	public partial class ClientNetworkAPI
	{
		public void CMD_Effect_Spawn_Projectile(string effectId, float2 position, float rotationZDegrees, string[] modItemIds)
		{
			var message = m_EffectMessageFactory.CreateSpawnProjectileMessage(effectId, position, rotationZDegrees, modItemIds);
			Command(message);
		}

		public void CMD_Effect_Spawn_ProjectileBatch(string effectId, List<float2> positions, List<float> rotationZDegrees, string[] modItemIds)
		{
			var message = m_EffectMessageFactory.CreateSpawnProjectileMessageBatch(effectId, positions, rotationZDegrees, modItemIds);
			Command(message);
		}
	}
}