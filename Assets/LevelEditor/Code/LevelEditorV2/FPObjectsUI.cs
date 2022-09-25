using FNZ.Shared.Model.Entity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace FNZ.LevelEditor
{
	public class FPObjectsUI : MonoBehaviour
	{
		public GameObject P_FPObjectButton;
		public Transform T_FPObjectList;
		public Text TXT_FPObjectCategoryName;
		public Image IMG_ExpandArrow;
		public Sprite S_Expanded, S_Collapsed;

		private bool m_Expanded = true;
		private List<FNEEntityData> m_FPObjectList;
		private string m_ListTitle;

		public void GenerateList(List<FNEEntityData> FPObjectList, string listTitle)
		{
			m_FPObjectList = FPObjectList;
			m_ListTitle = listTitle;

			TXT_FPObjectCategoryName.text = listTitle;

			foreach (var FPObject in FPObjectList)
			{
				var newButton = Instantiate(P_FPObjectButton);

				newButton.GetComponent<ME_FPButton>().fpType = FPObject.Id;
				newButton.GetComponentInChildren<Text>().text =
					FPObject.editorName.Equals("") || FPObject.editorName == null ? FPObject.Id : FPObject.editorName;

				newButton.transform.SetParent(T_FPObjectList);
			}
		}

		public void ToggleExpand()
		{
			m_Expanded = !m_Expanded;

			if (m_Expanded)
			{
				GenerateList(m_FPObjectList, m_ListTitle);
				IMG_ExpandArrow.sprite = S_Expanded;
			}
			else
			{
				List<Transform> toDestroy = new List<Transform>();
				foreach (Transform t in T_FPObjectList)
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