using FNZ.Server.Model.World;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Effect;
using FNZ.Shared.Model.Effect.RealEffect;
using Unity.Mathematics;

namespace FNZ.Server.Services
{
	public class EffectAPI
	{
		public void SpawnRealEffectsAtPosition(ServerWorld world, string id, float2 position, float rotation = 0)
		{
			EffectData deathEffectData = DataBank.Instance.GetData<EffectData>(id);
			if (deathEffectData != null && deathEffectData.HasRealEffect() && deathEffectData.GetRealEffectDataType() == typeof(ProjectileEffectData))
				for (int i = 0; i < deathEffectData.repetitions; i++)
					world.RealEffectManager.SpawnProjectileServerAuthority(
						deathEffectData,
						(ProjectileEffectData)deathEffectData.RealEffectData,
						position,
						rotation,
						-1
					);

			if (deathEffectData != null && deathEffectData.HasRealEffect() && deathEffectData.GetRealEffectDataType() == typeof(ExplosionEffectData))
			{
				world.RealEffectManager.ExecuteExplosionRealEffect(
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