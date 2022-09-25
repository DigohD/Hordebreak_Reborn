using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FNZ.Client.Utils 
{
	public class TMP_DayNightSwitch : MonoBehaviour
	{
		public GameObject sun, moon, castOver, dusk;

		private short lightType = 0;
		
		void Update()
	    {
		    if((Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.T)) || Input.GetKeyDown(KeyCode.F7))
		    {
				lightType += 1;
				if (lightType == 3)
				{
					lightType = 0;
				}

				switch (lightType)
				{
					case 0:
						sun.gameObject.SetActive(true);
						castOver.gameObject.SetActive(false);
						dusk.gameObject.SetActive(false);
						moon.gameObject.SetActive(false);
						break;
					case 1:
						sun.gameObject.SetActive(false);
						castOver.gameObject.SetActive(false);
						dusk.gameObject.SetActive(true);
						moon.gameObject.SetActive(false);
						break;
					case 2:
						sun.gameObject.SetActive(false);
						castOver.gameObject.SetActive(false);
						dusk.gameObject.SetActive(false);
						moon.gameObject.SetActive(true);
						break;
					//case 3:
					//	sun.gameObject.SetActive(false);
					//	// castOver.gameObject.SetActive(false);
					//	dusk.gameObject.SetActive(false);
					//	moon.gameObject.SetActive(true);
					//	break;
					default:
						sun.gameObject.SetActive(true);
						castOver.gameObject.SetActive(false);
						dusk.gameObject.SetActive(false);
						moon.gameObject.SetActive(false);
						break;
				}
			    //sun.gameObject.SetActive(!sun.gameObject.activeInHierarchy);
			    //moon.gameObject.SetActive(!moon.gameObject.activeInHierarchy);
		    }
	    }
	}
}