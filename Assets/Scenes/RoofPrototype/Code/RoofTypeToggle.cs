using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scenes.RoofPrototype.Code 
{

	public class RoofTypeToggle : MonoBehaviour
	{
		public RoofType RoofType;
		private SceneControl sceneControl;

		public Image IMG_Checkmark;

		void Start()
		{
			sceneControl = FindObjectOfType<SceneControl>();
			sceneControl.d_RoofTypeChange += ReRender;
			IMG_Checkmark.enabled = RoofType == sceneControl.RoofType;
		}

        public void OnClick()
        {
			sceneControl.SelectHouseType(RoofType);
		}

		private void ReRender(RoofType rooftype)
        {
			IMG_Checkmark.enabled = RoofType == sceneControl.RoofType;
		}
	}
}