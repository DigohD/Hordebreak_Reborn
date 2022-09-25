using System.Collections;
using System.Collections.Generic;
using FNZ.Shared.Model;
using FNZ.Shared.Model.World.Site;
using FNZ.Shared.Utils;
using UnityEngine;

namespace FNZ.Server.Model.World.Blueprint
{

	public partial class WorldBlueprintGen
	{
		private void ReserveLandmarkChunks(
			bool[] occupiedChunks,
			Dictionary<int, bool> landmarkChunks
		)
		{
			// Generate Landmark site map (smaller than 1 chunk in any direction and important)
			for (int y = 1; y < worldChunkSize - 1; y++)
			{
				for (int x = 1; x < worldChunkSize - 1; x++)
				{
					if (FNERandom.GetRandomIntInRange(0, 100) < 5)
					{
						bool skip = false;
						for (int i = x - 1; i < x + 1; i++)
						{
							for (int j = y - 1; j < y + 1; j++)
							{
								if (occupiedChunks[i + j * worldChunkSize])
								{
									skip = true;
									break;
								}
							}
							if (skip)
								break;
						}

						if (skip)
							continue;

						landmarkChunks.Add(x + (y * worldChunkSize), true);
						occupiedChunks[x + y * worldChunkSize] = true;

						pixels[x + y * worldChunkSize] = new Color32(0, 255, 255, 255);
					}
				}
			}
		}
		
		public Dictionary<int, SiteMetaData> PlaceLandmarks(Dictionary<int, bool> minorchunks, Color32[] pixels)
		{
			var allSites = DataBank.Instance.GetAllDataDefsOfType(typeof(SiteData));

			List<SiteData> possibleSites = new List<SiteData>();
			List<SiteData> possibleSpawnSites = new List<SiteData>();
			
			foreach (var siteDef in allSites)
			{
				var data = DataBank.Instance.GetData<SiteData>(siteDef.Id);
				if(!data.showOnMap)
					continue;
				
				if (data.height < 32 && data.width < 32)
				{
					possibleSites.Add(data);
					if (data.enemyBudget == 0)
					{
						possibleSpawnSites.Add(data);
					}
				}
			}

			Dictionary<int, SiteMetaData> actualOccupiedChunks = new Dictionary<int, SiteMetaData>();

			foreach (var chunkPos in minorchunks.Keys)
			{
				int x = chunkPos % 256;
				int y = chunkPos / 256;

				SiteData site;

				if (
					possibleSpawnSites.Count > 0 
					&& x >= 128 - safeZonePadding
					&& x <= 128 + safeZonePadding
					&& y >= 128 - safeZonePadding
					&& y <= 128 + safeZonePadding
				)
				{
					site = possibleSpawnSites[FNERandom.GetRandomIntInRange(0, possibleSpawnSites.Count)];
				}
				else
				{
					site = possibleSites[FNERandom.GetRandomIntInRange(0, possibleSites.Count)];
				}
				
				var centerOffsetX = FNERandom.GetRandomIntInRange(site.width / 2, 31 - site.width / 2);
				var centerOffsetY = FNERandom.GetRandomIntInRange(site.height / 2, 31 - site.height / 2);

				var centerWorldX = x * 32 + centerOffsetX;
				var centerWorldY = y * 32 + centerOffsetY;

				var siteMetaData = new SiteMetaData
				{
					centerWorldX = centerWorldX,
					centerWorldY = centerWorldY,
					chunkX = (ushort) x,
					chunkY = (ushort) y,
					width = (byte)site.width,
					height = (byte)site.height,
					siteId = site.Id,
					rotation = (byte)FNERandom.GetRandomIntInRange(0, 4)
				};

				actualOccupiedChunks.Add(x + y * 256, siteMetaData);
			}

			return actualOccupiedChunks;
		}
		
		private void ReservePlotChunks(
			bool[] occupiedChunks,
			Dictionary<int, bool> plotChunks
		)
		{
			// Generate Landmark site map (smaller than 1 chunk in any direction and important)
			for (int y = 1; y < worldChunkSize - 1; y++)
			{
				for (int x = 1; x < worldChunkSize - 1; x++)
				{
					if (FNERandom.GetRandomIntInRange(0, 100) < 25)
					{
						bool skip = false;
						for (int i = x - 1; i < x + 1; i++)
						{
							for (int j = y - 1; j < y + 1; j++)
							{
								if (occupiedChunks[i + j * worldChunkSize])
								{
									skip = true;
									break;
								}
							}
							if (skip)
								break;
						}

						if (skip)
							continue;

						plotChunks.Add(x + (y * worldChunkSize), true);
						occupiedChunks[x + y * worldChunkSize] = true;

						pixels[x + y * worldChunkSize] = new Color32(128, 0, 0, 50);
					}
				}
			}
		}
		
		public Dictionary<int, SiteMetaData> PlacePlots(Dictionary<int, bool> plotChunks, Color32[] pixels)
		{
			var allSites = DataBank.Instance.GetAllDataDefsOfType(typeof(SiteData));

			List<SiteData> possibleSites = new List<SiteData>();
			List<SiteData> possibleSpawnSites = new List<SiteData>();
			
			foreach (var siteDef in allSites)
			{
				var data = DataBank.Instance.GetData<SiteData>(siteDef.Id);
				if(data.showOnMap)
					continue;
				
				if (data.height < 32 && data.width < 32)
				{
					possibleSites.Add(data);
					if (data.enemyBudget == 0)
					{
						possibleSpawnSites.Add(data);
					}
				}
			}

			Dictionary<int, SiteMetaData> actualOccupiedChunks = new Dictionary<int, SiteMetaData>();

			foreach (var chunkPos in plotChunks.Keys)
			{
				int x = chunkPos % 256;
				int y = chunkPos / 256;

				SiteData site;

				if (
					possibleSpawnSites.Count > 0 
					&& x >= 128 - safeZonePadding
					&& x <= 128 + safeZonePadding
					&& y >= 128 - safeZonePadding
					&& y <= 128 + safeZonePadding
				)
				{
					site = possibleSpawnSites[FNERandom.GetRandomIntInRange(0, possibleSpawnSites.Count)];
				}
				else
				{
					site = possibleSites[FNERandom.GetRandomIntInRange(0, possibleSites.Count)];
				}
				
				var centerOffsetX = FNERandom.GetRandomIntInRange(site.width / 2, 31 - site.width / 2);
				var centerOffsetY = FNERandom.GetRandomIntInRange(site.height / 2, 31 - site.height / 2);

				var centerWorldX = x * 32 + centerOffsetX;
				var centerWorldY = y * 32 + centerOffsetY;

				var siteMetaData = new SiteMetaData
				{
					centerWorldX = centerWorldX,
					centerWorldY = centerWorldY,
					chunkX = (ushort) x,
					chunkY = (ushort) y,
					width = (byte)site.width,
					height = (byte)site.height,
					siteId = site.Id,
					rotation = (byte)FNERandom.GetRandomIntInRange(0, 4)
				};

				actualOccupiedChunks.Add(x + y * 256, siteMetaData);
			}

			return actualOccupiedChunks;
		}
	}
}