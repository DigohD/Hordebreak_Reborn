using Assets.LevelEditor.Code.LevelEditor.MapEditor;
using FNZ.LevelEditor;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.MountedObject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.LevelEditor.Code.LevelEditorV2 
{
	public class WallObjectsUI : MonoBehaviour
	{
		public GameObject P_WallObjectsButton;
		public Transform T_WallObjectsList;
		public Text TXT_WallObjectsCategoryName;
		public Image IMG_ExpandArrow;
		public Sprite S_Expanded, S_Collapsed;

		private bool m_Expanded = true;
		private List<MountedObjectData> m_WallObjectsList;
		private string m_ListTitle;

		public void GenerateList(List<MountedObjectData> wallObjectsList, string listTitle)
		{
			m_WallObjectsList = wallObjectsList;
			m_ListTitle = listTitle;

			TXT_WallObjectsCategoryName.text = listTitle;

			foreach (var wallObject in wallObjectsList)
			{
				var newButton = Instantiate(P_WallObjectsButton);

				newButton.GetComponent<ME_WallObjectsButton>().MountedObjectType = wallObject.Id;
				newButton.GetComponentInChildren<Text>().text =
					wallObject.editorName == null || wallObject.editorName.Equals("")  ? wallObject.Id : wallObject.editorName;

				newButton.transform.SetParent(T_WallObjectsList);
			}
		}

		public void ToggleExpand()
		{
			m_Expanded = !m_Expanded;

			if (m_Expanded)
			{
				GenerateList(m_WallObjectsList, m_ListTitle);
				IMG_ExpandArrow.sprite = S_Expanded;
			}
			else
			{
				List<Transform> toDestroy = new List<Transform>();
				foreach (Transform t in T_WallObjectsList)
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