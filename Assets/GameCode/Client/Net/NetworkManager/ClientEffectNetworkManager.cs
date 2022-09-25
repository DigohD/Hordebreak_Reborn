using FNZ.Client.Systems;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Effect;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Items.Components;
using FNZ.Shared.Net;
using FNZ.Shared.Utils;
using Lidgren.Network;
using Unity.Mathematics;
using UnityEngine.Profiling;

namespace FNZ.Client.Net.NetworkManager
{
	public class ClientEffectNetworkManager : INetworkManager
	{
		public ClientEffectNetworkManager()
		{
			GameClient.NetConnector.Register(NetMessageType.SPAWN_EFFECT, OnSpawnEffect);
			GameClient.NetConnector.Register(NetMessageType.SPAWN_PROJECTILE, OnSpawnProjectile);
			//GameClient.NetConnector.Register(NetMessageType.SPAWN_PROJECTILE_BATCH, OnSpawnProjectileBatch);
		}

		private void OnSpawnEffect(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			ushort idCode = incMsg.ReadUInt16();
			float2 location = new float2(incMsg.ReadFloat(), incMsg.ReadFloat());
			float rotation = FNEUtil.UnpackShortToFloat(incMsg.ReadUInt16());

			var effectId = IdTranslator.Instance.GetId<EffectData>(idCode);

			GameClient.ViewAPI.SpawnEffect(
				effectId, 
				location, 
				rotation, 
				false, 
				true,
				null,
				null
			);

			if (DataBank.Instance.GetData<EffectData>(effectId).hasBlood)
			{
				GameClient.ECS_ClientWorld.GetExistingSystem<BloodSplatterSystem>().AddBloodSplatterToSpawnQueue(new BloodSplatterSpawnData
				{
					Position = location,
					Rotation = math.radians(FNERandom.GetRandomFloatInRange(0.0f, 360.0f)),
					LifeTime = 600.0f, //in seconds, so 10 min right now
					Scale = FNERandom.GetRandomFloatInRange(0.15f, 0.3f)
				});
			}
		}

		private void OnSpawnProjectile(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			ushort idCode = incMsg.ReadUInt16();
			float2 location = new float2(incMsg.ReadFloat(), incMsg.ReadFloat());
			float rotation = FNEUtil.UnpackShortToFloat(incMsg.ReadUInt16());
            int ownerNetId = incMsg.ReadInt32();

			byte modCount = 0;
			string[] modItemIds = null;
			ItemWeaponModComponentData[] mods = null;
			if (incMsg.ReadBoolean())
			{
				modCount = incMsg.ReadByte();
				modItemIds = new string[modCount];
				mods = new ItemWeaponModComponentData[modCount];
				for (int i = 0; i < modCount; i++)
				{
					var modId = incMsg.ReadUInt16();
					if (modId == 0)
						continue;
					var itemId = IdTranslator.Instance.GetId<ItemData>(modId);
					modItemIds[i] = itemId;
					var data = DataBank.Instance.GetData<ItemData>(itemId);
					var modData = (ItemWeaponModComponentData)data.GetComponentData<ItemWeaponModComponentData>();
					mods[i] = modData;
				}
			}

			var effectId = IdTranslator.Instance.GetId<EffectData>(idCode);

			GameClient.ViewAPI.SpawnEffect(
				effectId, 
				location, 
				rotation, 
				false, 
				true,
				modItemIds,
				mods,
				-1, 
				ownerNetId
			);
		}

		/*private void OnSpawnProjectileBatch(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			byte amount = incMsg.ReadByte();
			ushort idCode = incMsg.ReadUInt16();

			float2[] locations = new float2[amount];
			float[] rotationZs = new float[amount];
			for (int i = 0; i < amount; i++)
			{
				locations[i].x = incMsg.ReadFloat();
				locations[i].y = incMsg.ReadFloat();
				rotationZs[i] = FNEUtil.UnpackShortToFloat(incMsg.ReadUInt16());
			}

			var effectId = IdTranslator.Instance.GetId<EffectData>(idCode);

			for (int i = 0; i < amount; i++)
				GameClient.ViewAPI.SpawnEffect(effectId, locations[i], rotationZs[i], false, true);
		}*/
	}
}