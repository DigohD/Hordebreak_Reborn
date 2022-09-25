using FNZ.Shared.Model;
using FNZ.Shared.Model.Effect;
using FNZ.Shared.Model.Effect.RealEffect;
using Unity.Mathematics;

namespace FNZ.Server.Services
{
	public class EffectAPI
	{
		public void SpawnRealEffectsAtPosition(string id, float2 position, float rotation = 0)
		{
			EffectData deathEffectData = DataBank.Instance.GetData<EffectData>(id);
			if (deathEffectData != null && deathEffectData.HasRealEffect() && deathEffectData.GetRealEffectDataType() == typeof(ProjectileEffectData))
				for (int i = 0; i < deathEffectData.repetitions; i++)
					GameServer.World.RealEffectManager.SpawnProjectileServerAuthority(
						deathEffectData,
						(ProjectileEffectData)deathEffectData.RealEffectData,
						position,
						rotation,
						-1
					);

			if (deathEffectData != null && deathEffectData.HasRealEffect() && deathEffectData.GetRealEffectDataType() == typeof(ExplosionEffectData))
			{
				GameServer.World.RealEffectManager.ExecuteExplosionRealEffect(
					(ExplosionEffectData)deathEffectData.RealEffectData,
					position
				);
			}
		}

		public void SpawnEffectOnEntity(string id, int entityNetId)
		{

		}
	}
}