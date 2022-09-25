using FNZ.Client.View.Audio;
using FNZ.Shared.Model.World;
using System.Collections;
using System.Collections.Generic;
using FNZ.Client.View.UI.HoverBox;
using FNZ.Client.View.UI.Manager;
using FNZ.Shared.Model.Items;
using UnityEngine;

namespace FNZ.Client.Model.World 
{

	public class ClientItemsOnGroundManager : ItemsOnGroundManager
	{
		private Dictionary<long, GameObject> ItemViewsDict = new Dictionary<long, GameObject>();

		public void AddItemOnGround(ItemOnGround item)
        {
			itemsDict.Add(item.identifier, item);

			var view = GameClient.ViewAPI.SpawnItemOnGroundView(item.Position);
			ItemViewsDict.Add(item.identifier, view);

			AudioManager.Instance.PlaySfx3dClip(item.item.Data.laydownSoundRef, item.Position, 15);
		}

		public void RemoveItemOnGround(long identifier)
		{
			AudioManager.Instance.PlaySfx3dClip(itemsDict[identifier].item.Data.pickupSoundRef, itemsDict[identifier].Position, 15);
			var item = itemsDict[identifier];

			itemsDict.Remove(identifier);
			var view = ItemViewsDict[identifier];
			ItemViewsDict.Remove(identifier);
			GameClient.Destroy(view);
		}
	}
}