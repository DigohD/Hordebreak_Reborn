using FNZ.Client.View.UI.Manager;
using System.Collections.Generic;
using UnityEngine;

namespace FNZ.Client.Utils.UI
{
	public class UI_StartMenuErrors : MonoBehaviour
	{
		[SerializeField] private GameObject m_Toggle;
		[SerializeField] private Transform m_ContentParent;

		private static List<GameObject> m_ErrorObjects = new List<GameObject>();

		private static string ERROR_NAME = string.Empty;
		private static string ERROR_MESSAGE = string.Empty;

		private void OnEnable()
		{
			if (ERROR_NAME != string.Empty)
				NewErrorMessage(ERROR_NAME, ERROR_MESSAGE);
		}

		private void Start()
		{
			ERROR_NAME = string.Empty;
			ERROR_MESSAGE = string.Empty;
		}

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

		public static void NameTaken(string errorName, string errorMessage)
		{
			ERROR_NAME = errorName;
			ERROR_MESSAGE = errorMessage;
		}
	}
}