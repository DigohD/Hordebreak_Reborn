using FNZ.Server.FarNorthZMigrationStuff;
using FNZ.Server.Utils;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Effect;
using FNZ.Shared.Model.Effect.RealEffect;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Items.Components;
using FNZ.Shared.Net;
using FNZ.Shared.Utils;
using Lidgren.Network;
using Unity.Mathematics;

namespace FNZ.Server.Net.NetworkManager
{
	public class ServerEffectNetworkManager : INetworkManager
	{
		public ServerEffectNetworkManager()
		{
			GameServer.NetConnector.Register(NetMessageType.SPAWN_PROJECTILE, OnClientSpawnProjectile);
            GameServer.NetConnector.Register(NetMessageType.SPAWN_PROJECTILE_BATCH, OnClientSpawnProjectileBatch);
        }

		private void OnClientSpawnProjectile(ServerNetworkConnector net, NetIncomingMessage incMsg)
		{
			var effectId = IdTranslator.Instance.GetId<EffectData>(incMsg.ReadUInt16());

			var position = new float2(incMsg.ReadFloat(), incMsg.ReadFloat());

			var rotationZdegrees = FNEUtil.UnpackShortToFloat(incMsg.ReadUInt16());
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
                    var idCode = incMsg.ReadUInt16();
                    if (idCode == 0)
                        continue;
                    var itemId = IdTranslator.Instance.GetId<ItemData>(idCode);
                    modItemIds[i] = itemId;
                    var data = DataBank.Instance.GetData<ItemData>(itemId);
                    var modData = (ItemWeaponModComponentData)data.GetComponentData<ItemWeaponModComponentData>();
                    mods[i] = modData;
                }
            }

            SpawnClientProjectile(
                effectId,
                position,
                rotationZdegrees,
                modItemIds,
                mods,
                incMsg.SenderConnection
            );
        }

        private void OnClientSpawnProjectileBatch(ServerNetworkConnector net, NetIncomingMessage incMsg)
        {
            int count = incMsg.ReadByte();
            var effectId = IdTranslator.Instance.GetId<EffectData>(incMsg.ReadUInt16());

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
                    var idCode = incMsg.ReadUInt16();
                    if (idCode == 0)
                        continue;
                    var itemId = IdTranslator.Instance.GetId<ItemData>(idCode);
                    modItemIds[i] = itemId;
                    var data = DataBank.Instance.GetData<ItemData>(itemId);
                    var modData = (ItemWeaponModComponentData)data.GetComponentData<ItemWeaponModComponentData>();
                    mods[i] = modData;
                }
            }

            for (int i = 0; i < count; i++)
            {
                var position = new float2(incMsg.ReadFloat(), incMsg.ReadFloat());
                var rotationZdegrees = FNEUtil.UnpackShortToFloat(incMsg.ReadUInt16());

                SpawnClientProjectile(
                    effectId,
                    position,
                    rotationZdegrees,
                    modItemIds,
                    mods,
                    incMsg.SenderConnection
                );
            }
        }

        private void SpawnClientProjectile(
            string effectId,
            float2 position,
            float rotationZdegrees,
            string[] modItemIds,
            ItemWeaponModComponentData[] mods,
            NetConnection senderConnection
        )
        {
            var effectData = DataBank.Instance.GetData<EffectData>(effectId);

            if (effectData.enemyAlertDistance > 0)
            {
                FlowFieldUtility.QueueFlowFieldForSpawn(
                    position, 
                    effectData.enemyAlertDistance, 
                    FlowFieldType.Sound
                );
            }
            
            GameServer.NetAPI.Effect_SpawnProjectile_BOR(
                effectId,
                position,
                rotationZdegrees,
                modItemIds,
                senderConnection
            );

            GameServer.World.RealEffectManager.SpawnProjectileClientAuthority(
                effectData,
                (ProjectileEffectData) effectData.RealEffectData,
                position,
                rotationZdegrees,
                mods,
                senderConnection
            );
        }
	}
}