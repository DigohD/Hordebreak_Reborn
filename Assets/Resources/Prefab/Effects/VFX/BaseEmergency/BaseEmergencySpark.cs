using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Resources.Prefab.Effects.VFX.BaseEmergency 
{

	public class BaseEmergencySpark : MonoBehaviour
	{
		public GameObject sparkCylinder;
		public float rotationSpeed;

        private float lifetime = 5;

	    // Start is called before the first frame update
	    void Start()
	    {
	        if (sparkCylinder == null)
			{
				return;
			}
	    }
	
	    // Update is called once per frame
	    void Update()
	    {
			if (rotationSpeed == 0)
			{
				rotationSpeed = 15;
			}
			sparkCylinder.transform.Rotate(0, rotationSpeed * Time.deltaTime, 0, 0);

            lifetime -= Time.deltaTime;
            if (lifetime <= 0)
                Destroy(transform.parent.gameObject);
        }
	}
}