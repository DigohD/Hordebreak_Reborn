using FNZ.Client.Model.Entity.Components.Door;
using FNZ.Client.Model.Entity.Components.Player;
using FNZ.Client.Model.Entity.Components.Polygon;
using FNZ.Client.Model.Entity.Components.RoomRequirements;
using FNZ.Client.View.Camera;
using FNZ.Client.View.Manager;
using FNZ.Client.View.UI.HoverBox;
using FNZ.Client.View.UI.Manager;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components;
using FNZ.Shared.Model.Entity.EntityViewData;
using FNZ.Shared.Model.World;
using FNZ.Shared.Utils;
using FNZ.Shared.Utils.CollisionUtils;
using System.Collections.Generic;
using FNZ.Client.Model.Entity.Components;
using FNZ.Client.Model.Entity.Components.InteractionEvent;
using FNZ.Shared.Model.Items;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Client.View.Player.Systems
{
	public class PlayerInteractionSystem
	{
		private PlayerController m_PlayerController;
		private FNEEntity m_Player;

		private List<int2> m_Tiles = new List<int2>();
		private List<FNEEntity> m_Walls = new List<FNEEntity>();

		private readonly float PLAYER_INTERACTION_RANGE = 1.0f;

		private long nearbyItemOnGroundId = -1;
		private FNEEntity interactableEntity = null;
		private FNEEntity prevHoverBoxOwner = null;

		private GameObject m_OutlineObject;

		public PlayerInteractionSystem(
			PlayerController playerController,
			GameObject P_HDRPOutline
		)
		{
			m_PlayerController = playerController;
			m_Player = playerController.GetPlayerEntity();

			m_OutlineObject = GameObject.Instantiate(P_HDRPOutline);
			m_OutlineObject.SetActive(false);
		}

		public void Update()
		{
			HandleInteractableCollision();
		}

		private void HandleInteractableCollision()
		{
			float2 playerPos = m_Player.Position;

			foreach(var player in GameClient.RemotePlayerEntities)
			{
				if(math.distance(playerPos, player.Position) <= 1)
				{
					var playerComp = player.GetComponent<PlayerComponentClient>();
					if (playerComp.IsInteractable())
					{
						UIManager.Instance.ShowInteractionPrompt(Localization.GetString(playerComp.InteractionPromptMessageRef()));
						interactableEntity = player;
						return;
					}
				}
			}

			nearbyItemOnGroundId = -1;
			if (!CameraScript.DidRaycastHitUI())
			{
				// Items on ground
				var itemsOnGroundDict = GameClient.ItemsOnGroundManager.GetItemsOnGround();
				foreach (var itemOnGroundEntry in itemsOnGroundDict)
				{
					var itemOnGround = itemOnGroundEntry.Value;
					if (math.distance(playerPos, itemOnGround.Position) < 1)
					{
						UIManager.HoverBoxGen.CreateSimpleItemHoverBox(
							itemOnGround.item, 
							new Vector3(itemOnGround.Position.x, 0, itemOnGround.Position.y)
						);
						UIManager.Instance.ShowInteractionPrompt(Localization.GetString("string_pick_up"));
						nearbyItemOnGroundId = itemOnGroundEntry.Key;
						interactableEntity = null;
						DisableHighlight();
						return;
					}
					else if (Object.ReferenceEquals(HB_Factory.sourceObject, itemOnGround.item))
					{
						UIManager.Instance.HideInteractionPrompt();
						HB_Factory.DestroyHoverbox();
					}
				}
			}

			List<FNEEntity> allHits = new List<FNEEntity>();
			List<int2> surrTiles = GameClient.World.GetSurroundingTilesInRadius(new int2((int)playerPos.x, (int)playerPos.y), 1);

			foreach (var tile in surrTiles)
			{
				var edgeObjects = GameClient.World.GetStraightDirectionsEdgeObjects(tile);
				foreach (var edgeObj in edgeObjects)
				{
					if (edgeObj == null)
						continue;

					var edgeObjectInteractables = edgeObj.GetInteractableComponents();
					if (edgeObjectInteractables != null)
					{
						if (CanInteract(edgeObj, playerPos))
						{
							foreach (var comp in edgeObjectInteractables)
							{
								float dist = math.distance(edgeObj.Position, playerPos);

								if (dist <= PLAYER_INTERACTION_RANGE)
								{
									if (!allHits.Contains(edgeObj))
										allHits.Add(edgeObj);
								}
							}
						}
					}
				}

				var tileEntity = GameClient.World.GetTileObject(tile.x, tile.y);
				var tileEntityInteractables = tileEntity?.GetInteractableComponents();

				if (tileEntity != null && tileEntityInteractables != null && tileEntityInteractables.Count > 0 && tileEntity.GetComponent<PolygonComponentClient>() != null)
				{
					var tileEntityRoomReqs = tileEntity?.GetComponent<RoomRequirementsComponentClient>();
					var meetsReqs = true;

					if (tileEntityRoomReqs != null)
					{
						var room = GameClient.RoomManager.GetRoom((int2)tileEntity.Position);
						if (room == null)
							meetsReqs = false;

						if (room != null && !GameClient.RoomManager.IsBaseOnline(room.ParentBaseId))
							meetsReqs = false;

						if (room != null && room.DoesRoomFulfillRequirements(tileEntityRoomReqs.Data.propertyRequirements))
						{
							meetsReqs = true;
						}
					}
					
					if (meetsReqs)
					{
						FNECollisionUtils.PolygonCircleHitResult hitRes = FNECollisionUtils.PolygonCircleCollision(
							tileEntity.GetComponent<PolygonComponentClient>().GetWorldPolygon(),
							playerPos,
							PLAYER_INTERACTION_RANGE,
							Vector2.zero
						);

						if (hitRes.hit && CanInteract(tileEntity, playerPos))
						{
							allHits.Add(tileEntity);
						}
					}
				}
				else if (tileEntity != null && tileEntityInteractables != null && tileEntityInteractables.Count > 0)
				{
					float dist = math.length(playerPos - ((float2)tile + new float2(0.5f, 0.5f)));

					if (dist <= PLAYER_INTERACTION_RANGE)
					{
						if (!allHits.Contains(tileEntity) && CanInteract(tileEntity, playerPos))
							allHits.Add(tileEntity);
					}
				}
			}

			if (!UIManager.Instance.HasPopup() || !allHits.Contains(interactableEntity))
			{
				FNEEntity objToUse = null;
				float shortestDistance = 10000;

				Ray ray = UnityEngine.Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
				int layerMask = 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("Body");
				Physics.Raycast(ray, out RaycastHit hit, 10000f, layerMask);
				var mousePointer = new float2(hit.point.x, hit.point.z);

				foreach (FNEEntity hitObject in allHits)
				{
					float distance = math.length(mousePointer - (hitObject.Position + new float2(0.5f, 0.5f)));

					if (distance < shortestDistance)
					{
						shortestDistance = distance;
						objToUse = hitObject;
					}
				}

				if (objToUse != null)
				{
					var interactables = objToUse.GetInteractableComponents();
					if (interactables != null && interactables.Count > 0)
					{
						interactableEntity = objToUse;
						var interactable = (IInteractableComponent)interactables[0];
						UIManager.Instance.ShowInteractionPrompt(Localization.GetString(interactable.InteractionPromptMessageRef()));
						HighLightTarget(interactables[0].ParentEntity);

						foreach (var i in interactables)
						{
							if (i is InteractionEventComponentClient)
							{
								var eventComp = (InteractionEventComponentClient) i;
								if (!string.IsNullOrEmpty(eventComp.Data.eventRef))
								{
									UIManager.HoverBoxGen.CreateEventHoverBox(eventComp.Data.eventRef,
										new Vector3(interactableEntity.Position.x + 0.5f, 0, interactableEntity.Position.y + 0.5f)
										);
								}
							}
						}
					}
				}
				else if (interactableEntity != null && objToUse == null)
				{
					foreach (var interactable in interactableEntity.GetInteractableComponents())
					{
						((IInteractableComponent)interactable).OnPlayerExitRange();
					}

					interactableEntity = null;
					UIManager.Instance.HideInteractionPrompt();

					DisableHighlight();
					HB_Factory.DestroyHoverbox();
				}

				if (interactableEntity != null)
				{
					if (prevHoverBoxOwner != null && prevHoverBoxOwner != interactableEntity)
					{
						foreach (var interactable in prevHoverBoxOwner.GetInteractableComponents())
						{
							((IInteractableComponent)interactable).OnPlayerExitRange();
						}
					}
				}else if (nearbyItemOnGroundId < 0)
				{
					UIManager.Instance.HideInteractionPrompt();
				}

				prevHoverBoxOwner = interactableEntity;
			}
		}

		public void OnInteract()
		{
			if (interactableEntity != null)
			{
				foreach (var interactable in interactableEntity.GetInteractableComponents())
				{
					((IInteractableComponent)interactable).OnInteract();
				}
			}
			else if(nearbyItemOnGroundId > 0){
				m_Player.GetComponent<PlayerComponentClient>().NE_Send_RequestPickUpItem(nearbyItemOnGroundId);
			}
		}

		private bool CanInteract(FNEEntity objToUse, float2 playerPos)
		{
			int2 objToUsePos = (int2)objToUse.Position;

			m_Tiles.Clear();

			m_Tiles.Add(new int2(objToUsePos.x + 1, objToUsePos.y));
			m_Tiles.Add(new int2(objToUsePos.x - 1, objToUsePos.y));
			m_Tiles.Add(new int2(objToUsePos.x, objToUsePos.y + 1));
			m_Tiles.Add(new int2(objToUsePos.x, objToUsePos.y - 1));

			m_Walls.Clear();

			foreach (var tile in m_Tiles)
			{
				foreach (var edgeObj in GameClient.World.GetStraightDirectionsEdgeObjects(tile))
				{
					if (edgeObj != null)
					{
						if (edgeObj.HasComponent<DoorComponentClient>())
						{
							continue;
						}

						if (edgeObj.HasComponent<PolygonComponentClient>())
						{
							m_Walls.Add(edgeObj);
						}
					}
				}
			}

			float2 objectOffset = objToUse.EntityType == EntityType.EDGE_OBJECT 
				? new float2(0, 0) 
				: new float2(0.5f, 0.5f);

			FNERayCastHitStruct hitStruct = FNECollisionUtils.RayCastOnPolygons(
				playerPos,
				objToUse.Position + objectOffset,
				m_Walls
			);

			return !hitStruct.IsHit;
		}

		private void HighLightTarget(FNEEntity interactable)
		{
			string viewRef = "";
			var cropComp = interactable.GetComponent<CropComponentClient>();
			if (cropComp != null)
			{
				viewRef = cropComp.Data.harvestableViewRef;
			}
			else
			{
				viewRef = FNEEntity.GetEntityViewVariationId(interactable.Data, interactable.Position);
			}
			
			var viewData = DataBank.Instance.GetData<FNEEntityViewData>(viewRef);
			var template = GameObjectPoolManager.GetObjectInstance(
				viewRef,
				PrefabType.FNEENTIY,
				interactable.Position
			);
			m_OutlineObject.transform.localScale = Vector3.one * viewData.scaleMod;

			if (template != null)
			{
				Quaternion rotation = Quaternion.AngleAxis(interactable.RotationDegrees, Vector3.down);
				var posVector = new Vector3(interactable.Position.x, viewData.heightPos, interactable.Position.y);
				if (interactable.Data.entityType == EntityType.TILE_OBJECT)
				{
					m_OutlineObject.transform.position = posVector + new Vector3(0.5f, 0.0f, 0.5f);
					m_OutlineObject.transform.rotation = rotation;
				}
				else
				{
					m_OutlineObject.transform.position = posVector;
					m_OutlineObject.transform.rotation = rotation;
				}

				m_OutlineObject.SetActive(true);
				var meshFilter = template.GetComponent<MeshFilter>();
				if (meshFilter != null)
				{
					m_OutlineObject.GetComponent<MeshFilter>().mesh = meshFilter.mesh;
				}
				else
				{
					var meshRenderer = template.GetComponentInChildren<SkinnedMeshRenderer>();
					if (meshRenderer != null)
					{
						m_OutlineObject.GetComponentInChildren<MeshFilter>().mesh = meshRenderer.sharedMesh;
					} else
                    {
						var staticMeshRenderer = template.GetComponentInChildren<MeshFilter>();
						m_OutlineObject.GetComponentInChildren<MeshFilter>().mesh = staticMeshRenderer.sharedMesh;
					}
				}
				
				GameObjectPoolManager.DoRecycle(viewRef, template);
			}
		}

		private void DisableHighlight()
		{
			m_OutlineObject.SetActive(false);
		}
	}

}
