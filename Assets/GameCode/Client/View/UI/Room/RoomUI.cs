using FNZ.Client.Utils;
using FNZ.Client.View.UI.Environment;
using FNZ.Client.View.UI.Manager;
using FNZ.Client.View.UI.Sprites;
using FNZ.Shared.Model;
using FNZ.Shared.Model.World;
using FNZ.Shared.Model.World.Environment;
using FNZ.Shared.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.Rooms
{

	public class RoomUI : MonoBehaviour
	{
		private long m_CurrentRoomId;
		private long m_CurrentBaseId;
		
		public InputField INPUT_BaseName, INPUT_RoomName;

		public Image IMG_OnlinePromptBG;
		public Text TXT_OnlinePromptText;

		public RectTransform T_PropertyHeader;
		public RectTransform T_PropertyParent;
		public GameObject P_PropertyEntry;

		public RectTransform T_EnvironmentHeader;
		public RectTransform T_EnvironmentParent;
		public GameObject P_EnvironmentEntry;

		public RectTransform T_ResourcesHeader;
		public RectTransform T_ResourcesParent;
		public GameObject P_ResourceEntry;

		private int m_Offset = 0;

		private GameObject m_BaseOutline;

		public void Init()
		{
			GameClient.RoomManager.d_OnBaseRoomNameChange += RerenderNameChange;
			UIManager.Instance.d_OnBaseChanged += RerenderBase;
			UIManager.Instance.d_OnRoomChanged += RerenderRoom;
			
			var BaseOutlinePrefab = Resources.Load<GameObject>("Prefab/Entity/Player/BaseOutline");
			m_BaseOutline = Instantiate(BaseOutlinePrefab);
			m_BaseOutline.SetActive(false);
		}

		public void RerenderNameChange()
		{
			Rerender();
		}

		public void RerenderBase(long baseId)
		{
			m_CurrentBaseId = baseId;
			Rerender();
		}
		
		public void RerenderRoom(long roomId)
		{
			m_CurrentRoomId = roomId;
			Rerender();
		}

		public void SetNoRoom()
		{
			m_CurrentRoomId = 0;
		}

		private void Rerender()
		{
			if (!GameClient.RoomManager.BaseExists(m_CurrentBaseId))
			{
				m_BaseOutline.SetActive(false);
				gameObject.SetActive(false);
				return;
			}
			
			var bas = GameClient.RoomManager.GetBase(m_CurrentBaseId);

			var diameter = bas.radius + bas.radius + 1f;
			
			m_BaseOutline.SetActive(true);
			m_BaseOutline.GetComponent<MeshFilter>().sharedMesh = ViewUtils.GetFramedPlane(
				(int) diameter,
				(int) diameter
			);
			m_BaseOutline.transform.position = new Vector3(
				bas.Position.x + 0.5f - (diameter / 2f),
				0,
				bas.Position.y + 0.5f - (diameter / 2f)
			);
			
			gameObject.SetActive(true);
			
			INPUT_BaseName.text = GameClient.RoomManager.GetBaseName(m_CurrentBaseId);
			
			var room = GameClient.RoomManager.GetRoom(m_CurrentRoomId);

			if (room != null)
			{
				// Height of both name bars and online bar
				m_Offset = 74;
			}
			else
			{
				// Height of one name bar and online bar
				m_Offset = 49;
			}
			
			IMG_OnlinePromptBG.transform.localPosition = new Vector2(0, -m_Offset + 24);
			
			if (GameClient.RoomManager.IsBaseOnline(m_CurrentBaseId))
			{
				IMG_OnlinePromptBG.color = new Color(0.2f, 0.6f, 0.2f);
				TXT_OnlinePromptText.text = Localization.GetString("base_online_prompt");
			}
			else
			{
				IMG_OnlinePromptBG.color = new Color(0.5f, 0.15f, 0.15f);
				TXT_OnlinePromptText.text = Localization.GetString("base_offline_prompt");
			}
			
			if (room != null)
			{
				INPUT_RoomName.gameObject.SetActive(true);
				INPUT_RoomName.text = room.Name;
				
				var roomStatusValues = room.roomEnvironmentValues;
				
				RenderProperties(room);

				foreach (Transform t in T_EnvironmentParent)
					Destroy(t.gameObject);

				foreach (Transform t in T_ResourcesParent)
					Destroy(t.gameObject);
				
				if(room.roomEnvironmentValues.Count > 0)
				{
					RenderEnvironment(room);
				}
						
				if(room.Resources.Count > 0)
				{
					RenderResources(room);
				}
			}
			else
			{
				T_EnvironmentHeader.gameObject.SetActive(false);
				T_EnvironmentParent.gameObject.SetActive(false);
				T_ResourcesHeader.gameObject.SetActive(false);
				T_ResourcesParent.gameObject.SetActive(false);
				T_PropertyHeader.gameObject.SetActive(false);
				T_PropertyParent.gameObject.SetActive(false);
				
				INPUT_RoomName.gameObject.SetActive(false);
			}
		}

		private void RenderProperties(Room room)
		{
			foreach (Transform t in T_PropertyParent)
				Destroy(t.gameObject);

			if (room == null)
				return;

			T_PropertyHeader.gameObject.SetActive(true);
			T_PropertyParent.gameObject.SetActive(true);
			
			T_PropertyHeader.transform.localPosition = new Vector2(0, -m_Offset);
			m_Offset += 20;

			T_PropertyParent.transform.localPosition = new Vector2(0, -m_Offset);

			int counter = 0;
			foreach (var propId in room.RoomProperties.Keys)
			{
				var propData = DataBank.Instance.GetData<RoomPropertyData>(propId);

				GameObject entry = Instantiate(P_PropertyEntry);
				entry.transform.SetParent(T_PropertyParent, false);
				entry.GetComponent<RoomPropertyEntry>().Render(propData, room.RoomProperties[propId]);
				entry.transform.localPosition = new Vector2(0, counter * -24);
				counter++;
			}

			T_PropertyParent.sizeDelta = new Vector2(0, counter * 24);
			m_Offset += 24 * counter;
		}

		private void RenderEnvironment(Room room)
		{
			if (room == null)
				return;

			bool renderedHeader = false;

			int counter = 0;
			foreach (var rev in room.roomEnvironmentValues)
			{
				var data = DataBank.Instance.GetData<EnvironmentData>(rev.Key);
				var value = room.roomEnvironmentValues[data.Id];
				if (value == 0)
				{
					continue;
				}

				if (!renderedHeader)
				{
					T_EnvironmentParent.gameObject.SetActive(true);
					T_EnvironmentHeader.gameObject.SetActive(true);
					renderedHeader = true;

					T_EnvironmentHeader.transform.localPosition = new Vector2(0, -m_Offset);
					m_Offset += 20;
					T_EnvironmentParent.transform.localPosition = new Vector2(0, -m_Offset);
				}

                var amount = room.roomEnvironmentAffectors[data.Id];

                GameObject entry = Instantiate(P_EnvironmentEntry);
				entry.transform.SetParent(T_EnvironmentParent, false);
				
				var comp = entry.GetComponent<EnvironmentEntry>();
				comp.Render(data, value, amount);

				entry.transform.localPosition = new Vector2((counter % 5) * 60, (counter / 5) * -60);
				counter++;
			}

			if(counter > 0)
			{
				T_EnvironmentParent.sizeDelta = new Vector2(0, 4 + (60 * ((counter / 5) + 1)));
				m_Offset += 4 + (60 * ((counter / 5) + 1));
			}
		}

		private void RenderResources(Room room)
		{
			if (room == null)
				return;

			bool renderedHeader = false;

			int counter = 0;
			foreach (var res in room.Resources)
			{
				var data = DataBank.Instance.GetData<RoomResourceData>(res.Key);
				var value = room.Resources[data.Id];

				if (value == 0)
				{
					continue;
				}

				if (!renderedHeader)
				{
					T_ResourcesHeader.gameObject.SetActive(true);
					T_ResourcesHeader.transform.localPosition = new Vector2(0, -m_Offset);
					m_Offset += 20;
					T_ResourcesParent.gameObject.SetActive(true);
					T_ResourcesParent.transform.localPosition = new Vector2(0, -m_Offset);

					renderedHeader = true;
				}

				GameObject entry = Instantiate(P_ResourceEntry);
				entry.transform.SetParent(T_ResourcesParent, false);

				var comp = entry.GetComponent<RoomResourceEntry>();
				comp.Render(data, value);

				entry.transform.localPosition = new Vector2((counter % 5) * 60, (counter / 5) * -60);
				counter++;
			}

			T_ResourcesParent.sizeDelta = new Vector2(0, 4 + (60 * ((counter / 5) + 1)));
			m_Offset += 4 + (60 * ((counter / 5) + 1));
		}

		public void SubmitRoomName(string newName)
		{
			GameClient.NetAPI.CMD_World_BaseRoomNameChange(false, m_CurrentRoomId, newName);
		}

		public void SubmitBaseName(string newName)
		{
			GameClient.NetAPI.CMD_World_BaseRoomNameChange(true, m_CurrentBaseId, newName);
		}

		/*public Transform T_ResourcesParent;
		public GameObject P_ResourceEntry;

		public void Init()
		{
			UIManager.Instance.d_OnRoomChanged += Rerender;
		}

		private void Rerender(long roomId)
		{
			foreach (Transform oldResource in T_ResourcesParent)
				Destroy(oldResource.gameObject);

			var room = GameClient.RoomManager.GetRoom(roomId);

			foreach (var resourceId in room.Resources.Keys)
			{
				var resourceData = DataBank.Instance.GetData<RoomResourceData>(resourceId);

				GameObject entry = Instantiate(P_ResourceEntry);
				entry.transform.SetParent(T_ResourcesParent, false);
				entry.GetComponent<RoomResourceEntry>().Render(resourceData, room.Resources[resourceId]);
			}

			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)T_ResourcesParent);
		}*/
	}
}