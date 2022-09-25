using FNZ.Client.Model.Entity.Components.Door;
using FNZ.Client.View.UI.HoverBox;
using FNZ.Client.View.UI.Manager;
using FNZ.Shared.Utils;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static FNZ.Shared.Model.World.GameWorld;

namespace FNZ.Client.View.UI.BaseViewerUI
{
	public class BaseViewer : MonoBehaviour
	{
        private List<long> m_Bases = new List<long>();
        private int activebaseIndex = 0;

        [SerializeField]
        private GameObject P_Base;

        [SerializeField]
        private GameObject P_Room;

        private GameObject m_BaseView;

        private Color[] roomColors = new Color[]
        {
            new Color(0.5f, 0.5f, 0.8f, 1),
            new Color(0.8f, 0.5f, 0.5f, 1),
            new Color(0.5f, 0.8f, 0.5f, 1),
            new Color(0.5f, 0.8f, 0.8f, 1),
            new Color(0.8f, 0.5f, 0.8f, 1),
            new Color(0.8f, 0.8f, 0.5f, 1),
            new Color(0.5f, 0.5f, 0.5f, 1),
            new Color(0.4f, 0.4f, 0.8f, 1),
            new Color(0.8f, 0.4f, 0.4f, 1),
            new Color(0.4f, 0.8f, 0.4f, 1),
            new Color(0.4f, 0.8f, 0.8f, 1),
            new Color(0.8f, 0.4f, 0.8f, 1),
            new Color(0.8f, 0.8f, 0.4f, 1),
        };


        public void Init()
        {
            m_Bases = GameClient.RoomManager.GetBases();
            activebaseIndex = 0;

			if (m_Bases.Count > 0)
                RenderActiveBase();
        }

