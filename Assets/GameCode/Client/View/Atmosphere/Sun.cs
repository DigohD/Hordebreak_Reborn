using System.Collections;
using System.Collections.Generic;
using FNZ.Client.Net.NetworkManager;
using FNZ.Shared.Model.World;
using FNZ.Shared.Utils;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace FNZ.Client.View.Atmosphere 
{
	public class Sun : MonoBehaviour
	{
		private float m_TargetRotation;
		private float m_PrevTargetRotation;

		private float m_CurrentRotation;
		
		// Rotatio of transform at start of sunrise
		private Quaternion m_InitRotation;
		private float m_InitIntensity;

		private float hourTimer = 0;
		private byte m_CurrentHour = 0;

		[SerializeField]
		private HDAdditionalLightData m_Light;

		public static float DaylightIntensity;
		
		void Start()
	    {
		    m_InitRotation = transform.localRotation;
		    m_InitIntensity = m_Light.intensity;
		    DaylightIntensity = m_InitIntensity;
		    ClientWorldNetworkManager.d_NewHour += OnNewHour;
	    }
	
	    void Update()
	    {
		    hourTimer += Time.deltaTime;
		    hourTimer = hourTimer > EnvironmentShared.SECONDS_PER_HOUR ? EnvironmentShared.SECONDS_PER_HOUR : hourTimer;
		    transform.localRotation = m_InitRotation;
	        m_CurrentRotation = Mathf.Lerp( m_PrevTargetRotation, m_TargetRotation, hourTimer /  ((float) EnvironmentShared.SECONDS_PER_HOUR + 0.1f));
		    transform.Rotate(new Vector3(-m_CurrentRotation, 0, 0), Space.Self);


		    if (m_CurrentHour == 6)
		    {
			    var lightLerp = Mathf.Lerp(0, m_InitIntensity, hourTimer /  (float) EnvironmentShared.SECONDS_PER_HOUR);
			    m_Light.intensity = FNEMath.ExpoCurve(100000, 5, lightLerp);		    
			    var shadowLerp = Mathf.Lerp(0, 1, hourTimer /  (float) EnvironmentShared.SECONDS_PER_HOUR);
			    m_Light.shadowDimmer = FNEMath.ExpoCurve(1, 0.05f, shadowLerp);	
		    }
		    else if (m_CurrentHour == 20)
		    {
			    var lightLerp = Mathf.Lerp(m_InitIntensity, 0, hourTimer /  (float) EnvironmentShared.SECONDS_PER_HOUR);
			    m_Light.intensity = FNEMath.ExpoCurve(100000, 5, lightLerp);
			    var shadowLerp = Mathf.Lerp(1, 0, hourTimer /  (float) EnvironmentShared.SECONDS_PER_HOUR);
			    m_Light.shadowDimmer = FNEMath.ExpoCurve(1, 0.05f, shadowLerp);			    }
	    }

	    private void OnNewHour(byte hour)
	    {
		    m_CurrentHour = hour;
		    hourTimer = 0;
		    if (hour >= 6 && hour <= 20)
		    {
			    float lerpValue = (hour - 6f) / 15f;
			    m_PrevTargetRotation = m_TargetRotation;
			    m_TargetRotation = Mathf.Lerp(0, 120, lerpValue);
			  
			  
			    m_Light.EnableShadows(true);
		    }else
		    {
			    m_PrevTargetRotation = 0;
			    m_TargetRotation = 0;
			    
			    m_Light.EnableShadows(false);
		    }
	    }
	}
}