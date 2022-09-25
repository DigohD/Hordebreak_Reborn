using System.Collections.Generic;
using FNZ.Client.Utils;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.WorldEvent;
using FNZ.Shared.Utils;
using GameCode.Client.Net.NetworkManager;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Client.View.UI.WorldEventUI 
{
	public class WorldEventUIManager : MonoBehaviour
	{
		private struct ActiveEvent
		{
			public bool IsTimed;
			public float2 Position;
			public float SecondsLeft;
			public float Radius;
			public string Title;
			public string Description;
			public long UniqueId;
		}
		
		private List<ActiveEvent> m_ActiveEvents = new List<ActiveEvent>();
		
		[SerializeField]
		private Transform T_Timer;
		[SerializeField]
		private Transform T_Hint;
		[SerializeField]
		private Transform T_Title;
		[SerializeField]
		private Transform T_Top;
		
		
		[SerializeField]
		private TextMeshProUGUI TXT_Timer;
		[SerializeField]
		private TextMeshProUGUI TXT_Hint;
		[SerializeField]
		private TextMeshProUGUI TXT_Title;
		
		private FNEEntity m_Player;

		private ActiveEvent m_ActiveProximityEvent;

		private GameObject m_EventOutLine;
		private float m_TopTargetY = 100;
		private float m_HintTargetY = -100;
		
		private float m_OutlineTargetScale = 0;

		private float m_UISwitchTimer = 0;
		
	    void Start()
	    {
		    ClientWorldEventNetworkManager.d_OnEventReceived += OnEventReceived;
		    ClientWorldEventNetworkManager.d_OnEventFinishedReceived += OnEventFinishedReceived;

		    var EventOutlinePrefab = Resources.Load<GameObject>("Prefab/Entity/Player/BaseOutline");
		    m_EventOutLine = Instantiate(EventOutlinePrefab);
		    m_EventOutLine.SetActive(false);
	    }

	    private void OnEventFinishedReceived(WorldEventFinishedReceived worldeventfinished)
	    {
		    m_ActiveEvents.RemoveAll(x => x.UniqueId == worldeventfinished.Id);
		    if (m_ActiveProximityEvent.UniqueId == worldeventfinished.Id)
		    {
			    m_ActiveProximityEvent = default;
			    HideUI();
		    }
	    }

	    void Update()
	    {
		    if (m_Player == null && GameClient.LocalPlayerEntity == null)
		    {
			    return;
		    }
		    else if (m_Player == null)
		    {
			    m_Player = GameClient.LocalPlayerEntity;
		    }

		    m_UISwitchTimer += Time.deltaTime;
			LerpAnchors();
		    
			var tmp = m_ActiveProximityEvent;
			tmp.SecondsLeft -= Time.deltaTime;
			m_ActiveProximityEvent = tmp;
		    if (m_ActiveProximityEvent.SecondsLeft < 0)
			    m_ActiveProximityEvent.SecondsLeft = 0;
		    
		    for (int i = 0; i < m_ActiveEvents.Count; i++)
		    {
			    tmp = m_ActiveEvents[i];
			    tmp.SecondsLeft -= Time.deltaTime;
			    if (tmp.SecondsLeft < 0)
				    tmp.SecondsLeft = 0;
			    m_ActiveEvents[i] = tmp;
		    }

		    if (!string.IsNullOrEmpty(m_ActiveProximityEvent.Title))
		    {
			    if (!IsPlayerWithinEventRadius(m_ActiveProximityEvent))
			    {
				    m_ActiveProximityEvent = default;
				    HideUI();
			    }
			    else
			    {
				    SetTime();
			    }
		    }
		    else
		    {
			    foreach (var activeEvent in m_ActiveEvents)
			    {
				    if (IsPlayerWithinEventRadius(activeEvent))
				    {
					    m_ActiveProximityEvent = activeEvent;
					    ShowUI();
				    }
			    }
		    }
	    }

	    private void HideUI()
	    {
		    T_Hint.gameObject.SetActive(false);
		    T_Timer.gameObject.SetActive(false);
		    T_Title.gameObject.SetActive(false);
		    
		    if(m_EventOutLine != null)
			    m_EventOutLine.SetActive(false);
		    
			m_TopTargetY = 100;
		    m_HintTargetY = -100;

		    m_OutlineTargetScale = 0f;
		    m_UISwitchTimer = 0;
	    }

	    private void ShowUI()
	    {
		    TXT_Title.text = m_ActiveProximityEvent.Title;
		    TXT_Hint.text = m_ActiveProximityEvent.Description;

			SetTime();
		    
		    T_Hint.gameObject.SetActive(true);
		    T_Timer.gameObject.SetActive(true);
		    T_Title.gameObject.SetActive(true);
		    
		    var diameter = m_ActiveProximityEvent.Radius + m_ActiveProximityEvent.Radius + 1f;
		    
		    if(m_EventOutLine != null)
			    m_EventOutLine.SetActive(false);
		    
		    m_EventOutLine.SetActive(true);
		    m_EventOutLine.GetComponent<MeshFilter>().sharedMesh = ViewUtils.GetFramedPlane(
			    (int) diameter,
			    (int) diameter
		    );

		    m_EventOutLine.transform.position = new Vector3(
			    m_ActiveProximityEvent.Position.x,
			    0,
			    m_ActiveProximityEvent.Position.y
		    );
		    m_EventOutLine.transform.localScale = Vector3.zero;

		    m_OutlineTargetScale = 1f;
		    
		    m_TopTargetY = -20;
		    m_HintTargetY = 20;

		    m_UISwitchTimer = 0;
	    }

	    private void SetTime()
	    {
		    var time = (int) m_ActiveProximityEvent.SecondsLeft;
		    var seconds = time % 60;
		    var minutes = time / 60;

		    TXT_Timer.text = minutes + ":" + seconds;
	    }
	    
	    private void LerpAnchors()
	    {
		    m_EventOutLine.transform.localScale = Vector3.Lerp(
			    m_EventOutLine.transform.localScale,
			    Vector3.one * m_OutlineTargetScale,
			    5 * Time.deltaTime
		    );

		    var diameter = m_ActiveProximityEvent.Radius + m_ActiveProximityEvent.Radius + 1f;

		    var eventPos = new Vector3(m_ActiveProximityEvent.Position.x, 0, m_ActiveProximityEvent.Position.y);
		    var targetPos = new Vector3(
			    m_ActiveProximityEvent.Position.x + 0.5f - (diameter / 2f),
			    0,
			    m_ActiveProximityEvent.Position.y + 0.5f - (diameter / 2f)
		    );
		    
		    m_EventOutLine.transform.position = Vector3.Lerp(
			    eventPos,
			    targetPos,
			    m_EventOutLine.transform.localScale.x
		    );

		    if (m_UISwitchTimer > 0.3f)
		    {
			    T_Top.transform.localPosition = Vector2.Lerp(
				    T_Top.transform.localPosition,
				    new Vector2(T_Top.transform.localPosition.x, m_TopTargetY),
				    10 * Time.deltaTime
			    );
		    }

		    if (m_UISwitchTimer > 0.6f)
		    {
			    T_Hint.transform.localPosition = Vector2.Lerp(
				    T_Hint.transform.localPosition,
				    new Vector2(T_Hint.transform.localPosition.x, m_HintTargetY),
				    10 * Time.deltaTime
			    );
		    }
		    
		    
	    }
	    
	    private void OnEventReceived(WorldEventReceivedData worldEvent)
	    {
		    switch (worldEvent.Type)
		    {
			    case WorldEventType.Survival:
				    var duration = worldEvent.Data.Duration;
					
				    var startSeconds = (float) (worldEvent.StartTimeStamp / 1000000000.0);
				    var nowSeconds = FNEUtil.NanoTime() / 1000000000f;

				    var newEvent = new ActiveEvent
				    {
					    IsTimed = true,
					    SecondsLeft = duration - (nowSeconds - startSeconds),
					    Radius = worldEvent.Data.PlayerRangeRadius,
					    Title = Localization.GetString(worldEvent.Data.NameRef),
					    Description = Localization.GetString(worldEvent.Data.DescriptionRef),
					    Position = worldEvent.Position,
					    UniqueId = worldEvent.Id
				    };
				    
				    m_ActiveEvents.Add(newEvent);
				    break;
		    }
	    }

	    private bool IsPlayerWithinEventRadius(ActiveEvent activeEvent)
	    {
		    var pos = m_Player.Position;
		    var eventPos = activeEvent.Position + new float2(0.5f, 0.5f);
		    var radius = activeEvent.Radius + 0.5f;
		    if (pos.x >= eventPos.x - radius &&
		        pos.x <= eventPos.x + radius &&
		        pos.y >= eventPos.y - radius &&
		        pos.y <= eventPos.y + radius)
		    {
			    return true;
		    }

		    return false;
	    }
	}
}