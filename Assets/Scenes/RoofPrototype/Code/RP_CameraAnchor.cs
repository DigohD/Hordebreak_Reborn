using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scenes.RoofPrototype.Code 
{

	public class RP_CameraAnchor : MonoBehaviour
	{
	    void Update()
	    {
			transform.Rotate(Vector3.up, 15f * Time.deltaTime);
	    }
	}
}