using FNZ.Shared.Model.Entity.Components.Excavatable;
using FNZ.Shared.Utils;

namespace FNZ.Server.Model.Entity.Components.Excavatable
{
	public class ExcavatableComponentServer : ExcavatableComponentShared
	{
		public void Hit()
		{
			hitsRemaining--;
			if (hitsRemaining == 0)
			{
				GameServer.NetAPI.Effect_SpawnEffect_BAR(Data.deathEffectRef, ParentEntity.Position + new Unity.Mathematics.float2(0.5f, 0.5f), 0);

				if (Data.transformsOnExcavation && !string.IsNullOrEmpty(Data.transformedEntityRef))
				{
					GameServer.EntityFactory.QueueEntityForReplacement(ParentEntity, Data.transformedEntityRef);
				}
				else
				{
					GameServer.EntityAPI.NetDestroyEntityImmediate(ParentEntity);
				}
			}
			else
			{
				GameServer.NetAPI.Entity_UpdateComponent_BAR(this);
				GameServer.NetAPI.Effect_SpawnEffect_BAR(Data.hitEffectRef, ParentEntity.Position + FNEUtil.HalfFloat2, 0);
			}
		}
	}
}
