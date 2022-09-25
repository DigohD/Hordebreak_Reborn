using FNZ.Shared.Model.Entity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace FNZ.LevelEditor
{
	public class WallsUI : MonoBehaviour
	{
		public GameObject P_WallButton;
		public Transform T_WallList;
		public Text TXT_WallCategoryName;
		public Image IMG_ExpandArrow;
		public Sprite S_Expanded, S_Collapsed;

		private bool m_Expanded = true;
		private List<FNEEntityData> m_WallList;
		private string m_ListTitle;

		public void GenerateList(List<FNEEntityData> wallList, string listTitle)
		{
			m_WallList = wallList;
			m_ListTitle = listTitle;

			TXT_WallCategoryName.text = listTitle;

			foreach (var wall in wallList)
			{
				var newButton = Instantiate(P_WallButton);

				newButton.GetComponent<ME_WallButton>().WallType = wall.Id;
				newButton.GetComponentInChildren<Text>().text =
					wall.editorName.Equals("") || wall.editorName == null ? wall.Id : wall.editorName;

				newButton.transform.SetParent(T_WallList);
			}
		}

		public void ToggleExpand()
		{
			m_Expanded = !m_Expanded;

			if (m_Expanded)
			{
				GenerateList(m_WallList, m_ListTitle);
				IMG_ExpandArrow.sprite = S_Expanded;
			}
			else
			{
				List<Transform> toDestroy = new List<Transform>();
				foreach (Transform t in T_WallList)
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
