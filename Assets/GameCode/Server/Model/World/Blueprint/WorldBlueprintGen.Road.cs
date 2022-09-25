using System.Collections;
using System.Collections.Generic;
using FNZ.Shared.Utils;
using UnityEngine;

namespace FNZ.Server.Model.World.Blueprint 
{

	public partial class WorldBlueprintGen
	{
		private void GenerateHighways(
			bool[] occupiedChunks,
			Dictionary<int, bool> vertHighwayChunks,
			Dictionary<int, bool> horHighwayChunks
		)
		{
			int highwayChunkIndex = FNERandom.GetRandomIntInRange(180, 220);
			for (int x = 0; x < worldChunkSize; x++)
			{
				occupiedChunks[x + highwayChunkIndex * worldChunkSize] = true;
				pixels[x + highwayChunkIndex * worldChunkSize] = new Color32(0, 0, 0, 255);
				horHighwayChunks.Add(x + (highwayChunkIndex * worldChunkSize), true);
			}
			
			highwayChunkIndex = FNERandom.GetRandomIntInRange(36, 76);
			for (int x = 0; x < worldChunkSize; x++)
			{
				occupiedChunks[x + highwayChunkIndex * worldChunkSize] = true;
				pixels[x + highwayChunkIndex * worldChunkSize] = new Color32(0, 0, 0, 255);
				horHighwayChunks.Add(x + (highwayChunkIndex * worldChunkSize), true);
			}
			
			highwayChunkIndex = FNERandom.GetRandomIntInRange(180, 220);
			for (int y = 0; y < worldChunkSize; y++)
			{
				occupiedChunks[highwayChunkIndex + y * worldChunkSize] = true;
				pixels[highwayChunkIndex + y * worldChunkSize] = new Color32(0, 0, 0, 255);
				vertHighwayChunks.Add(highwayChunkIndex + (y * worldChunkSize), true);
			}
			
			highwayChunkIndex = FNERandom.GetRandomIntInRange(36, 76);
			for (int y = 0; y < worldChunkSize; y++)
			{
				occupiedChunks[highwayChunkIndex + y * worldChunkSize] = true;
				pixels[highwayChunkIndex + y * worldChunkSize] = new Color32(0, 0, 0, 255);
				vertHighwayChunks.Add(highwayChunkIndex + (y * worldChunkSize), true);
			}
		}

		public void FloodFillHighways(
			bool[] occupiedChunks,
			Dictionary<int, bool> vertHighwayChunks,
			Dictionary<int, bool> horHighwayChunks
		)
		{
			Stack<int> toVisit = new Stack<int>();
			Stack<int> toVisitNext = new Stack<int>();
			
			foreach (var key in vertHighwayChunks.Keys)
			{
				if(horHighwayChunks.ContainsKey(key))
					continue;
				
				toVisit.Push(key);
				m_RoadDistances[key] = 0;
			}
			
			foreach (var key in horHighwayChunks.Keys)
			{
				toVisit.Push(key);
				m_RoadDistances[key] = 0;
			}

			while (toVisit.Count > 0)
            {
                while (toVisit.Count > 0)
                {
	                var tileIndex = toVisit.Pop();

                    var tileX = tileIndex % worldChunkSize;
                    var tileY = tileIndex / worldChunkSize;

                    var thisCost = m_RoadDistances[tileIndex];

                    // var pixelVal = (byte) (thisCost > 127 ? 127 : thisCost);
                    // pixelVal *= 2;
                    //
                    // pixels[tileIndex] = new Color32(pixelVal, 0, 0, 255);
                    
                    if (tileX < 255)
                    {
	                    var eastIndex = (tileX + 1) + tileY * worldChunkSize;
	                    var eastCost = m_RoadDistances[eastIndex];
	                    if (!occupiedChunks[eastIndex] && eastCost > thisCost + 1)
	                    {
		                    m_RoadDistances[eastIndex] = thisCost + 1;
		                    toVisitNext.Push(eastIndex);
	                    }
                    }
                    
                    if (tileX > 0)
                    {
	                    var westIndex = (tileX - 1) + tileY * worldChunkSize;
	                    var westCost = m_RoadDistances[westIndex];
	                    if (!occupiedChunks[westIndex] && westCost > thisCost + 1)
	                    {
		                    m_RoadDistances[westIndex] = thisCost + 1;
		                    toVisitNext.Push(westIndex);
	                    }
                    }
                    
                    if (tileY < 255)
                    {
	                    var northIndex = tileX + (tileY + 1) * worldChunkSize;
	                    var northCost = m_RoadDistances[northIndex];
	                    if (!occupiedChunks[northIndex] && northCost > thisCost + 1)
	                    {
		                    m_RoadDistances[northIndex] = thisCost + 1;
		                    toVisitNext.Push(northIndex);
	                    }
                    }
                    
                    if (tileY > 0)
                    {
	                    var southIndex = tileX + (tileY - 1) * worldChunkSize;
	                    var southCost = m_RoadDistances[southIndex];
	                    if (!occupiedChunks[southIndex] && southCost > thisCost + 1)
	                    {
		                    m_RoadDistances[southIndex] = thisCost + 1;
		                    toVisitNext.Push(southIndex);
	                    }
                    }
                }

                var tmp = toVisit;
                toVisit = toVisitNext;
                toVisitNext = tmp;
            }
		}
		
