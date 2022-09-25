using System.Collections.Generic;
using UnityEngine;

namespace FNZ.Client.View.UI.Manager
{
	public class UI_ErrorMessage : MonoBehaviour
	{
		[SerializeField] private GameObject m_Toggle;

		[SerializeField] private Transform m_ContentParent;

		private List<GameObject> m_ErrorObjects = new List<GameObject>();

		public void OnButtonClick()
		{
			m_Toggle.SetActive(false);

			foreach (var obj in m_ErrorObjects)
				Destroy(obj);

			m_ErrorObjects.Clear();
		}

		public void NewErrorMessage(string title, string info)
		{
			if (!m_Toggle.activeInHierarchy)
				m_Toggle.SetActive(true);

			var newErrorMessage = Instantiate(Resources.Load("Prefab/UI/ErrorMessagePrefab"), m_ContentParent) as GameObject;
			newErrorMessage.GetComponent<UI_Error>().Init(title, info);
			m_ErrorObjects.Add(newErrorMessage);
		}
	}
}