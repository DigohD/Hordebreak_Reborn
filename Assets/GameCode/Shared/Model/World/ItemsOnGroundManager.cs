using FNZ.Shared.Model.Items;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Shared.Model.World 
{

	public struct ItemOnGround
	{
		public long identifier;
		public Item item;
		public float2 Position;
		public float FlingDirectionZ;
	}

	public class ItemsOnGroundManager
	{
		protected Dictionary<long, ItemOnGround> itemsDict = new Dictionary<long, ItemOnGround>();

		public Dictionary<long, ItemOnGround> GetItemsOnGround()
		{
			return itemsDict;
		}
	}
}