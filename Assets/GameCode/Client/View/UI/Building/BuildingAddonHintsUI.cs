using System.Collections;
using System.Collections.Generic;
using FNZ.Client.Model.Entity.Components.InteractionEvent;
using FNZ.Client.Utils;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.WorldEvent;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Client.View.UI.Building 
{

	public class BuildingAddonHintsUI : FNESingleton<BuildingAddonHintsUI>
	{
		private FNEEntity m_Player;

		[SerializeField] private GameObject P_AddonMarker;
		
		private Dictionary<float2, GameObject> m_ActiveMarkers = new Dictionary<float2, GameObject>();
		
	    void Update()
	    {
		    UpdateMarkerPositions();
	    }
	    
	    public void UpdateTileObjectEventTriggers(List<GameObject> hitBoxes)
		{
			foreach (var hitBox in hitBoxes)
			{
				var newMarker = Instantiate(P_AddonMarker);
				newMarker.transform.SetParent(transform, false);
				
				m_ActiveMarkers.Add(
					new float2(
						hitBox.transform.position.x, 
						hitBox.transform.position.z
					), 
					newMarker
				);
				
				newMarker.transform.position = UnityEngine.Camera.main.WorldToScreenPoint(
					new Vector3(
						hitBox.transform.position.x, 
						1,
						hitBox.transform.position.z
					),
					UnityEngine.Camera.MonoOrStereoscopicEye.Mono
				);
			}
		}

	    public void ClearAddonHints()
	    {
		    foreach (var marker in m_ActiveMarkers.Values)
		    {
			    Destroy(marker);
		    }
			
		    m_ActiveMarkers.Clear();
	    }
	    
	    public void UpdateEdgebjectEventTriggers(List<GameObject> hitBoxes)
	    {
		    foreach (var hitBox in hitBoxes)
		    {
			    var newMarker = Instantiate(P_AddonMarker);
			    newMarker.transform.SetParent(transform, false);
				
			    
			    m_ActiveMarkers.Add(
				    new float2(
					    hitBox.transform.position.x, 
					    hitBox.transform.position.z
				    ), 
				    newMarker
			    );
			    
			    newMarker.transform.position = UnityEngine.Camera.main.WorldToScreenPoint(
				    new Vector3(
					    hitBox.transform.position.x, 
					    1,
					    hitBox.transform.position.z
				    ),
				    UnityEngine.Camera.MonoOrStereoscopicEye.Mono
			    );
		    }
	    }

	    private void UpdateMarkerPositions()
	    {
		    foreach (var marker in m_ActiveMarkers.Keys)
		    {
			    m_ActiveMarkers[marker].transform.position = UnityEngine.Camera.main.WorldToScreenPoint(
				    new Vector3(
					    marker.x,
					    1,
					    marker.y
				    ),
				    UnityEngine.Camera.MonoOrStereoscopicEye.Mono
			    );
		    }
	    }
	}
}