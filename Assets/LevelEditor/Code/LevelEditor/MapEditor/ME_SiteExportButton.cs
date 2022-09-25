using FNZ.LevelEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.LevelEditor.Code.LevelEditor.MapEditor 
{

	public class ME_SiteExportButton : MonoBehaviour
	{
		public void Update()
		{
			if (!ME_Control.IsExportSelectionValid())
			{
				GetComponent<Button>().interactable = false;
			}
			else
			{
				GetComponent<Button>().interactable = true;
			}
		}
	}
}