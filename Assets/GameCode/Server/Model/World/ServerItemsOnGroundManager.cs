using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.World;
using FNZ.Shared.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Server.Services 
{
	public class ServerItemsOnGroundManager : ItemsOnGroundManager
	{
		static long identifierCounter = 0;
		public void SpawnItemOnGround(float2 Position, Item item)
        {
			if(item == null)
            {
				Debug.LogError("A null item was passed to spawn item on ground!");
				return;
            }

			float flingDirection = FNERandom.GetRandomFloatInRange(0, 360);
			long identifier = ++identifierCounter;
			var itemOnGround = new ItemOnGround
			{
				identifier = identifier,
				item = item,
				Position = Position,
				FlingDirectionZ = flingDirection
			};

			itemsDict.Add(
				identifier,
				itemOnGround
			);

			GameServer.NetAPI.BA_Spawn_Item_On_Ground(itemOnGround);
		}

		public Item GetItemOnGround(long identifier)
		{
			if (!itemsDict.ContainsKey(identifier))
			{
				return null;
			}

			return itemsDict[identifier].item;
		}

		public Item PopItemOnGround(long identifier)
		{
			if (!itemsDict.ContainsKey(identifier))
			{
				return null;
			}

			var toReturn = itemsDict[identifier].item;
			itemsDict.Remove(identifier);

			return toReturn;
		}
	}
}