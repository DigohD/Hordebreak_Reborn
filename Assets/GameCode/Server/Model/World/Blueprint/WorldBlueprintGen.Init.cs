using System.Collections;
using System.Collections.Generic;
using FNZ.Shared.Utils;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Server.Model.World.Blueprint 
{

	public partial class WorldBlueprintGen
	{
		public void InitBlueprintGen(
			bool[] occupiedChunks,
			Dictionary<int, bool> majorChunkCenters,
			Dictionary<int, bool> minorChunks
		)
		{
			majorChunkCenters.Add(135 + (128 * worldChunkSize), true);
            event1Sites.Add(new int2(135, 128));
            for (int i = 135 - sitePadding; i < 135 + sitePadding; i++)
            {
                for (int j = 128 - sitePadding; j < 128 + sitePadding; j++)
                {
                    occupiedChunks[i + j * worldChunkSize] = true;
                    pixels[i + j * worldChunkSize] = new Color32(0, 0, 255, 255);
                }
            }
            
            majorChunkCenters.Add(121 + (128 * worldChunkSize), true);
            event1Sites.Add(new int2(121, 128));
            for (int i = 121 - sitePadding; i < 121 + sitePadding; i++)
            {
	            for (int j = 128 - sitePadding; j < 128 + sitePadding; j++)
	            {
		            occupiedChunks[i + j * worldChunkSize] = true;
		            pixels[i + j * worldChunkSize] = new Color32(0, 0, 255, 255);
	            }
            }
            
            majorChunkCenters.Add(128 + (121 * worldChunkSize), true);
            event1Sites.Add(new int2(128, 121));
            for (int i = 128 - sitePadding; i < 128 + sitePadding; i++)
            {
	            for (int j = 121 - sitePadding; j < 121 + sitePadding; j++)
	            {
		            occupiedChunks[i + j * worldChunkSize] = true;
		            pixels[i + j * worldChunkSize] = new Color32(0, 0, 255, 255);
	            }
            }
            
            majorChunkCenters.Add(128 + (135 * worldChunkSize), true);
            event1Sites.Add(new int2(128, 135));
            for (int i = 128 - sitePadding; i < 128 + sitePadding; i++)
            {
	            for (int j = 135 - sitePadding; j < 135 + sitePadding; j++)
	            {
		            occupiedChunks[i + j * worldChunkSize] = true;
		            pixels[i + j * worldChunkSize] = new Color32(0, 0, 255, 255);
	            }
            }
            
            majorChunkCenters.Add(136 + (136 * worldChunkSize), true);
            event2Site = new int2(136, 136);
            for (int i = 136 - sitePadding; i < 136 + sitePadding; i++)
            {
	            for (int j = 136 - sitePadding; j < 136 + sitePadding; j++)
	            {
		            occupiedChunks[i + j * worldChunkSize] = true;
		            pixels[i + j * worldChunkSize] = new Color32(0, 0, 255, 255);
	            }
            }
            
            for (int i = 128 - safeZonePadding; i < 128 + safeZonePadding; i++)
            {
	            for (int j = 128 - safeZonePadding; j < 128 + safeZonePadding; j++)
	            {

		            if (occupiedChunks[i + j * worldChunkSize])
			            continue;
		            
		            occupiedChunks[i + j * worldChunkSize] = true;
		            
		            if (FNERandom.GetRandomIntInRange(0, 100) < 25)
		            {
			            pixels[i + j * worldChunkSize] = new Color32(255, 0, 0, 255);
			            minorChunks.Add(i + (j * worldChunkSize), true);
		            }
		            else
		            {
			            pixels[i + j * worldChunkSize] = new Color32(0, 0, 255, 255);
		            }
	            }
            }
		}
	}
}