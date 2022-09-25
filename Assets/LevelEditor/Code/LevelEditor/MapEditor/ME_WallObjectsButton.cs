using FNZ.LevelEditor;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.MountedObject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.LevelEditor.Code.LevelEditor.MapEditor 
{

	public class ME_WallObjectsButton : MonoBehaviour
	{
		public string MountedObjectType;

		public void Start()
		{
			var data = DataBank.Instance.GetData<MountedObjectData>(MountedObjectType);
		}

		public void Update()
		{
			if ((ME_Control.activePaintMode == ME_Control.PaintMode.MOUNTED_OBJECT)
				&& ME_Control.SelectedMountedObject == MountedObjectType)
				GetComponent<Image>().color = new Color(0.1f, 0.6f, 0.1f);
			else
				GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f);
		}

		public void OnClick()
		{
			ME_Control.activePaintMode = ME_Control.PaintMode.MOUNTED_OBJECT;
			ME_Control.SelectedMountedObject = MountedObjectType;
		}
	}
}