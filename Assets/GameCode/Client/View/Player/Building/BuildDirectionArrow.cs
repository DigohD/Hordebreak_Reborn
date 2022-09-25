using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FNZ.Client.View.Player.Building 
{

	public class BuildDirectionArrow : MonoBehaviour
	{
		float timer = 0;

	    void Update()
	    {
			timer += Time.deltaTime;
			transform.localPosition = new Vector3(
				0,
				transform.localPosition.y,
				-(0.25f + ((Mathf.Sin(timer * 12f) + 1) / 6f)) * transform.localScale.x
			);
	    }
	}
}