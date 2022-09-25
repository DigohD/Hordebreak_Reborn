using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scenes.RoofPrototype.Code 
{

	public class SymmetricXModulo : MonoBehaviour
	{
		void Start()
		{
			GetComponentInChildren<InputField>().SetTextWithoutNotify("2");
			SceneControl.SymetricXModulo = 2;
		}

		public void OnClick()
		{

		}

		public void OnModuloChange(string modulo)
		{
			try
			{
				SceneControl.SymetricXModulo = int.Parse(modulo);
			}catch(Exception e)
			{
				GetComponentInChildren<InputField>().SetTextWithoutNotify("");
				Debug.LogError(e);
			}
		}
	}
}