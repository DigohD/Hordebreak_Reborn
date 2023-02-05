using FNZ.Client.Model.World;
using FNZ.Client.Net.NetworkManager;
using FNZ.Client.View.UI.HoverBox;
using FNZ.Client.View.UI.Manager;
using FNZ.Client.View.UI.Sprites;
using FNZ.Shared.Model;
using FNZ.Shared.Model.World.MetaWorld;
using FNZ.Shared.Model.World.Site;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.MetaWorld
{

	public class MetaWorldUI : MonoBehaviour
	{
		[SerializeField]
		private RectTransform T_Map;

		[SerializeField]
		private Transform T_PlacesParent;

		[SerializeField]
		private RectTransform T_MapBG;

		//[SerializeField] private RawImage IMG_TileMap;
		//[SerializeField] private RawImage IMG_TileMapChunk;
		//[SerializeField] private RawImage IMG_MaskedMap;

		[SerializeField] private GameObject P_PlaceMarker;
		[SerializeField] private GameObject P_HomeMarker;

		private GameObject m_HomeMarker;
		private Dictionary<string, Tuple<Place, GameObject>> m_PlaceMarkers = new Dictionary<string, Tuple<Place, GameObject>>();

		private int m_MaxZoomIn = 1000000;
		private int m_MaxZoomOut = 100000;
		private int m_ZoomLevel = 80000;

		private Vector2 m_InitPosition, m_Translation;
		private Vector2 m_PrevDragPos;

		private float m_MarkerTranspose = 80000f / 8192f;

		private Sprite m_PlaceMarkerSprite;
		private Sprite m_HomeMarkerSprite;

		private Vector2 MapCenter;

		private Rect m_AllowedBoundingBox;

		void Start()
		{
			MapCenter = T_PlacesParent.transform.localPosition;

			m_PlaceMarkerSprite = SpriteBank.GetSprite("map_icon_landmark");
			m_HomeMarkerSprite = SpriteBank.GetSprite("map_icon_base_flag");

			//for (var y = 0; y < 8; y++)
			//{
			//	for (var x = 0; x < 8; x++)
			//	{
			//		var go = Instantiate(IMG_TileMapChunk.gameObject);
			//		go.transform.SetParent(IMG_TileMap.transform, false);
			//		go.transform.localPosition = new Vector3(x * 1024, y * 1024, 0);
			//		//go.GetComponentInChildren<RawImage>().texture = ClientMapManager.GetMapPartTexture(x, y);
			//		//TileMapParts[x, y] = go.GetComponentInChildren<RawImage>();
			//	}
			//}

			//IMG_TileMap.transform.localScale = Vector3.one * m_MarkerTranspose;

			var center = new float2(1, 1) * (m_ZoomLevel / 2f);
			var diff = (float2.zero * m_MarkerTranspose) - center;
			var correctedDiff = (Vector2) diff;
			T_Map.transform.localPosition = -correctedDiff;
			m_InitPosition = T_PlacesParent.transform.localPosition;

			T_MapBG.sizeDelta = new Vector2(m_ZoomLevel, m_ZoomLevel);

			InitEvents();
			BuildMarkers();
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

            ClientMetaWorldNetworkManager.d_OnMetaWorldUpdate += OnMetaWorldUpdate;
        }



		void Update()
		{
			UpdateZoom();
			UpdateMarkers();
			UpdateMapTranslation();
		}

		private void LateUpdate()
		{

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

			m_MarkerTranspose = m_ZoomLevel / ((float)GameClient.World.WIDTH);
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
				var correctedDiff = (Vector2)diff;

				m_Translation += (Vector2)correctedDiff * m_MarkerTranspose;

				SnapTranslationToBounidngBox();
			}

			//IMG_TileMap.transform.localScale = Vector3.one * m_MarkerTranspose;
		}

		private void UpdateMapTranslation()
		{
			// Follow Player
			// var center = new float2(1, 1) * (ZoomLevel / 2f);
			// var diff = (GameClient.LocalPlayerEntity.Position * markerTranspose) - center;
			// var correctedDiff = (Vector2) diff;
			// T_Map.transform.localPosition = -correctedDiff;

			T_Map.localPosition = m_InitPosition + m_Translation;
		}

		private void UpdateMarkers()
		{
			var places = GameClient.MetaWorld.Places;

			m_AllowedBoundingBox = new Rect();

			foreach (var place in places)
			{
				var newX = place.Coords.x * 10;
				var newY = place.Coords.y * 10;

				m_PlaceMarkers[place.Id.ToString()].Item2.transform.localPosition = (Vector2)new float2(newX, newY) * m_MarkerTranspose;

				if (newX > m_AllowedBoundingBox.x + m_AllowedBoundingBox.width)
				{
					m_AllowedBoundingBox.width = newX - m_AllowedBoundingBox.x;
				}
				if (newX < m_AllowedBoundingBox.x)
				{
					m_AllowedBoundingBox.x = newX;
				}

				if (newY > m_AllowedBoundingBox.y + m_AllowedBoundingBox.height)
				{
					m_AllowedBoundingBox.height = newY - m_AllowedBoundingBox.y;
				}
				if (newY < m_AllowedBoundingBox.y)
				{
					m_AllowedBoundingBox.y = newY;
				}
			}
		}

		// Can potentially be useful later when places markers need removal
		//
		//public void RemovePlayerFromMap(FNEEntity entity)
		//{
		//	Destroy(m_RemotePlayerMarkers[entity.NetId]);
		//	m_RemotePlayerMarkers.Remove(entity.NetId);
		//}

		private void BuildMarkers()
		{
			foreach (Transform t in T_PlacesParent)
			{
				Destroy(t.gameObject);
			}

			m_PlaceMarkers.Clear();

			m_HomeMarker = Instantiate(P_HomeMarker);
			m_HomeMarker.name = "HOME";
			m_HomeMarker.transform.SetParent(T_PlacesParent, false);
			m_HomeMarker.transform.localPosition = Vector2.zero * m_MarkerTranspose;
			m_HomeMarker.GetComponent<Image>().sprite = m_HomeMarkerSprite;

			var places = GameClient.MetaWorld.Places;
			foreach (var place in places)
			{
				GameObject newMarker = Instantiate(P_PlaceMarker);
				newMarker.transform.SetParent(T_PlacesParent, false);
				newMarker.GetComponent<Image>().color = new Color(1f, 1f, 1f);

				//var siteData = DataBank.Instance.GetData<SiteData>(site.SiteId);
				//isSite = siteData.width >= 32 || siteData.height >= 32;

				EventTrigger trigger = newMarker.GetComponentInChildren<EventTrigger>();
				EventTrigger.Entry entry = new EventTrigger.Entry();
				entry.eventID = EventTriggerType.PointerEnter;
				entry.callback.AddListener((data) =>
				{
                    //UIManager.HoverBoxGen.CreateSimpleTextHoverBox("Testhover " + place.Name);
                    UIManager.HoverBoxGen.CreateSiteHoverBox(DataBank.Instance.GetData<SiteData>(place.SiteId));
                });
				trigger.triggers.Add(entry);

				entry = new EventTrigger.Entry();
				entry.eventID = EventTriggerType.PointerExit;
				entry.callback.AddListener((data) =>
				{
					HB_Factory.DestroyHoverbox();
				});
				trigger.triggers.Add(entry);

				m_PlaceMarkers.Add(place.Id.ToString(), new Tuple<Place, GameObject>(place, newMarker));
				newMarker.name = place.Id.ToString();
				newMarker.GetComponent<Image>().sprite = m_PlaceMarkerSprite;
			}
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

			var correctedDiff = (Vector2)diff;
			//Debug.LogWarning("DIFF X: " + diff.x + " CORR X: " + correctedDiff.x);

			m_Translation += (Vector2)correctedDiff;
			SnapTranslationToBounidngBox();

			m_PrevDragPos = currentDragPos - diff;
		}

		private void OnMetaWorldUpdate()
		{
			BuildMarkers();
		}

		#endregion

		private void OnDestroy()
		{
			ClientMetaWorldNetworkManager.d_OnMetaWorldUpdate -= OnMetaWorldUpdate;	
		}

		private void SnapTranslationToBounidngBox()
        {
			m_Translation.x = m_Translation.x > m_AllowedBoundingBox.xMax * m_MarkerTranspose ? m_AllowedBoundingBox.xMax * m_MarkerTranspose : m_Translation.x;
			m_Translation.x = m_Translation.x < m_AllowedBoundingBox.xMin * m_MarkerTranspose ? m_AllowedBoundingBox.xMin * m_MarkerTranspose : m_Translation.x;
			m_Translation.y = m_Translation.y > m_AllowedBoundingBox.yMax * m_MarkerTranspose ? m_AllowedBoundingBox.yMax * m_MarkerTranspose : m_Translation.y;
			m_Translation.y = m_Translation.y < m_AllowedBoundingBox.yMin * m_MarkerTranspose ? m_AllowedBoundingBox.yMin * m_MarkerTranspose : m_Translation.y;

			Debug.LogWarning("--------------------");
			Debug.LogWarning("xMax" + m_AllowedBoundingBox.xMax * m_MarkerTranspose);
			Debug.LogWarning("xMin" + m_AllowedBoundingBox.xMin * m_MarkerTranspose);
			Debug.LogWarning("yMax" + m_AllowedBoundingBox.yMax * m_MarkerTranspose);
			Debug.LogWarning("yMin" + m_AllowedBoundingBox.yMin * m_MarkerTranspose);
			Debug.LogWarning("--------------------");
		}
	}
}