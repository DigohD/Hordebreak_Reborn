using FNZ.Shared.Model.Entity;

namespace FNZ.Server.Services
{
	public class EntityEventAPI
	{
		public void EntityInteractionEvent(int entityNetId, FNEEntity interactingEntity)
		{
			// Code for sending an interact event from an entity to another entity.
		}

		public void EntityTriggerEvent(int entityNetId)
		{
			// Code for sending a trigger event to an entity.
		}

		public void EntityKillEvent(int entityNetId)
		{
			// Code for killing an entity.
		}

		public void EntityDamageEvent(int entityNetId, int damage)
		{
			// Code for damaging an entity.
		}
	}
}