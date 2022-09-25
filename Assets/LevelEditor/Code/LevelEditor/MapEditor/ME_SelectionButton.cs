using FNZ.LevelEditor;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.LevelEditor.Code.LevelEditor.MapEditor 
{

	public class ME_SelectionButton : MonoBehaviour
	{

		public void Start()
		{
			
		}

		public void Update()
		{
			if (ME_Control.activePaintMode == ME_Control.PaintMode.EXPORT_SELECTION)
				GetComponent<Image>().color = new Color(0.1f, 0.6f, 0.1f);
			else
				GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f);
		}

		public void OnClick()
		{
			ME_Control.activePaintMode = ME_Control.PaintMode.EXPORT_SELECTION;
			ME_Control.activeSnapMode = ME_Control.SnapMode.TILE;
		}
	}
}