		public void PlaceMajorRoads(
			bool[] occupiedChunks,
			List<SiteMetaData> majorSites,
			Dictionary<int, SiteMetaData> majorOccupiedChunks,
			Dictionary<int, bool> majorRoadChunks
		)
		{
			List<int> roadChunks = new List<int>();
			Dictionary<int, bool> connectedSites = new Dictionary<int, bool>();
			foreach (var site in majorSites)
			{
				var cX = site.centerWorldX / 32;
				var cY = site.centerWorldY / 32;
				
				if (connectedSites.ContainsKey(cX + cY * worldChunkSize))
					continue;

				roadChunks.Clear();
				
				connectedSites.Add(cX + cY * worldChunkSize, true);

				bool traverseNorth = !(cY > 128);

				var foundNature = false;
				while (!foundNature)
				{
					var index = 0;
					if (traverseNorth)
					{
						index = cX + (cY + 1) * worldChunkSize;
						cY++;
					}
					else
					{
						index = cX + (cY - 1) * worldChunkSize;
						cY--;
					}
					
					if (!occupiedChunks[index])
						foundNature = true;
				}

				pixels[cX + cY * worldChunkSize] = new Color32(255, 255, 0, 255);
				majorRoadChunks.Add(cX + cY * worldChunkSize, true);
				occupiedChunks[cX + cY * worldChunkSize] = true;
				roadChunks.Add(cX + cY * worldChunkSize);
				
				if (traverseNorth)
				{
					cY++;
				}
				else
				{
					cY--;
				}
				
				pixels[cX + cY * worldChunkSize] = new Color32(255, 255, 0, 255);
				majorRoadChunks.Add(cX + cY * worldChunkSize, true);
				occupiedChunks[cX + cY * worldChunkSize] = true;
				roadChunks.Add(cX + cY * worldChunkSize);
				
				var currentCost = m_RoadDistances[cX + cY * worldChunkSize];
				while (currentCost > 0)
				{
					var westIndex = (cX - 1) + cY * worldChunkSize;
					var eastIndex = (cX + 1) + cY * worldChunkSize;
					var northIndex = cX + (cY + 1) * worldChunkSize;
					var southIndex = cX + (cY - 1) * worldChunkSize;

					if (!occupiedChunks[westIndex] && m_RoadDistances[westIndex] < currentCost)
					{
						currentCost = m_RoadDistances[westIndex];
						cX--;
					}else if (!occupiedChunks[eastIndex] && m_RoadDistances[eastIndex] < currentCost)
					{
						currentCost = m_RoadDistances[eastIndex];
						cX++;
					}else if (!occupiedChunks[northIndex] && m_RoadDistances[northIndex] < currentCost)
					{
						currentCost = m_RoadDistances[northIndex];
						cY++;
					}else if (!occupiedChunks[southIndex] && m_RoadDistances[southIndex] < currentCost)
					{
						currentCost = m_RoadDistances[southIndex];
						cY--;
					}
					else
					{
						break;
					}
					
					occupiedChunks[cX + cY * worldChunkSize] = true;
					roadChunks.Add(cX + cY * worldChunkSize);
					pixels[cX + cY * worldChunkSize] = new Color32(128, 128, 128, 255);
				}

				FloodFillRoad(occupiedChunks, roadChunks);
			}
		}
		
		public void FloodFillRoad(
			bool[] occupiedChunks,
			List<int> roadChunks
		)
		{
			Stack<int> toVisit = new Stack<int>();
			Stack<int> toVisitNext = new Stack<int>();
			
			foreach (var key in roadChunks)
			{
				toVisit.Push(key);
				m_RoadDistances[key] = 0;
			}

			while (toVisit.Count > 0)
            {
                while (toVisit.Count > 0)
                {
	                var tileIndex = toVisit.Pop();

                    var tileX = tileIndex % worldChunkSize;
                    var tileY = tileIndex / worldChunkSize;

                    var thisCost = m_RoadDistances[tileIndex];

                    //var pixelVal = (byte) (thisCost > 127 ? 127 : thisCost);
                    //pixelVal *= 2;
                    
                    //pixels[tileIndex] = new Color32(pixelVal, 0, 0, 255);
                    
                    if (tileX < 255)
                    {
	                    var eastIndex = (tileX + 1) + tileY * worldChunkSize;
	                    var eastCost = m_RoadDistances[eastIndex];
	                    if (!occupiedChunks[eastIndex] && eastCost > thisCost + 1)
	                    {
		                    m_RoadDistances[eastIndex] = thisCost + 1;
		                    toVisitNext.Push(eastIndex);
	                    }
                    }
                    
                    if (tileX > 0)
                    {
	                    var westIndex = (tileX - 1) + tileY * worldChunkSize;
	                    var westCost = m_RoadDistances[westIndex];
	                    if (!occupiedChunks[westIndex] && westCost > thisCost + 1)
	                    {
		                    m_RoadDistances[westIndex] = thisCost + 1;
		                    toVisitNext.Push(westIndex);
	                    }
                    }
                    
                    if (tileY < 255)
                    {
	                    var northIndex = tileX + (tileY + 1) * worldChunkSize;
	                    var northCost = m_RoadDistances[northIndex];
	                    if (!occupiedChunks[northIndex] && northCost > thisCost + 1)
	                    {
		                    m_RoadDistances[northIndex] = thisCost + 1;
		                    toVisitNext.Push(northIndex);
	                    }
                    }
                    
                    if (tileY > 0)
                    {
	                    var southIndex = tileX + (tileY - 1) * worldChunkSize;
	                    var southCost = m_RoadDistances[southIndex];
	                    if (!occupiedChunks[southIndex] && southCost > thisCost + 1)
	                    {
		                    m_RoadDistances[southIndex] = thisCost + 1;
		                    toVisitNext.Push(southIndex);
	                    }
                    }
                }

                var tmp = toVisit;
                toVisit = toVisitNext;
                toVisitNext = tmp;
            }
		}
		
