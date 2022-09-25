using FNZ.Server.Model.Entity.Components;
using FNZ.Server.Model.Entity.Components.Inventory;
using FNZ.Shared.Constants;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Items.Components;
using Lidgren.Network;
using Unity.Mathematics;

namespace FNZ.Server.Services
{
	public class ConsumableAPI
	{
		public static void Consume(FNEEntity player, Item item, NetIncomingMessage incMsg, InventoryComponentServer ics)
		{
			var consumable = item.GetComponent<ItemConsumableComponent>().Data;
			var statCompServer = player.GetComponent<StatComponentServer>();

			switch (consumable.buff)
			{
				case ConsumableBuff.HEALTH_GAIN:
					statCompServer.Heal(consumable.amount);
					GameServer.NetAPI.Effect_SpawnEffect_BAR(consumable.effectRef, new float2(player.Position.x, player.Position.y), player.RotationDegrees);
					GameServer.NetAPI.Entity_UpdateComponent_STC(statCompServer, incMsg.SenderConnection);

					if (item.amount == 1) { ics.RemoveItem(item); }
					else { item.amount -= 1; }
					break;

				case ConsumableBuff.HEALTH_LOSS:
					statCompServer.Server_ApplyDamage(consumable.amount, DamageTypesConstants.TRUE_DAMAGE);
					GameServer.NetAPI.Effect_SpawnEffect_BAR(consumable.effectRef, new float2(player.Position.x, player.Position.y), player.RotationDegrees);
					GameServer.NetAPI.Entity_UpdateComponent_STC(statCompServer, incMsg.SenderConnection);

					if (item.amount == 1) { ics.RemoveItem(item); }
					else { item.amount -= 1; }
					break;


				default:
					break;
			}
		}
	}
}