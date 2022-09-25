using FNZ.Client.View.UI.Manager;
using FNZ.Client.View.UI.Rooms;
using FNZ.Client.View.UI.Sprites;
using FNZ.Shared.Model;
using FNZ.Shared.Model.World;
using FNZ.Shared.Model.World.Environment;
using FNZ.Shared.Utils;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.Environment
{
	public class EnvironmentUI : MonoBehaviour
	{
		public Transform T_EnvironmentStatus;
		public GameObject P_EnvironmentEntry;

		private long m_CurrentRoomId;

		public InputField INPUT_BaseName, INPUT_RoomName;

		public RoomUI roomUI;
		public RoomPropertyUI roomPropertyUI;

		private void Init()
		{
			GameClient.RoomManager.d_OnBaseRoomNameChange += RerenderUIChange;
			UIManager.Instance.d_OnRoomChanged += RerenderRoomChange;
		}

		private void Start()
		{
			//roomUI.Init();
			roomPropertyUI.Init();
		}

		private void OnDestroy()
		{
			GameClient.RoomManager.d_OnBaseRoomNameChange -= RerenderUIChange;
			UIManager.Instance.d_OnRoomChanged -= RerenderRoomChange;
			//roomUI.Init();
		}

		private void RerenderRoomChange(long roomId)
		{
			var room = GameClient.RoomManager.GetRoom(roomId);

			if (room != null)
				Rerender(room.Id);
		}

		private void RerenderUIChange()
		{
			var room = GameClient.RoomManager.GetRoom((int2)GameClient.LocalPlayerEntity.Position);

			if (room != null)
				Rerender(room.Id);
		}

		public void Rerender(long roomId)
		{
			this.m_CurrentRoomId = roomId;

			foreach (Transform oldStatus in T_EnvironmentStatus)
				Destroy(oldStatus.gameObject);

			var room = GameClient.RoomManager.GetRoom(roomId);
			if (room != null)
			{
				gameObject.SetActive(true);
				var roomStatusValues = room.roomEnvironmentValues;

				INPUT_BaseName.text = GameClient.RoomManager.GetBaseName(room.ParentBaseId);
				INPUT_RoomName.text = room.Name;

				CalculateIndoors(roomStatusValues, room);
			}
			else
			{
				gameObject.SetActive(false);
			}

			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) T_EnvironmentStatus);
			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) transform);
		}
		private void CalculateIndoors(Dictionary<string, int> roomEnvironmentValues, Room room)
		{
			foreach (var rev in roomEnvironmentValues)
			{
                var data = DataBank.Instance.GetData<EnvironmentData>(rev.Key);
                var value = room.roomEnvironmentValues[data.Id];
                if (value == 0)
                {
                    continue;
                }

				GameObject entry = Instantiate(P_EnvironmentEntry);
				entry.transform.SetParent(T_EnvironmentStatus, false);
				var comp = entry.GetComponent<EnvironmentEntry>();

				comp.TXT_Name.text = Localization.GetString(data.nameRef);
				comp.TXT_Amount.text = "" + value;
				comp.IMG_Icon.sprite = SpriteBank.GetSprite(data.iconRef);
			}
		}

		public void SubmitRoomName(string newName)
		{
			GameClient.NetAPI.CMD_World_BaseRoomNameChange(false, m_CurrentRoomId, newName);
		}

		public void SubmitBaseName(string newName)
		{
			var room = GameClient.RoomManager.GetRoom(m_CurrentRoomId);
			GameClient.NetAPI.CMD_World_BaseRoomNameChange(true, room.ParentBaseId, newName);
		}


	}
}