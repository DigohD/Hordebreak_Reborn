using FNZ.Client.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace FNZ.Client.View.UI.Notifications
{
	public class UI_Notification : FNESingleton<UI_Notification>
	{
		public GameObject GO_NoteBox;
		public List<GameObject> NotificationBoxList = new List<GameObject>();
		public Dictionary<float, GameObject> NotificationBoxIds = new Dictionary<float, GameObject>();
		public static float s_Spacing = 55;

		[SerializeField] private Transform T_BoxParent;
		private const float m_DefaultDuration = 5;
		private bool m_FoundInactive;

		public void ManageNotification(string spriteId, string color, bool isPermanent, string message, float identifier)
		{
			if (NotificationBoxIds.ContainsKey(identifier))
				UpdateNotification(spriteId, color, isPermanent, message, identifier);
			else
				CreateNewNotification(spriteId, color, isPermanent, message, identifier);
		}

		public void CreateNewNotification(string spriteId, string color, bool isPermanent, string message, float identifier)
		{
			m_FoundInactive = false;
			int tempIndex = 0;

			//counts the number of permanent boxes
			foreach (Transform child in T_BoxParent)
			{
				if (child.gameObject.GetComponent<UI_NotificationBox>().IsPermanent)
					tempIndex++;
			}

			//Places the new box in it's proper position.
			foreach (Transform child in T_BoxParent)
			{
				var box = child.gameObject;

				if (!box.activeInHierarchy)
				{
					m_FoundInactive = true;
					InitializeBox(box, tempIndex, spriteId, color, isPermanent, message, identifier);
					box.SetActive(true);

					break;
				}
			}

			//If there were no inactive boxes available.
			if (!m_FoundInactive)
			{
				var box = Instantiate(GO_NoteBox, transform);
				InitializeBox(box, tempIndex, spriteId, color, isPermanent, message, identifier);
			}
		}

		public void UpdateNotification(string spriteId, string color, bool isPermanent, string message, float identifier)
		{
			var comp = NotificationBoxIds[identifier].GetComponent<UI_NotificationBox>();
			comp.Init(spriteId, message, color);
			if (!isPermanent)
				DeletePermanentBox(identifier);
		}

		private void InitializeBox(GameObject box, int tempIndex, string spriteId, string color, bool isPermanent, string message, float identifier)
		{
			var comp = box.GetComponent<UI_NotificationBox>();

			comp.Identifier = identifier;
			NotificationBoxIds.Add(comp.Identifier, box);
			comp.SetTimer(m_DefaultDuration);
			comp.Init(spriteId, message, color);
			comp.IsPermanent = isPermanent;

			if (isPermanent)
			{
				NotificationBoxList.Insert(0, box);
				box.transform.localPosition = (Vector3.up * s_Spacing) * 0;
			}
			else
			{
				NotificationBoxList.Insert(tempIndex, box);
				box.transform.localPosition = (Vector3.up * s_Spacing) * tempIndex;
			}

			//comp.SetBoxSize();
		}

		public void DeletePermanentBox(float boxId)
		{
			foreach (Transform child in T_BoxParent)
			{
				var obj = child.gameObject;
				var comp = obj.GetComponent<UI_NotificationBox>();

				if (obj.activeInHierarchy && comp.IsPermanent && comp.Identifier == boxId)
				{
					comp.SetTimer(comp.FadeOutTime);
					comp.SetPermanentFalse();
					break;
				}
			}
		}

		public void DeleteFromList(GameObject expireNotification)
		{
			float identifierId = expireNotification.GetComponent<UI_NotificationBox>().Identifier;

			NotificationBoxIds.Remove(identifierId);
			NotificationBoxList.Remove(expireNotification);
		}
	}
}