		public void PlaceMinorRoads(
			bool[] occupiedChunks,
			Dictionary<int, SiteMetaData> landmarkChunks,
			Dictionary<int, bool> minorRoadChunks
		)
		{
			List<int> roadChunks = new List<int>();
			Dictionary<int, bool> connectedSites = new Dictionary<int, bool>();
			foreach (var site in landmarkChunks)
			{
				var cX = site.Key % worldChunkSize;
				var cY = site.Key / worldChunkSize;
				
				if (connectedSites.ContainsKey(cX + cY * worldChunkSize))
					continue;

				roadChunks.Clear();
				
				connectedSites.Add(cX + cY * worldChunkSize, true);
				
				var northIndex =  cX + (cY + 1) * worldChunkSize;
				var eastIndex = (cX + 1) + cY * worldChunkSize;
				var westIndex =  (cX - 1)  + cY * worldChunkSize;
				var southIndex =  cX + (cY - 1) * worldChunkSize;

				var roadCost = int.MaxValue;
				
				if (!occupiedChunks[eastIndex] && roadCost > m_RoadDistances[eastIndex])
				{
					roadCost = m_RoadDistances[eastIndex];
					cX++;
				}
				
				if (!occupiedChunks[northIndex] && roadCost > m_RoadDistances[northIndex])
				{
					roadCost = m_RoadDistances[northIndex];
					cY++;
				}
				
				if (!occupiedChunks[westIndex] &&roadCost > m_RoadDistances[westIndex])
				{
					roadCost = m_RoadDistances[westIndex];
					cX--;
				}
				
				if (!occupiedChunks[southIndex] &&roadCost > m_RoadDistances[southIndex])
				{
					roadCost = m_RoadDistances[southIndex];
					cY--;
				}

				if (roadCost == int.MaxValue)
				{
					continue;
				}

				pixels[cX + cY * worldChunkSize] = new Color32(255, 255, 0, 255);
				minorRoadChunks.Add(cX + cY * worldChunkSize, true);
				occupiedChunks[cX + cY * worldChunkSize] = true;
				roadChunks.Add(cX + cY * worldChunkSize);

				var currentCost = m_RoadDistances[cX + cY * worldChunkSize];
				while (currentCost > 0)
				{
					westIndex = (cX - 1) + cY * worldChunkSize;
					eastIndex = (cX + 1) + cY * worldChunkSize;
					northIndex = cX + (cY + 1) * worldChunkSize;
					southIndex = cX + (cY - 1) * worldChunkSize;

					if (cX > 0 && !occupiedChunks[westIndex] && m_RoadDistances[westIndex] < currentCost)
					{
						currentCost = m_RoadDistances[westIndex];
						cX--;
					}else if (cX < worldChunkSize - 1 && !occupiedChunks[eastIndex] && m_RoadDistances[eastIndex] < currentCost)
					{
						currentCost = m_RoadDistances[eastIndex];
						cX++;
					}else if (cY < worldChunkSize - 1 && !occupiedChunks[northIndex] && m_RoadDistances[northIndex] < currentCost)
					{
						currentCost = m_RoadDistances[northIndex];
						cY++;
					}else if (cY > 0 && !occupiedChunks[southIndex] && m_RoadDistances[southIndex] < currentCost)
					{
						currentCost = m_RoadDistances[southIndex];
						cY--;
					}
					else
					{
						break;
					}
					
					occupiedChunks[cX + cY * worldChunkSize] = true;
					roadChunks.Add(cX + cY * worldChunkSize);
					pixels[cX + cY * worldChunkSize] = new Color32(96, 40, 0, 255);
				}

				FloodFillRoad(occupiedChunks, roadChunks);
			}
		}
	}
}