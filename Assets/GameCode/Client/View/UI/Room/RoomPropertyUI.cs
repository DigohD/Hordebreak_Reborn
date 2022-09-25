using FNZ.Client.View.UI.Manager;
using FNZ.Client.View.UI.Rooms;
using FNZ.Shared.Model;
using FNZ.Shared.Model.World;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.Rooms
{

	public class RoomPropertyUI : MonoBehaviour
	{
		public Transform T_PropertiesParent;
		public GameObject P_PropertyEntry;

		public void Init()
		{
			UIManager.Instance.d_OnRoomChanged += Rerender;
		}

		private void Rerender(long roomId)
		{
			foreach (Transform oldProp in T_PropertiesParent)
				Destroy(oldProp.gameObject);

			var room = GameClient.RoomManager.GetRoom(roomId);

			foreach (var propId in room.RoomProperties.Keys)
			{
				var propData = DataBank.Instance.GetData<RoomPropertyData>(propId);

				GameObject entry = Instantiate(P_PropertyEntry);
				entry.transform.SetParent(T_PropertiesParent, false);
				entry.GetComponent<RoomPropertyEntry>().Render(propData, room.RoomProperties[propId]);
			}

			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)T_PropertiesParent);
		}
	}
}