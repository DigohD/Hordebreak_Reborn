using System.Collections;
using System.Collections.Generic;
using FNZ.Shared.Model;
using FNZ.Shared.Model.World.Site;
using FNZ.Shared.Utils;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Server.Model.World.Blueprint
{
	public partial class WorldBlueprintGen
	{
		private void ReserveMajorSiteChunks(
			bool[] occupiedChunks,
			Dictionary<int, bool> majorChunkCenters
		)
		{
			for (int y = sitePadding; y < worldChunkSize - sitePadding; y++)
			{
				for (int x = sitePadding; x < worldChunkSize - sitePadding; x++)
				{
					if(FNERandom.GetRandomIntInRange(0, 250) == 1)
					{
						var skipChunk = false;
						for (int testY = y - sitePadding; testY <= y + sitePadding; testY++)
						{
							for (int testX = x - sitePadding; testX <= x + sitePadding; testX++)
							{
								skipChunk = skipChunk || occupiedChunks[testX + testY * worldChunkSize];
								if (skipChunk)
									break;
							}
							if (skipChunk)
								break;
						}

						if(skipChunk)
						{
							continue;
						}

						majorChunkCenters.Add(x + (y * worldChunkSize), true);

						for(int i = x - sitePadding; i < x + sitePadding; i++)
						{
							for (int j = y - sitePadding; j < y + sitePadding; j++)
							{
								occupiedChunks[i + j * worldChunkSize] = true;
								pixels[i + j * worldChunkSize] = new Color32(0, 0, 255, 255);
							}
						}

						pixels[x + y * worldChunkSize] = new Color32(0, 255, 0, 255);
					}
				}
			}
		}
		
		public Dictionary<int, SiteMetaData> PlaceMajorSites(Dictionary<int, bool> majorChunkCenters, Color32[] pixels)
        {
			var allSites = DataBank.Instance.GetAllDataDefsOfType(typeof(SiteData));

			List<SiteData> possibleSites = new List<SiteData>();

			foreach (var siteDef in allSites)
            {
				var data = DataBank.Instance.GetData<SiteData>(siteDef.Id);
				if(data.height > 32 || data.width > 32)
                {
					possibleSites.Add(data);
				}
            }

			Dictionary<int, SiteMetaData> actualOccupiedChunks = new Dictionary<int, SiteMetaData>();

			if(possibleSites.Count > 0)
				foreach (var center in majorChunkCenters.Keys)
				{
					int x = center % 256;
					int y = center / 256;

					var site = possibleSites[FNERandom.GetRandomIntInRange(0, possibleSites.Count)];

					// DEMO CODE
					if (x == 136 && y == 136)
					{
						site = DataBank.Instance.GetData<SiteData>("mudplains_site_city_t2");
					}

					if (event1Sites.Contains(new int2(x, y)))
					{
						site = DataBank.Instance.GetData<SiteData>("mudplains_site_factory");
					}
					
					var centerWorldX = x * 32 + FNERandom.GetRandomIntInRange(0, 31);
					var centerWorldY = y * 32 + FNERandom.GetRandomIntInRange(0, 31);

					var startX = centerWorldX - site.width / 2;
					var startY = centerWorldY - site.height / 2;

					var endX = startX + site.width;
					var endY = startY + site.height;

					int siteChunks = 0;

					var currentX = startX;
					var currentY = startY;
					while (currentX < endX)
					{
						while (currentY < endY)
						{
							ushort chunkX = (ushort)(currentX / 32);
							ushort chunkY = (ushort)(currentY / 32);

							siteChunks++;
							var siteMetaData = new SiteMetaData
							{
								centerWorldX = centerWorldX,
								centerWorldY = centerWorldY,
								chunkX = chunkX,
								chunkY = chunkY,
								width = (byte)site.width,
								height = (byte)site.height,
								siteId = site.Id,
								rotation = (byte)FNERandom.GetRandomIntInRange(0, 4)
							};

							actualOccupiedChunks.Add(chunkX + chunkY * 256, siteMetaData);
							
							// base case
							if(currentY % 32 != 0)
							{
								currentY += 32 - (currentY % 32);
							}
							else if(endY - currentY > 32)
							{
								currentY += 32;
							}
							else
							{
								currentY += endY - currentY;
							}
						}
						currentY = startY;

						// base case
						if (currentX % 32 != 0)
						{
							currentX += 32 - (currentX % 32);
						}
						else if (endX - currentX > 32)
						{
							currentX += 32;
						}
						else
						{
							currentX += endX - currentX;
						}
					}

					//Debug.LogWarning(siteChunks + " site chunks for " + site.Id);
				}

			return actualOccupiedChunks;
		}
	}
}