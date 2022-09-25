using System.Collections.Generic;
using FNZ.Client.Model.World;
using FNZ.Client.Net.NetworkManager;
using FNZ.Client.View.UI.HoverBox;
using FNZ.Client.View.UI.Manager;
using FNZ.Client.View.UI.Sprites;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.World;
using FNZ.Shared.Model.World.Site;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.WorldMap 
{

	public class WorldMapUI : MonoBehaviour
	{
		[SerializeField]
		private RectTransform T_Map;
		
		[SerializeField]
		private Transform T_BasesParent;
		[SerializeField]
		private Transform T_PlayersParent;
		[SerializeField]
		private Transform T_SitesParent;
		[SerializeField]
		private Transform T_LandmarksParent;
		[SerializeField]
		private Transform T_QuestParent;

		[SerializeField]
		private RectTransform T_MapBG;
		
		[SerializeField] private RawImage IMG_TileMap;
		[SerializeField] private RawImage IMG_TileMapChunk;
		[SerializeField] private RawImage IMG_MaskedMap;

		[SerializeField] private GameObject P_PlayerMarker;
		[SerializeField] private GameObject P_SiteMarker;
		[SerializeField] private GameObject P_SiteMarkerFull;
		[SerializeField] private GameObject P_BaseMarker;
		
		private GameObject m_LocalPlayerMarker;
		private Dictionary<int, GameObject> m_RemotePlayerMarkers = new Dictionary<int, GameObject>();
		private Dictionary<int, GameObject> m_SiteMarkers = new Dictionary<int, GameObject>();
		private Dictionary<long, GameObject> m_BaseMarkers = new Dictionary<long, GameObject>();
		private Dictionary<float2, GameObject> m_QuestMarkers = new Dictionary<float2, GameObject>();

		public static float2[] m_MapHighlights = new float2[0];
		
		private int m_MaxZoomIn = 100000;
		private int m_MaxZoomOut = 1000;
		private int m_ZoomLevel = 80000;

		private Vector2 m_InitPosition, m_Translation;
		private Vector2 m_PrevDragPos;
		
		private float m_MarkerTranspose = 80000f / 8192f;

		private Sprite m_PlayerMarker, m_selfPlayerMarker,
			m_BaseMarker,
			m_SiteMarker,
			m_LandmarkMarker,
			m_UnknownMarker;

		private RawImage[,] TileMapParts = new RawImage[8, 8];
		
		void Start()
	    {
		    m_PlayerMarker = SpriteBank.GetSprite("map_icon_player");
		    m_LandmarkMarker = SpriteBank.GetSprite("map_icon_landmark");
		    m_BaseMarker = SpriteBank.GetSprite("map_icon_base_flag");
		    m_selfPlayerMarker = SpriteBank.GetSprite("map_icon_player_self");
		    m_SiteMarker = SpriteBank.GetSprite("map_icon_site_factory");
		    m_UnknownMarker = SpriteBank.GetSprite("map_icon_undiscovered");
		    
		    for (var y = 0; y < 8; y++)
		    {
			    for (var x = 0; x < 8; x++)
			    {
				    var go = Instantiate(IMG_TileMapChunk.gameObject);
				    go.transform.SetParent(IMG_TileMap.transform, false);
				    go.transform.localPosition = new Vector3(x * 1024, y * 1024, 0);
				    go.GetComponentInChildren<RawImage>().texture = ClientMapManager.GetMapPartTexture(x, y);
				    TileMapParts[x, y] = go.GetComponentInChildren<RawImage>();
			    }
		    }

		    IMG_TileMap.transform.localScale =  Vector3.one * m_MarkerTranspose;
		    
		    m_LocalPlayerMarker= Instantiate(P_PlayerMarker);
		    m_LocalPlayerMarker.transform.SetParent(T_PlayersParent, false);
		    m_LocalPlayerMarker.transform.localPosition = (Vector2) GameClient.LocalPlayerEntity.Position * m_MarkerTranspose;
		    m_LocalPlayerMarker.GetComponent<Image>().sprite = m_selfPlayerMarker;
		    
		    var center = new float2(1, 1) * (m_ZoomLevel / 2f);
		    var diff = (GameClient.LocalPlayerEntity.Position * m_MarkerTranspose) - center;
		    var correctedDiff = Quaternion.Euler(0, 0, 45) * (Vector2) diff;
		    T_Map.transform.localPosition = -correctedDiff;
		    m_InitPosition = T_Map.transform.localPosition;
		    
		    T_MapBG.sizeDelta = new Vector2(m_ZoomLevel, m_ZoomLevel);
		    
		    InitEvents();
		    TestMarkers();
	    }

		void InitEvents()
		{
			EventTrigger trigger = GetComponentInChildren<EventTrigger>();
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.Drag;
			entry.callback.AddListener((data) => { OnMouseDrag((PointerEventData)data); });
			trigger.triggers.Add(entry);
			
			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.BeginDrag;
			entry.callback.AddListener((data) => { OnBeginMouseDrag((PointerEventData)data); });
			trigger.triggers.Add(entry);

			ClientWorldNetworkManager.d_OnSiteMapUpdate += OnSiteMapUpdate;
			ClientQuestNetworkManager.d_OnQuestUpdateReceived += SetObjective;
		}
		
		
		
	    void Update()
	    {
		    UpdateZoom();
		    UpdateTileMap();
		    UpdatePlayers();
		    UpdateMarkers();
		    UpdateMapTranslation();
	    }

	    private void LateUpdate()
	    {
		    
	    }

	    void UpdateTileMap()
	    {
		    for (var y = 0; y < 8; y++)
		    {
			    for (var x = 0; x < 8; x++)
			    {
				    TileMapParts[x, y].texture = ClientMapManager.GetMapPartTexture(x, y);
			    }
		    }
			//IMG_TileMap.texture = ClientMapManager.GetTileMap();
			// IMG_MaskedMap.texture = ClientMapManager.GetMaskedTileMap();
		}
	    
	    void UpdatePlayers()
	    {
		    m_LocalPlayerMarker.transform.localPosition = (Vector2) GameClient.LocalPlayerEntity.Position * m_MarkerTranspose;
		    
		    var players = GameClient.RemotePlayerEntities;
		    foreach (var player in players)
		    {
			    if (!m_RemotePlayerMarkers.ContainsKey(player.NetId))
			    {
				    var newMarker = Instantiate(P_PlayerMarker);
				    newMarker.transform.SetParent(T_PlayersParent, false);
				    m_RemotePlayerMarkers.Add(player.NetId, newMarker);
				    newMarker.GetComponent<Image>().color = new Color(0, 0.7f, 1f);
				    newMarker.GetComponent<Image>().sprite = m_PlayerMarker;
				    
				    EventTrigger trigger = newMarker.GetComponentInChildren<EventTrigger>();
				    EventTrigger.Entry entry = new EventTrigger.Entry();
				    entry.eventID = EventTriggerType.PointerEnter;
				    entry.callback.AddListener((data) =>
				    {
					    UIManager.HoverBoxGen.CreateMapPlayerHoverBox(player);
				    });
				    trigger.triggers.Add(entry);
			
				    entry = new EventTrigger.Entry();
				    entry.eventID = EventTriggerType.PointerExit;
				    entry.callback.AddListener((data) =>
				    {
					    HB_Factory.DestroyHoverbox();
				    });			    
				    trigger.triggers.Add(entry);
			    }
		    }
		    
		    foreach (var netID in m_RemotePlayerMarkers.Keys)
		    {
			    var playerEntity = GameClient.NetConnector.GetEntity(netID);
			    m_RemotePlayerMarkers[netID].transform.localPosition = (Vector2) playerEntity.Position * m_MarkerTranspose;
		    }
	    }

	    private void UpdateZoom()
	    {
		    var screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
		    var mousePos = UnityEngine.Input.mousePosition;

		    Vector2 preCord = Vector2.one;
		    
		    bool didZoom = false;
		    if (UnityEngine.Input.GetAxis("Mouse ScrollWheel") > 0 && m_ZoomLevel < m_MaxZoomIn)
		    {
			    var PrevZoomLevel = m_ZoomLevel;
			    
			    m_ZoomLevel += m_ZoomLevel / 10;
			    
			    RectTransformUtility.ScreenPointToLocalPointInRectangle(
				    T_Map, 
				    mousePos, 
				    null, 
				    out var mousePosPreZoom
			    );

			    preCord = mousePosPreZoom / m_MarkerTranspose;
			    
			    didZoom = true;
			    if (m_ZoomLevel > m_MaxZoomIn)
				    m_ZoomLevel = m_MaxZoomIn;
		    }
		    else if (UnityEngine.Input.GetAxis("Mouse ScrollWheel") < 0 && m_ZoomLevel > m_MaxZoomOut)
		    {
			    var PrevZoomLevel = m_ZoomLevel;
			    
			    m_ZoomLevel -= m_ZoomLevel / 10;
			    
			    RectTransformUtility.ScreenPointToLocalPointInRectangle(
				    T_Map, 
				    mousePos, 
				    null, 
				    out var mousePosPreZoom
			    );
			    
			    preCord = mousePosPreZoom / m_MarkerTranspose;
			    
			    didZoom = true;
			    if (m_ZoomLevel < m_MaxZoomOut)
				    m_ZoomLevel = m_MaxZoomOut;
		    }
		    
			m_MarkerTranspose = m_ZoomLevel / ((float)GameClient.World.WIDTH_IN_CHUNKS * 32.0f);
			T_Map.sizeDelta = new Vector2(m_ZoomLevel, m_ZoomLevel);
			
			if (didZoom)
			{
				RectTransformUtility.ScreenPointToLocalPointInRectangle(
					T_Map, 
					UnityEngine.Input.mousePosition, 
					null, 
					out var mousePosPostZoom
				);

				var postCord = mousePosPostZoom / m_MarkerTranspose;
				
				var diff = postCord - preCord;
				var correctedDiff = Quaternion.Euler(0, 0, 45) * (Vector2) diff;
			    
				m_Translation += (Vector2)correctedDiff * m_MarkerTranspose;
			}
			
			IMG_TileMap.transform.localScale =  Vector3.one * m_MarkerTranspose;
	    }

	    private void UpdateMapTranslation()
	    {
		    // Follow Player
		    // var center = new float2(1, 1) * (ZoomLevel / 2f);
		    // var diff = (GameClient.LocalPlayerEntity.Position * markerTranspose) - center;
		    // var correctedDiff = Quaternion.Euler(0, 0, 45) * (Vector2) diff;
		    // T_Map.transform.localPosition = -correctedDiff;

		    T_Map.localPosition = m_InitPosition + m_Translation;
	    }

	    private void UpdateMarkers()
	    {
		    var siteMap = GameClient.World.WorldMap.GetRevealedSiteMap();

		    foreach (var markerKey in m_SiteMarkers.Keys)
		    {
			    var x = markerKey % GameClient.World.WIDTH;
			    var y = markerKey / GameClient.World.WIDTH;
			    
			    m_SiteMarkers[markerKey].transform.localPosition = (Vector2) new float2(x, y) * m_MarkerTranspose;
		    }
		    
		    foreach (var markerKey in m_BaseMarkers.Keys)
		    {
			    var b = GameClient.RoomManager.GetBase(markerKey);
			    
			    m_BaseMarkers[markerKey].transform.localPosition = (Vector2) new float2(b.Position.x, b.Position.y) * m_MarkerTranspose;
		    }
		    
		    foreach (var markerKey in m_QuestMarkers.Keys)
		    {
			    m_QuestMarkers[markerKey].transform.localPosition = (Vector2) new float2(markerKey.x * 32f + 15, markerKey.y * 32f + 15) * m_MarkerTranspose;
		    }
	    }
	    
	    public void RemovePlayerFromMap(FNEEntity entity)
	    {
		    Destroy(m_RemotePlayerMarkers[entity.NetId]);
		    m_RemotePlayerMarkers.Remove(entity.NetId);
	    }

	    private void TestMarkers()
	    {
		    foreach (Transform child in T_BasesParent)
		    {
			    Destroy(child.gameObject);
		    }
		    
		    m_BaseMarkers.Clear();
		    
		    foreach (Transform child in T_SitesParent)
		    {
			    Destroy(child.gameObject);
		    }
		    
		    m_SiteMarkers.Clear();
		    
		    foreach (Transform t in T_QuestParent)
		    {
			    Destroy(t.gameObject);
		    }
		    
		    m_QuestMarkers.Clear();
		    
		    var siteMap = GameClient.World.WorldMap.GetRevealedSiteMap();
		    foreach (var site in siteMap.Values)
		    {
			    GameObject newMarker = site.FullReveal ? Instantiate(P_SiteMarkerFull) : Instantiate(P_SiteMarker);
			    newMarker.transform.SetParent(T_SitesParent, false);
			    newMarker.GetComponent<Image>().color = new Color(1f, 1f, 1f);
			    
			    bool isSite = false;
			    if (site.FullReveal)
			    {
				    var siteData = DataBank.Instance.GetData<SiteData>(site.SiteId);
				    isSite = siteData.width >= 32 || siteData.height >= 32;
				    
				    EventTrigger trigger = newMarker.GetComponentInChildren<EventTrigger>();
				    EventTrigger.Entry entry = new EventTrigger.Entry();
				    entry.eventID = EventTriggerType.PointerEnter;
				    entry.callback.AddListener((data) =>
				    {
					    //UIManager.HoverBoxGen.CreateSimpleTextHoverBox("Testhover " + site.SiteId);
						UIManager.HoverBoxGen.CreateSiteHoverBox(siteData);
				    });
				    trigger.triggers.Add(entry);
			
				    entry = new EventTrigger.Entry();
				    entry.eventID = EventTriggerType.PointerExit;
				    entry.callback.AddListener((data) =>
				    {
					    HB_Factory.DestroyHoverbox();
				    });			    
				    trigger.triggers.Add(entry);
			    }
			    else
			    {
				    EventTrigger trigger = newMarker.GetComponentInChildren<EventTrigger>();
				    EventTrigger.Entry entry = new EventTrigger.Entry();
				    entry.eventID = EventTriggerType.PointerEnter;
				    entry.callback.AddListener((data) =>
				    {
					    UIManager.HoverBoxGen.CreateSimpleTextHoverBox("string_unknown");
				    });
				    trigger.triggers.Add(entry);
			
				    entry = new EventTrigger.Entry();
				    entry.eventID = EventTriggerType.PointerExit;
				    entry.callback.AddListener((data) =>
				    {
					    HB_Factory.DestroyHoverbox();
				    });			    
				    trigger.triggers.Add(entry);
			    }
			
			    m_SiteMarkers.Add((int) site.PosX + (int) site.PosY * GameClient.World.WIDTH, newMarker);
			    newMarker.name = (int) site.PosX + " : " + (int) site.PosY + " : " + site.SiteId;
			    newMarker.GetComponent<Image>().sprite = site.FullReveal ? isSite ? m_SiteMarker : m_LandmarkMarker : m_UnknownMarker;
		    }

		    foreach (var baseId in GameClient.RoomManager.GetBases())
		    {
			    var b = GameClient.RoomManager.GetBase(baseId);
			    GameObject newMarker = Instantiate(P_BaseMarker);
			    newMarker.transform.SetParent(T_BasesParent, false);
			    newMarker.GetComponent<Image>().color = new Color(0.3f, 0.3f, 1f);
			    newMarker.GetComponent<Image>().sprite = m_BaseMarker;
			    
			    EventTrigger trigger = newMarker.GetComponentInChildren<EventTrigger>();
			    EventTrigger.Entry entry = new EventTrigger.Entry();
			    entry.eventID = EventTriggerType.PointerEnter;
			    entry.callback.AddListener((data) =>
			    {
				    //UIManager.HoverBoxGen.CreateSimpleTextHoverBox("Testhover " + site.SiteId);
				    UIManager.HoverBoxGen.CreateMapBaseHoverBox(b);
			    });
			    trigger.triggers.Add(entry);
		
			    entry = new EventTrigger.Entry();
			    entry.eventID = EventTriggerType.PointerExit;
			    entry.callback.AddListener((data) =>
			    {
				    HB_Factory.DestroyHoverbox();
			    });			    
			    trigger.triggers.Add(entry);
			    
			    m_BaseMarkers.Add(baseId, newMarker);
		    }
		    
		    for (int i = 0; i < m_MapHighlights.Length; i++)
		    {
			    GameObject newMarker = Instantiate(P_SiteMarkerFull);
			    newMarker.transform.SetParent(T_QuestParent, false);
			    newMarker.GetComponent<Image>().color = new Color(1f, 0, 0);
			    newMarker.GetComponent<Image>().sprite = m_BaseMarker;
			    
			    m_QuestMarkers.Add(m_MapHighlights[i], newMarker);
		    }
	    }
	    
	    private void SetObjective(string id, int progress, float2[] mapHighlights)
	    {
		    TestMarkers();
	    }

	    #region EventListeners

	    private void OnBeginMouseDrag(PointerEventData data)
	    {
		    RectTransformUtility.ScreenPointToLocalPointInRectangle(
			    T_Map, 
			    UnityEngine.Input.mousePosition, 
			    null, 
			    out m_PrevDragPos
		    );
	    }
	    
	    private void OnMouseDrag(PointerEventData data)
	    {
		    RectTransformUtility.ScreenPointToLocalPointInRectangle(
			    T_Map, 
			    UnityEngine.Input.mousePosition, 
			    null, 
			    out var currentDragPos
		    );

		    var diff = currentDragPos - m_PrevDragPos;
		    
		    var correctedDiff = Quaternion.Euler(0, 0, 45) * (Vector2) diff;
		    //Debug.LogWarning("DIFF X: " + diff.x + " CORR X: " + correctedDiff.x);
		    
		    m_Translation += (Vector2)correctedDiff;
		    m_PrevDragPos = currentDragPos - diff;
	    }

	    private void OnSiteMapUpdate(Dictionary<int, MapManager.RevealedSiteData> siteMap)
	    {
		    TestMarkers();
	    }

	    #endregion

	    private void OnDestroy()
	    {
		    ClientWorldNetworkManager.d_OnSiteMapUpdate -= OnSiteMapUpdate;
		    ClientQuestNetworkManager.d_OnQuestUpdateReceived -= SetObjective;
	    }
	}
}