        private void RenderActiveBase()
        {
            var rooms = GameClient.RoomManager.GetBaseRooms(m_Bases[activebaseIndex]);

            var maxX = int.MinValue;
            var minX = int.MaxValue;
            var maxY = int.MinValue;
            var minY = int.MaxValue;

            foreach(var room in rooms)
            {
                foreach (var tile in room.Tiles)
                {
                    maxX = maxX < tile.x ? tile.x : maxX;
                    minX = minX > tile.x ? tile.x : minX;
                    maxY = maxY < tile.y ? tile.y : maxY;
                    minY = minY > tile.y ? tile.y : minY;
                }
            }

            m_BaseView = Instantiate(P_Base);
            m_BaseView.transform.SetParent(transform.GetChild(0));
            m_BaseView.transform.localPosition = Vector2.zero;

            Texture2D backgroundTex = new Texture2D((maxX - minX) + 1 + 6, (maxY - minY) + 1 + 6);
            backgroundTex.filterMode = FilterMode.Point;

            for (int y = minY - 3; y <= maxY + 3; y++)
            {
                for (int x = minX - 3; x <= maxX + 3; x++)
                {
                    var tileData = GameClient.World.GetTileData(x, y);
                    var color = FNEUtil.ConvertHexStringToColor(tileData.mapColor, 0.25f);
                    backgroundTex.SetPixel(x - (minX - 3), y - (minY - 3), color);
                    //backgroundTex.SetPixel(x - (minX - 3), y - (minY - 3), Color.red);
                }
            }

            backgroundTex.Apply();

            var baseCenter = new float2(minX + (maxX - minX) / 2f, minY + (maxY - minY) / 2f);

            var image = m_BaseView.GetComponent<RawImage>();
            image.texture = backgroundTex;
            image.SetNativeSize();
            image.transform.localScale = Vector3.one * 50;

            int colorCounter = 0;
            foreach (var room in rooms)
            {
                Texture2D roomTex = new Texture2D(((room.width + 2) * 5), ((room.height + 2) * 5));
                roomTex.filterMode = FilterMode.Point;

                var fillColorArray = roomTex.GetPixels();
                for (var i = 0; i < fillColorArray.Length; ++i)
                {
                    fillColorArray[i] = new Color(0, 0, 0, 0.01f);
                }
                roomTex.SetPixels(fillColorArray);
                
                foreach (var tile in room.Tiles)
                {
                    var westNeighbor = GameClient.World.GetEdgeObject(tile.x, tile.y, EdgeObjectDirection.WEST);
                    var southNeighbor = GameClient.World.GetEdgeObject(tile.x, tile.y, EdgeObjectDirection.SOUTH);
                    var eastNeighbor = GameClient.World.GetEdgeObject(tile.x + 1, tile.y, EdgeObjectDirection.WEST);
                    var northNeighbor = GameClient.World.GetEdgeObject(tile.x, tile.y + 1, EdgeObjectDirection.SOUTH);

                    for (int i = 0; i < 5; i++)
                    {
                        for(int j = 0; j < 5; j++)
                        {
                            var Color = roomColors[colorCounter % roomColors.Length];

                            if (i == 0 && westNeighbor != null)
                            {
                                if(westNeighbor.GetComponent<DoorComponentClient>() != null && j > 0 && j < 4)
                                {
                                    Color = new Color(Color.r * 0.75f, Color.g * 0.75f, Color.b * 0.75f, 1);
                                }
                                else if (!westNeighbor.Data.roomPropertyRefs.Contains("room_property_indoors") && j % 2 == 1)
                                {
                                    Color = new Color(Color.r * 0.55f, Color.g * 0.55f, Color.b * 0.55f, 1);
                                }
                                else
                                {
                                    Color = new Color(Color.r * 0.4f, Color.g * 0.4f, Color.b * 0.4f, 1);
                                }
                            }
                            else if (j == 0 && southNeighbor != null)
                            {
                                if (southNeighbor.GetComponent<DoorComponentClient>() != null && i > 0 && i < 4)
                                {
                                    Color = new Color(Color.r * 0.75f, Color.g * 0.75f, Color.b * 0.75f, 1);
                                }
                                else if (!southNeighbor.Data.roomPropertyRefs.Contains("room_property_indoors") && i % 2 == 1)
                                {
                                    Color = new Color(Color.r * 0.55f, Color.g * 0.55f, Color.b * 0.55f, 1);
                                }
                                else
                                {
                                    Color = new Color(Color.r * 0.4f, Color.g * 0.4f, Color.b * 0.4f, 1);
                                }
                            }
                            else if (i == 4 && eastNeighbor != null)
                            {
                                if (eastNeighbor.GetComponent<DoorComponentClient>() != null && j > 0 && j < 4)
                                {
                                    Color = new Color(Color.r * 0.75f, Color.g * 0.75f, Color.b * 0.75f, 1);
                                }
                                else if (!eastNeighbor.Data.roomPropertyRefs.Contains("room_property_indoors") && j % 2 == 1)
                                {
                                    Color = new Color(Color.r * 0.55f, Color.g * 0.55f, Color.b * 0.55f, 1);
                                }
                                else
                                {
                                    Color = new Color(Color.r * 0.4f, Color.g * 0.4f, Color.b * 0.4f, 1);
                                }
                            }
                            else if (j == 4 && northNeighbor != null)
                            {
                                if (northNeighbor.GetComponent<DoorComponentClient>() != null && i > 0 && i < 4)
                                {
                                    Color = new Color(Color.r * 0.75f, Color.g * 0.75f, Color.b * 0.75f, 1);
                                }
                                else if (!northNeighbor.Data.roomPropertyRefs.Contains("room_property_indoors") && i % 2 == 1)
                                {
                                    Color = new Color(Color.r * 0.55f, Color.g * 0.55f, Color.b * 0.55f, 1);
                                }
                                else
                                {
                                    Color = new Color(Color.r * 0.4f, Color.g * 0.4f, Color.b * 0.4f, 1);
                                }
                            }

                            roomTex.SetPixel(5 + ((tile.x - room.minX) * 5) + i, 5 + ((tile.y - room.minY) * 5) + j, Color);
                        }
                    }
                }

                colorCounter++;

                roomTex.Apply();

                var roomView = Instantiate(P_Room);
                var roomImageimage = roomView.GetComponent<Image>();
                var sprite = Sprite.Create(
                    roomTex, 
                    new Rect(0.0f, 0.0f, roomTex.width, roomTex.height), 
                    new Vector2(0.5f, 0.5f),
                    1,
                    0,
                    SpriteMeshType.Tight
                );
                
                roomImageimage.sprite = sprite;
                roomImageimage.color = Color.gray;
                ((RectTransform)roomImageimage.transform).sizeDelta = new Vector2(room.width + 2, room.height + 2);
                roomImageimage.alphaHitTestMinimumThreshold = 0.5f;
                roomView.transform.SetParent(m_BaseView.transform);
                roomImageimage.transform.localScale = Vector3.one * 1;
                ((RectTransform) roomView.transform).anchoredPosition = new Vector2(
                    room.minX - (minX - 3) - 1,
                    room.minY - (minY - 3) - 1
                );

                AddRoomEvents(roomImageimage, room.Id);
            }
        }

        public void AddRoomEvents(Image image, long roomId)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((eventData) =>
            {
                UIManager.HoverBoxGen.CreateRoomHoverBox(roomId);
                image.color = Color.white;
            });
            image.GetComponent<EventTrigger>().triggers.Add(entry);

            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerExit;
            entry.callback.AddListener((eventData) =>
            {
                HB_Factory.DestroyHoverbox();
                image.color = Color.gray;
            });
            image.GetComponent<EventTrigger>().triggers.Add(entry);
        }
    }
}