using System.Collections;
using System.Collections.Generic;
using FNZ.Server.Model.Entity.Components.Name;
using FNZ.Server.Model.Entity.Components.Player;
using FNZ.Server.Model.World.Blueprint;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.World;
using FNZ.Shared.Model.World.Site;
using FNZ.Shared.Utils;
using Lidgren.Network;
using UnityEngine;

namespace FNZ.Server.Model.World 
{
	public class ServerMapManager : MapManager
	{

		private const byte RevealPadding = 5;
		private const byte DetectRadius = 4;

		private ServerWorld _world;
		
		public ServerMapManager(ServerWorld world, int widthInChunks, int heightInChunks) : base(widthInChunks, heightInChunks)
		{
			_world = world;
		}

		public void Tick()
		{
			CalculateRevealedSites();
		}

		public void VisitChunk(ServerWorldChunk chunk)
		{
			var cX = chunk.ChunkX;
			var cY = chunk.ChunkY;

			var siteMap = _world.GetSiteMetaData();
			
			var width = _world.WIDTH;
			var height = _world.HEIGHT;
			
			var anyChanges = false;
			
			if (!visitedChunks[cX + cY * width])
			{
				visitedChunks[cX + cY * width] = true;
				GameServer.NetAPI.World_ChunkMapUpdate_BA(chunk);
				
				for (
					var x = (cX - DetectRadius < 0) ? 0 : cX - DetectRadius;
					x < (cX + DetectRadius > width ? width : cX + DetectRadius);
					x++
				)
				{
					for (
						var y = (cY - DetectRadius < 0) ? 0 : cY - DetectRadius;
						y < (cY + DetectRadius > height ? height : cY + DetectRadius);
						y++
					)
					{
						if (siteMap.ContainsKey(x + y * width))
						{
							var siteMetaData = siteMap[x + y * width];

							var siteCX = siteMetaData.centerWorldX / _world.WIDTH;
							var siteCY = siteMetaData.centerWorldY / _world.HEIGHT;
							
							var siteHash = siteCX + siteCY * width;
							var siteData = DataBank.Instance.GetData<SiteData>(siteMetaData.siteId);
							if(!siteData.showOnMap)
								continue;;
							
							if (!m_RevealedSites.ContainsKey(siteHash))
							{
								var newRevealedSite = new RevealedSiteData
								{
									PosX = siteMetaData.centerWorldX,
									PosY = siteMetaData.centerWorldY,
									FullReveal = false,
									SiteId = siteMetaData.siteId,
									PrefixName = ""
								};
								m_RevealedSites.Add(siteHash, newRevealedSite);
								anyChanges = true;
							}
						}
					}
				}
			}
			else
			{
				/*if (m_RevealedSites.ContainsKey(x + y * width))
				{
					var siteToReveal = m_RevealedSites[x + y * width];
					if (!siteToReveal.FullReveal)
					{
						siteToReveal.FullReveal = true;
						m_RevealedSites[x + y * width] = siteToReveal;
						anyChanges = true;
					}
				}*/
			}

			if (anyChanges)
			{
				GameServer.NetAPI.World_SiteMapUpdate_BA(m_RevealedSites);
			}
			
		}

		private void CalculateRevealedSites()
		{
			bool anyRevealed = false;
			
			var siteMetaData = _world.GetSiteMetaData();
			var playerConns = GameServer.NetConnector.GetConnectedClientConnections();
			foreach (var conn in playerConns)
			{
				var entity = GameServer.NetConnector.GetPlayerFromConnection(conn);
				var playerChunk = _world.GetWorldChunk<ServerWorldChunk>();
				var siteHash = playerChunk.ChunkX + playerChunk.ChunkY * _world.WIDTH;
				var playerComp = entity.GetComponent<PlayerComponentServer>();
				WorldBlueprintGen.SiteMetaData currentSite = default; 
				if (siteMetaData.ContainsKey(siteHash))
				{
					var chunkSite = siteMetaData[siteHash];
					if (DoesPlayerSeeSite(entity, chunkSite))
					{
						currentSite = chunkSite;

						var siteCenterChunkX = chunkSite.centerWorldX / _world.WIDTH;
						var siteCenterChunkY = chunkSite.centerWorldY / _world.HEIGHT;
						var siteCenterHash = siteCenterChunkX + siteCenterChunkY * _world.WIDTH;
						if (m_RevealedSites.ContainsKey(siteCenterHash))
						{
							var siteToReveal = m_RevealedSites[siteCenterHash];
							if (!siteToReveal.FullReveal)
							{
								siteToReveal.FullReveal = true;
								m_RevealedSites[siteCenterHash] = siteToReveal;
								var siteData = DataBank.Instance.GetData<SiteData>(siteToReveal.SiteId);
								var playerName = entity.GetComponent<NameComponentServer>().entityName;
								GameServer.NetAPI.Notification_SendNotification_BA(
									siteData.mapIconRef,
									"cyan",
									"false",
									playerName + 
									" " + Localization.GetString("string_map_discovery") + ": " +
									Localization.GetString(siteData.siteTypeRef)
								);
								anyRevealed = true;
							}
						}
					}
				}
				playerComp.SetCurrentSite(!string.IsNullOrEmpty(currentSite.siteId), currentSite);
			}
			
			if (anyRevealed)
			{
				GameServer.NetAPI.World_SiteMapUpdate_BA(m_RevealedSites);
			}
		}

		private bool DoesPlayerSeeSite(FNEEntity player, WorldBlueprintGen.SiteMetaData site)
		{
			var startX = site.centerWorldX - (site.width / 2) - RevealPadding;
			var startY = site.centerWorldY - (site.height / 2) - RevealPadding;
			var endX = site.centerWorldX + (site.width / 2) + RevealPadding;
			var endY = site.centerWorldY + (site.height / 2) + RevealPadding;

			var playerPos = player.Position;

			if (playerPos.x > startX && playerPos.x < endX && playerPos.y > startY && playerPos.y < endY)
			{
				return true;
			}
			
			return false;
		}
	}
}