using FNZ.Server.Model.Entity.Components.AI;
using FNZ.Server.Model.World;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Effect;
using FNZ.Shared.Model.Items.Components;
using FNZ.Shared.Model.SFX;
using FNZ.Shared.Utils;
using Lidgren.Network;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Server.Net.API
{
	public partial class ServerNetworkAPI
	{
		public void Effect_SpawnEffect_BAR(string effectId, float2 location, float rotation)
		{
			EffectData effectData = DataBank.Instance.GetData<EffectData>(effectId);
			SFXData sfxData = null;

			if (effectData.alertsEnemies)
			{
				foreach (var tile in GameServer.World.GetSurroundingTilesInRadius((int2)location, effectData.enemyAlertDistance))
				{
					var enemies = GameServer.World.GetTileEnemies(tile);
					if (enemies == null || enemies.Count == 0) continue;

					foreach (var enemy in enemies)
						enemy.GetComponent<AIComponentServer>().HeardSound(location);
				}
			}

			if (!string.IsNullOrEmpty(effectData.sfxRef))
				sfxData = DataBank.Instance.GetData<SFXData>(effectData.sfxRef);

			var connectedPlayers = GameServer.NetConnector.GetConnectedClientConnections();
			var relevantPlayers = new List<NetConnection>();
			float soundDistance = DefaultValues.DEFAULT_AUDIOSOURCE_MAXDISTANCE;
			if (sfxData != null)
				soundDistance = sfxData.distance != 0 ? sfxData.distance : DefaultValues.DEFAULT_AUDIOSOURCE_MAXDISTANCE;

			var locationChunk = GameServer.World.GetWorldChunk<ServerWorldChunk>(location);
			var chunkManager = GameServer.ChunkManager;

			foreach (var player in connectedPlayers)
			{
				var entity = GameServer.NetConnector.GetPlayerFromConnection(player);

				if (chunkManager.GetPlayerChunkState(player).CurrentlyLoadedChunks.Contains(locationChunk) ||
					Vector3.Distance(new Vector3(entity.Position.x, entity.Position.y), new Vector3(location.x, location.y)) <= soundDistance)
				{
					relevantPlayers.Add(player);
				}
			}

			if (relevantPlayers.Count > 0)
			{
				var message = m_EffectMessageFactory.CreateEffectMessage(effectId, location, rotation);
				Broadcast_To_Clients(message, relevantPlayers);
			}
		}

		public void Effect_SpawnEffect_BOR(
			string effectId, 
			float2 location, 
			float rotation,
			NetConnection toExclude
		)
		{
			EffectData effectData = DataBank.Instance.GetData<EffectData>(effectId);
			SFXData sfxData = null;

			if (!string.IsNullOrEmpty(effectData.sfxRef))
				sfxData = DataBank.Instance.GetData<SFXData>(effectData.sfxRef);

			var connectedPlayers = GameServer.NetConnector.GetConnectedClientConnections();
			var relevantPlayers = new List<NetConnection>();
			float soundDistance = DefaultValues.DEFAULT_AUDIOSOURCE_MAXDISTANCE;
			if (sfxData != null)
				soundDistance = sfxData.distance != 0 ? sfxData.distance : DefaultValues.DEFAULT_AUDIOSOURCE_MAXDISTANCE;

			var locationChunk = GameServer.World.GetWorldChunk<ServerWorldChunk>(location);
			var chunkManager = GameServer.ChunkManager;

			foreach (var player in connectedPlayers)
			{
				var entity = GameServer.NetConnector.GetPlayerFromConnection(player);

				if (player.RemoteUniqueIdentifier != toExclude.RemoteUniqueIdentifier &&
					(chunkManager.GetPlayerChunkState(player).CurrentlyLoadedChunks.Contains(locationChunk) ||
					Vector3.Distance(new Vector3(entity.Position.x, entity.Position.y), new Vector3(location.x, location.y)) <= soundDistance))
				{
					relevantPlayers.Add(player);
				}
			}

			if (relevantPlayers.Count > 0)
			{
				var message = m_EffectMessageFactory.CreateEffectMessage(effectId, location, rotation);
				Broadcast_To_Clients(message, relevantPlayers);
			}
		}

        public void Effect_SpawnProjectile_BOR(
			string effectId, 
			float2 location, 
			float rotation,
			string[] modItemIds,
			NetConnection ownerConnection
		)
        {
            EffectData effectData = DataBank.Instance.GetData<EffectData>(effectId);
            SFXData sfxData = null;

            if (effectData.sfxRef != string.Empty)
                sfxData = DataBank.Instance.GetData<SFXData>(effectData.sfxRef);

            var connectedPlayers = GameServer.NetConnector.GetConnectedClientConnections();
            var relevantPlayers = new List<NetConnection>();
            float soundDistance = DefaultValues.DEFAULT_AUDIOSOURCE_MAXDISTANCE;
            if (sfxData != null)
                soundDistance = sfxData.distance != 0 ? sfxData.distance : DefaultValues.DEFAULT_AUDIOSOURCE_MAXDISTANCE;

            var locationChunk = GameServer.World.GetWorldChunk<ServerWorldChunk>(location);
            var chunkManager = GameServer.ChunkManager;

            foreach (var player in connectedPlayers)
            {
                var entity = GameServer.NetConnector.GetPlayerFromConnection(player);

                if (player.RemoteUniqueIdentifier != ownerConnection.RemoteUniqueIdentifier &&
                    (chunkManager.GetPlayerChunkState(player).CurrentlyLoadedChunks.Contains(locationChunk) ||
                    Vector3.Distance(new Vector3(entity.Position.x, entity.Position.y), new Vector3(location.x, location.y)) <= soundDistance))
                {
                    relevantPlayers.Add(player);
                }
            }

            if (relevantPlayers.Count > 0)
            {
                var ownerNetId = GameServer.NetConnector.GetPlayerFromConnection(ownerConnection).NetId;
                var message = m_EffectMessageFactory.CreateSpawnProjectileMessage(effectId, location, rotation, modItemIds, ownerNetId);
                Broadcast_To_Clients(message, relevantPlayers);
            }
        }
    }
}