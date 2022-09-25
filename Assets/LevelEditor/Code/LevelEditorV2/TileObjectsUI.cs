using FNZ.Shared.Model.Entity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.LevelEditor
{
	public class TileObjectsUI : MonoBehaviour
	{
		public GameObject P_TileObjectButton;
		public Transform T_TileObjectList;
		public Text TXT_TileObjectCategoryName;
		public Image IMG_ExpandArrow;
		public Sprite S_Expanded, S_Collapsed;

		private bool m_Expanded = true;
		private List<FNEEntityData> m_TileObjectList;
		private string m_ListTitle;

		public void GenerateList(List<FNEEntityData> tileObjectList, string listTitle)
		{
			m_TileObjectList = tileObjectList;
			m_ListTitle = listTitle;

			TXT_TileObjectCategoryName.text = listTitle;

			foreach (var tileObject in tileObjectList)
			{
				var newButton = Instantiate(P_TileObjectButton);

				newButton.GetComponent<ME_TOButton>().toType = tileObject.Id;
				newButton.GetComponentInChildren<Text>().text =
					string.IsNullOrEmpty(tileObject.editorName) ? tileObject.Id : tileObject.editorName;

				newButton.transform.SetParent(T_TileObjectList);
			}
		}

		public void ToggleExpand()
		{
			m_Expanded = !m_Expanded;

			if (m_Expanded)
			{
				GenerateList(m_TileObjectList, m_ListTitle);
				IMG_ExpandArrow.sprite = S_Expanded;
			}
			else
			{
				List<Transform> toDestroy = new List<Transform>();
				foreach (Transform t in T_TileObjectList)
				{
					toDestroy.Add(t);
				}
				foreach (Transform t in toDestroy)
				{
					t.SetParent(null);
					Destroy(t.gameObject);
				}
				IMG_ExpandArrow.sprite = S_Collapsed;
			}

			foreach (RectTransform t in transform.parent)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(t);
			}

			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent);


		}
	}
}