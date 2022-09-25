using System.Collections.Generic;
using UnityEngine;

namespace FNZ.Client.View.UI.HoverBox
{
	public class HB_Pool : MonoBehaviour
	{
		private List<GameObject> m_HoverBoxList = new List<GameObject>();
		private List<GameObject> m_TextList = new List<GameObject>();
		private List<GameObject> m_DividerLineList = new List<GameObject>();
		private List<GameObject> m_IconTextRowList = new List<GameObject>();
		private List<GameObject> m_IconTextIconRowList = new List<GameObject>();
		private List<GameObject> m_RepeatingIconRowList = new List<GameObject>();
		
		//private List<GameObject> m_LootInfoList = new List<GameObject>();
		//private List<GameObject> m_ModInfoList = new List<GameObject>();

		private GameObject m_HoverBox;
		private GameObject m_Text;
		private GameObject m_DividerLine;
		private GameObject m_IconTextRow;
		private GameObject m_IconTextIconRow;
		private GameObject m_RepeatingIconRow;
		//private GameObject m_LootInfo;
		//private GameObject m_ModInfo;

		private void Start()
		{
			m_HoverBox = (GameObject)Resources.Load("Prefab/MainUI/HoverBox/HBMain");
			m_Text = (GameObject)Resources.Load("Prefab/MainUI/HoverBox/Text");
			m_DividerLine = (GameObject)Resources.Load("Prefab/MainUI/HoverBox/DividerLine");
			m_IconTextRow = (GameObject)Resources.Load("Prefab/MainUI/HoverBox/IconTextRow");
			m_IconTextIconRow = (GameObject)Resources.Load("Prefab/MainUI/HoverBox/IconTextIconRow");
			m_RepeatingIconRow = (GameObject)Resources.Load("Prefab/MainUI/HoverBox/RepeatingIconRow");

			//m_LootInfo = (GameObject)Resources.Load("Prefab/MainUI/HoverBox/LootInfo");
			//m_ModInfo = (GameObject)Resources.Load("Prefab/MainUI/HoverBox/ModInfo");

			//pre-load objects to use
			for (int i = 0; i < 5; i++)
			{
				CreateNewText();
				CreateNewDividerLine();
				CreateNewIconTextRow();
				CreateNewIconTextIconRow();
				CreateNewRepeatingIconRow();
			}
		}

		//Currently not REALLY in use but can be later if we wish.
		public void CreateNewHoverBox()
		{
			var obj = Instantiate(m_HoverBox);
			obj.transform.SetParent(transform);
			obj.SetActive(false);
			m_HoverBoxList.Add(obj);
		}
		public void RecycleHoverBox(GameObject obj)
		{
			obj.SetActive(false);
			obj.transform.SetParent(transform);
			m_HoverBoxList.Add(obj);
		}
		public GameObject GetHoverBox()
		{
			return Instantiate(m_HoverBox);

			/*
			if (m_HoverBoxList.Count == 0)
				return Instantiate(m_HoverBox);

			var objToReturn = m_HoverBoxList[0];
			m_HoverBoxList.RemoveAt(0);
			objToReturn.SetActive(true);
			return objToReturn;
			*/
		}
		//

		public void CreateNewText()
		{
			var obj = Instantiate(m_Text);
			obj.transform.SetParent(transform, false);
			obj.SetActive(false);
			m_TextList.Add(obj);
		}
		public void RecycleText(GameObject obj)
		{
			obj.SetActive(false);
			obj.transform.SetParent(transform, false);
			m_TextList.Add(obj);
		}
		public GameObject GetText()
		{
			if (m_TextList.Count == 0)
			{
				return Instantiate(m_Text);
			}

			var objToReturn = m_TextList[0];
			m_TextList.RemoveAt(0);
			objToReturn.SetActive(true);
			return objToReturn;
		}

		public void CreateNewDividerLine()
		{
			var obj = Instantiate(m_DividerLine);
			obj.transform.SetParent(transform, false);
			obj.SetActive(false);
			m_DividerLineList.Add(obj);
		}
		public void RecycleDividerLine(GameObject obj)
		{
			obj.SetActive(false);
			obj.transform.SetParent(transform, false);
			m_DividerLineList.Add(obj);
		}
		public GameObject GetDividerLine()
		{
			if (m_DividerLineList.Count == 0)
			{
				return Instantiate(m_DividerLine);
			}

			var objToReturn = m_DividerLineList[0];
			m_DividerLineList.RemoveAt(0);
			objToReturn.SetActive(true);
			return objToReturn;
		}

		public void CreateNewIconTextRow()
		{
			var obj = Instantiate(m_IconTextRow);
			obj.transform.SetParent(transform, false);
			obj.SetActive(false);
			m_IconTextRowList.Add(obj);
		}
		
		public void CreateNewIconTextIconRow()
		{
			var obj = Instantiate(m_IconTextIconRow);
			obj.transform.SetParent(transform, false);
			obj.SetActive(false);
			m_IconTextIconRowList.Add(obj);
		}
		
		public void CreateNewRepeatingIconRow()
		{
			var obj = Instantiate(m_RepeatingIconRow);
			obj.transform.SetParent(transform, false);
			obj.SetActive(false);
			m_RepeatingIconRowList.Add(obj);
		}
		
		public void RecycleIconTextRow(GameObject obj)
		{
			foreach (Transform child in obj.transform)
				Destroy(child.gameObject);

			obj.SetActive(false);
			obj.transform.SetParent(transform, false);
			m_IconTextRowList.Add(obj);
		}
		
		public void RecycleIconTextIconRow(GameObject obj)
		{
			foreach (Transform child in obj.transform)
				Destroy(child.gameObject);

			obj.SetActive(false);
			obj.transform.SetParent(transform, false);
			m_IconTextIconRowList.Add(obj);
		}
		public GameObject GetIconTextRow()
		{
			if (m_IconTextRowList.Count == 0)
			{
				return Instantiate(m_IconTextRow);
			}

			var objToReturn = m_IconTextRowList[0];
			m_IconTextRowList.RemoveAt(0);
			objToReturn.SetActive(true);
			return objToReturn;
		}
		
		public GameObject GetIconTextIconRow()
		{
			if (m_IconTextIconRowList.Count == 0)
			{
				return Instantiate(m_IconTextIconRow);
			}

			var objToReturn = m_IconTextIconRowList[0];
			m_IconTextIconRowList.RemoveAt(0);
			objToReturn.SetActive(true);
			return objToReturn;
		}
		
		public GameObject GetRepeatingIconRow()
		{
			if (m_RepeatingIconRowList.Count == 0)
			{
				return Instantiate(m_RepeatingIconRow);
			}

			var objToReturn = m_RepeatingIconRowList[0];
			m_RepeatingIconRowList.RemoveAt(0);
			objToReturn.SetActive(true);
			return objToReturn;
		}

	}
}