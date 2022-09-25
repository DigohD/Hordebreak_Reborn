using System.Collections;
using System.Collections.Generic;
using FNZ.Client.Model.Entity.Components.InteractionEvent;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.WorldEvent;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Client.View.UI.EventTriggeUI 
{

	public class EventTriggerUI : MonoBehaviour
	{
		private FNEEntity m_Player;
		
		private Dictionary<int2, GameObject> m_ActiveMarkers = new Dictionary<int2, GameObject>();

		private int2 m_PreviousPlayerTilePos;

		[SerializeField] private GameObject P_EventMarker;
		
	    void Update()
	    {
		    if (m_Player == null && GameClient.LocalPlayerEntity == null)
			    return;
		    else if (m_Player == null)
			    m_Player = GameClient.LocalPlayerEntity;
		    
		    var pos = m_Player.Position;
		    var posInt2 = new int2((int)pos.x, (int)pos.y);
		    
		    // If the player has entered a new tile: recalculate
		    if (!posInt2.Equals(m_PreviousPlayerTilePos))
		    {
			    m_PreviousPlayerTilePos = posInt2;
			    UpdateTileObjectEventTriggers();
		    }

		    UpdateMarkerPositions();
	    }
	    
	    private void UpdateTileObjectEventTriggers()
		{
			foreach (var marker in m_ActiveMarkers.Values)
			{
				Destroy(marker);
			}
			
			m_ActiveMarkers.Clear();

			var pos = m_Player.Position;
			var posInt2 = new int2((int)pos.x, (int)pos.y);
			
			var tiles = GameClient.World.GetSurroundingTilesInRadius(posInt2, 10);
			foreach (var tilePos in tiles)
			{
				var tileObject = GameClient.World.GetTileObject(tilePos.x, tilePos.y);

				if (tileObject == null)
					continue;

				var eventComp = tileObject.GetComponent<InteractionEventComponentClient>();
				if (eventComp == null)
					continue;
				
				if(string.IsNullOrEmpty(eventComp.Data.eventRef))
					continue;

				var eventData = DataBank.Instance.GetData<WorldEventData>(eventComp.Data.eventRef);

				var newMarker = Instantiate(P_EventMarker);
				newMarker.transform.SetParent(transform, false);
				
				m_ActiveMarkers.Add(tilePos, newMarker);
			}
		}

	    private void UpdateMarkerPositions()
	    {
		    foreach (var marker in m_ActiveMarkers.Keys)
		    {
			    m_ActiveMarkers[marker].transform.position = UnityEngine.Camera.main.WorldToScreenPoint(
				    new Vector3(
					    marker.x + 0.5f,
					    1,
					    marker.y + 0.5f
				    ),
				    UnityEngine.Camera.MonoOrStereoscopicEye.Mono
			    );
		    }
	    }
	}
}