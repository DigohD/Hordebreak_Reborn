using FNZ.Client.Model.Entity.Components.Builder;
using FNZ.Client.Model.Entity.Components.BuildingAddon;
using FNZ.Client.Model.Entity.Components.EdgeObject;
using FNZ.Client.Model.Entity.Components.Inventory;
using FNZ.Client.View.Manager;
using FNZ.Client.View.Prefab;
using FNZ.Client.View.UI.Addon;
using FNZ.Client.View.UI.HoverBox;
using FNZ.Client.View.UI.Manager;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Building;
using FNZ.Shared.Model.BuildingAddon;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.EntityViewData;
using FNZ.Shared.Model.Entity.MountedObject;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.World.Tile;
using FNZ.Shared.Utils.CollisionUtils;
using Knife.HDRPOutline.Core;
using System.Collections.Generic;
using FNZ.Client.View.UI.Building;
using FNZ.Shared.Model.Entity.Components.BaseTerminal;
using FNZ.Shared.Model.Entity.Components.RoomRequirements;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Client.View.Player.Building
{

	public class PlayerBuildView : MonoBehaviour
	{

		private enum BuildingType
		{
			NADA = -1,
			ADDON = 0,
			WALL = 1,
			TILE_OBJECT = 2,
			TILE = 3,
			MOUNTED_OBJECT = 4
		}

		public readonly byte WALL_ADDON_RANGE = 2;
		public readonly byte TO_ADDON_RANGE = 2;

		private Vector2 m_BuildingPoint = new Vector2();
		private float m_BuildingRotation = 0;
		private bool m_WallStartPointSet = false;
		private Vector2 m_WallStartPoint = Vector2.zero;
		private bool m_TileStartPointSet = false;
		private int2 m_TileStartPoint = new int2(0, 0);
		private BuildingType m_ObjectType = BuildingType.NADA;
		private BuildingData m_BuildingData;
		private string m_ProductId;

		private int m_WallDX, m_WallDY;
		private int2 m_TileBoxTarget;


		[SerializeField]
		private GameObject P_BuildingPreview;
		[HideInInspector]
		public GameObject BuildingPreview;
		
		[SerializeField]
		private GameObject P_DirectionSprite;
		private bool m_MountedObjectOppositeDirection;

		public GameObject P_AddonMenu;
		private GameObject ActiveAddonMenu;

		public Mesh Mesh_Cube;

		[HideInInspector]
		public List<string> ValidTiles;

		[HideInInspector]
		public bool IsBlocked = false;

		private List<GameObject> activeWallTargets = new List<GameObject>();
		private Stack<GameObject> pooledWallTargets = new Stack<GameObject>();
		private List<GameObject> tempWallTargets = new List<GameObject>();
		private List<FNEEntity> tempWallTargetEntitiesAdded = new List<FNEEntity>();

		private List<GameObject> m_ActiveTOTargets = new List<GameObject>();
		private Stack<GameObject> m_PooledTOTargets = new Stack<GameObject>();
		private List<GameObject> m_TempTOTargets = new List<GameObject>();

		private GameObject currentTOTargetMarker;
		private int2 previousPlayerTilePos;

		[SerializeField]
		private GameObject P_HDRPOutline;
		private GameObject m_OutlineObject;

		public void Init(FNEEntity player)
		{
			if (player == GameClient.LocalPlayerEntity)
			{
				m_OutlineObject = Instantiate(P_HDRPOutline);
				m_OutlineObject.SetActive(false);
			}
		}

		// Update is called once per frame
		void Update()
		{
			if (m_ObjectType == BuildingType.NADA)
			{
				if (BuildingPreview != null)
					Destroy(BuildingPreview);
				return;
			}

			if (BuildingPreview == null)
				AddonModeUpdate();
			else
				BuildingAddonHintsUI.Instance.ClearAddonHints();

			RaycastHit hit;
			Ray ray = UnityEngine.Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
			int layerMask = 1 << LayerMask.NameToLayer("Ground");

			if (Physics.Raycast(ray, out hit, 10000f, layerMask))
			{
				switch (m_ObjectType)
				{
					case BuildingType.TILE_OBJECT:
						TileObjectModeUpdate(hit);
						break;

					case BuildingType.WALL:
						EdgeObjectModeUpdate(hit);
						break;

					case BuildingType.TILE:
						TileModeUpdate(hit);
						break;

					case BuildingType.MOUNTED_OBJECT:
						MountedObjectUpdate();
						break;
				}
			}
		}

		private void TileObjectModeUpdate(RaycastHit hit)
		{
			TileObjectSnap(hit.point.x, hit.point.z);
			if (UnityEngine.Input.GetKeyDown("r"))
			{
				m_BuildingRotation += 90;
				if (m_BuildingRotation == 360)
					m_BuildingRotation = 0;
			}

			var tileInt2 = new int2((int) m_BuildingPoint.x, (int) m_BuildingPoint.y);
			
			var tileInsideBase = GameClient.RoomManager.IsTileWithinBase(
				tileInt2
			);

			var entity = DataBank.Instance.GetData<FNEEntityData>(m_BuildingData.productRef);
			var isNewBaseEntity = false;
			if (!tileInsideBase)
			{
				var btComp = entity.GetComponentData<BaseTerminalComponentData>();
				if (btComp != null)
				{
					isNewBaseEntity = true;
				}
			}

			var room = GameClient.RoomManager.GetRoom(tileInt2);
			var roomReqs = entity.GetComponentData<RoomRequirementsComponentData>();
			
			if((!isNewBaseEntity && !tileInsideBase) || DoesTileObjectCollide())
			{
				BuildingPreview.GetComponent<OutlineObject>().Color = Color.red;
				IsBlocked = true;
			}
			else if (room == null && roomReqs != null && roomReqs.propertyRequirements.Count > 0)
			{
				BuildingPreview.GetComponent<OutlineObject>().Color = Color.red;
				IsBlocked = true;
			}
			else
			{
				if (
					room != null && 
					roomReqs != null && 
					roomReqs.propertyRequirements.Count > 0 &&
					!room.DoesRoomFulfillRequirements(roomReqs.propertyRequirements)
				)
				{
					BuildingPreview.GetComponent<OutlineObject>().Color = Color.red;
					IsBlocked = true;
				}
				else
				{
					BuildingPreview.GetComponent<OutlineObject>().Color = Color.green;

					var inventory = GameClient.LocalPlayerEntity.GetComponent<InventoryComponentClient>();
					var materials = m_BuildingData.requiredMaterials;
					for (int i = 0; i < materials.Count; i++)
					{
						if (inventory.GetItemCount(materials[i].itemRef) < materials[i].amount)
						{
							BuildingPreview.GetComponent<OutlineObject>().Color = Color.red;
							break;
						}
					}

					IsBlocked = false;
				}
			}

			if (BuildingPreview != null)
			{
				BuildingPreview.transform.rotation = Quaternion.Euler(0, m_BuildingRotation, 0);
				BuildingPreview.transform.position = new Vector3(m_BuildingPoint.x, 0, m_BuildingPoint.y);
			}
		}

		private void EdgeObjectModeUpdate(RaycastHit hit)
		{
			// EdgeObjectSnapToEdge(hit.point.x, hit.point.y);

			var previousBuildingPoint = m_BuildingPoint;

			m_BuildingPoint = new Vector2((int)(hit.point.x + 0.5f), (int)(hit.point.z + 0.5f));

			// Only update stuff if something has changed.
			if (m_BuildingPoint == previousBuildingPoint)
				return;

			if (m_WallStartPointSet)
			{
				m_WallDX = (int)m_BuildingPoint.x - (int)m_WallStartPoint.x;
				m_WallDY = (int)m_BuildingPoint.y - (int)m_WallStartPoint.y;

				if (Mathf.Abs(m_WallDX) >= Mathf.Abs(m_WallDY))
				{
					m_WallDY = 0;
				}
				else
				{
					m_WallDX = 0;
				}

				BuildingPreview.transform.position = new Vector3(
					m_WallStartPoint.x + (m_WallDX / 2f),
					1,
					m_WallStartPoint.y + (m_WallDY / 2f)
				);

				BuildingPreview.transform.localScale = new Vector3(m_WallDX + 0.1f, 2f, m_WallDY + 0.1f);

				Color colorVar;
				int buildableWalls;
				if (CalculateBuildableWalls(out colorVar, out buildableWalls))
				{
					BuildingPreview.GetComponent<OutlineObject>().Color = colorVar;
					IsBlocked = buildableWalls == 0;
				}
				else
				{
					BuildingPreview.GetComponent<OutlineObject>().Color = colorVar;
					IsBlocked = false;
				}

				if (buildableWalls > 0)
				{
					bool hasMaterials = true;
					foreach (MaterialDef md in m_BuildingData.requiredMaterials)
					{
						hasMaterials &= GameClient.LocalPlayerEntity.GetComponent<InventoryComponentClient>().GetItemCount(md.itemRef) >= md.amount * buildableWalls;
					}

					if (!hasMaterials)
					{
						BuildingPreview.GetComponent<OutlineObject>().Color = Color.red;

						IsBlocked = true;
					}

					UIManager.HoverBoxGen.CreateWallCostHoverBox(m_BuildingData, buildableWalls, hasMaterials);
				}
				else
				{
					HB_Factory.DestroyHoverbox();
				}
			}
			else
			{
				BuildingPreview.transform.position = new Vector3(
					m_BuildingPoint.x,
					1,
					m_BuildingPoint.y
				);
			}
		}

		private void AddonModeUpdate()
		{
			var pos = GameClient.LocalPlayerEntity.Position;
			var posInt2 = new int2((int)pos.x, (int)pos.y);
			// If the player has entered a new tile, recalculate hitboxes
			if (!posInt2.Equals(previousPlayerTilePos))
			{
				previousPlayerTilePos = posInt2;
				
				UpdateWallTargets(posInt2);
				UpdateTileObjectTargets(posInt2);
			}
			else
			{
				foreach(var toTarget in m_ActiveTOTargets)
				{
					var tileObject = GameClient.World.GetTileObject((int) toTarget.transform.position.x, (int) toTarget.transform.position.z);

					if (tileObject == null || !tileObject.HasComponent<BuildingAddonComponentClient>())
					{
						m_TempTOTargets.Add(toTarget);
						m_PooledTOTargets.Push(toTarget);
						toTarget.SetActive(false);
					}
				}

				foreach (var tempTarget in m_TempTOTargets)
				{
					m_ActiveTOTargets.Remove(tempTarget);
				}

				m_TempTOTargets.Clear();
			}
			
			BuildingAddonHintsUI.Instance.ClearAddonHints();
			BuildingAddonHintsUI.Instance.UpdateEdgebjectEventTriggers(activeWallTargets);
			BuildingAddonHintsUI.Instance.UpdateTileObjectEventTriggers(m_ActiveTOTargets);
			
			RaycastHit hit;
			Ray ray = UnityEngine.Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
			int layerMask = 1 << LayerMask.NameToLayer("EdgeObjectTarget");
			// If the player hovers a nearby, active hitbox
			if (UIManager.Instance.AddonAnchor.childCount == 0 && Physics.Raycast(ray, out hit, 10000f, layerMask))
			{
				var hitCol = hit.collider;
				FNEEntity target = null;
				if(hitCol.name.Contains("WMP"))
				{
					target = GameClient.World.GetEdgeObjectAtPosition(new float2(
						hit.collider.transform.position.x,
						hit.collider.transform.position.z
					));
				}else if(hitCol.name.Contains("TOMP"))
				{
					target = GameClient.World.GetTileObject(
						(int)hit.collider.transform.position.x,
						(int)hit.collider.transform.position.z
					);
				}

				HighLightTarget(target);
			}
			else
			{
				m_OutlineObject.GetComponent<OutlineObject>().enabled = false;
			}
		}

		private void HighLightTarget(FNEEntity target)
		{
			// Remote players
			if (m_OutlineObject == null)
				return;

			m_OutlineObject.GetComponent<OutlineObject>().enabled = true;
			if (target.EntityType == EntityType.EDGE_OBJECT)
				m_OutlineObject.name = "WMP";
			else if (target.EntityType == EntityType.TILE_OBJECT)
				m_OutlineObject.name = "TOMP";
			
			var viewRef = FNEEntity.GetEntityViewVariationId(target.Data, target.Position);
			var viewData = DataBank.Instance.GetData<FNEEntityViewData>(viewRef);
			var template = GameObjectPoolManager.GetObjectInstance(
				viewRef,
				PrefabType.FNEENTIY,
				target.Position
			);
			m_OutlineObject.transform.localScale = Vector3.one * viewData.scaleMod;

			if (template != null)
			{
				Quaternion rotation = Quaternion.AngleAxis(-target.RotationDegrees, Vector3.down);
				var posVector = new Vector3(target.Position.x, 0, target.Position.y);
				if (target.Data.entityType == EntityType.TILE_OBJECT)
				{
					m_OutlineObject.transform.position = posVector + new Vector3(0.5f, viewData.heightPos, 0.5f);
					m_OutlineObject.transform.rotation = rotation;
				}
				else
				{
					m_OutlineObject.transform.position = posVector + new Vector3(0, viewData.heightPos, 0);
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
					m_OutlineObject.GetComponentInChildren<MeshFilter>().mesh = meshRenderer.sharedMesh;
				}

				GameObjectPoolManager.DoRecycle(viewRef, template);
			}
		}

		private void MountedObjectUpdate()
		{
			var pos = GameClient.LocalPlayerEntity.Position;
			var posInt2 = new int2((int)pos.x, (int)pos.y);

			UpdateWallTargets(posInt2);

			if (UnityEngine.Input.GetKeyDown("r"))
			{
				m_MountedObjectOppositeDirection = !m_MountedObjectOppositeDirection;
			}

			RaycastHit hit;
			Ray ray = UnityEngine.Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
			int layerMask = 1 << LayerMask.NameToLayer("EdgeObjectTarget");
			// If the player hovers a nearby, active hitbox
			if (Physics.Raycast(ray, out hit, 10000f, layerMask))
			{
				var targetEntity = GameClient.World.GetEdgeObjectAtPosition(new float2(
					hit.collider.transform.position.x,
					hit.collider.transform.position.z
				));

				BuildingPreview.SetActive(true);

				var targetColor = Color.green;

				var inventory = GameClient.LocalPlayerEntity.GetComponent<InventoryComponentClient>();
				var materials = m_BuildingData.requiredMaterials;
				for (int i = 0; i < materials.Count; i++)
				{
					if (inventory.GetItemCount(materials[i].itemRef) < materials[i].amount)
					{
						targetColor = Color.red;
						break;
					}
				}

				BuildingPreview.transform.position = new Vector3(targetEntity.Position.x, 0, targetEntity.Position.y);

				// Vertical wall
				if (targetEntity.Position.x % 1 == 0)
				{
					if (!m_MountedObjectOppositeDirection)
						BuildingPreview.transform.rotation = Quaternion.Euler(0, 90, 0);
					else
						BuildingPreview.transform.rotation = Quaternion.Euler(0, 270, 0);
				}
				// Horizontal wall
				else
				{
					if (!m_MountedObjectOppositeDirection)
						BuildingPreview.transform.rotation = Quaternion.Euler(0, 0, 0);
					else
						BuildingPreview.transform.rotation = Quaternion.Euler(0, 180, 0);
				}

				IsBlocked = false;
				if (!targetEntity.Data.isMountable || targetEntity.GetComponent<EdgeObjectComponentClient>().MountedObjectData != null)
				{
					targetColor = Color.red;
					IsBlocked = true;
				}


				BuildingPreview.GetComponent<OutlineObject>().Color = targetColor;
			}
			else
			{
				BuildingPreview.SetActive(false);
			}
		}

		private void UpdateWallTargets(int2 posInt2)
		{
			// Move active walls to temp walls. temp walls will then be distributed to pooled or active walls as necessary.
			foreach (var wallTarget in activeWallTargets)
			{
				tempWallTargets.Add(wallTarget);
			}
			activeWallTargets.Clear();

			var tiles = GameClient.World.GetSurroundingTilesInRadius(posInt2, WALL_ADDON_RANGE);
			foreach (var tilePos in tiles)
			{
				foreach (var edgeObject in GameClient.World.GetStraightDirectionsEdgeObjects(tilePos))
				{
					// Only handle actual edge objects, also continue if this is an edge we have already checked.
					if (edgeObject == null || tempWallTargetEntitiesAdded.Contains(edgeObject))
						continue;

					if (!edgeObject.HasComponent<BuildingAddonComponentClient>())
						continue;

					var alreadyAddedToActive = false;
					foreach (var wallTarget in activeWallTargets)
					{
						// Check if this edge object already has a corresponding, active wall target GameObject
						if (
							wallTarget.transform.position.x == edgeObject.Position.x &&
							wallTarget.transform.position.z == edgeObject.Position.y)
						{
							alreadyAddedToActive = true;
						}
					}

					if(alreadyAddedToActive)
						continue;
					
					GameObject alreadyPresentWallTarget = null;
					foreach (var wallTarget in tempWallTargets)
					{
						// Check if this edge object already has a corresponding, active wall target GameObject
						if (
							wallTarget.transform.position.x == edgeObject.Position.x &&
							wallTarget.transform.position.z == edgeObject.Position.y)
						{
							alreadyPresentWallTarget = wallTarget;
						}
					}
					
					// If not, see if there are pooled hitboxes ready for usage, otherwise instantiate a new one 
					if (alreadyPresentWallTarget == null)
					{
						GameObject newTargetMarker = null;
						if (pooledWallTargets.Count > 0)
						{
							newTargetMarker = pooledWallTargets.Pop();
							newTargetMarker.SetActive(true);
						}
						else
						{
							newTargetMarker = Instantiate(Resources.Load<GameObject>("Prefab/Entity/Player/WMP"));
						}

						newTargetMarker.transform.position = new Vector3(edgeObject.Position.x, 0, edgeObject.Position.y);
						newTargetMarker.transform.localRotation = Quaternion.Euler(90, edgeObject.RotationDegrees, 0);

						activeWallTargets.Add(newTargetMarker);
					}
					else
					{
						tempWallTargets.Remove(alreadyPresentWallTarget);
						tempWallTargetEntitiesAdded.Add(edgeObject);
						activeWallTargets.Add(alreadyPresentWallTarget);
					}
				}
			}

			// The edge objects of the active hitboxes are not in range, deactivate hitboxes and pool.
			foreach (var wallTarget in tempWallTargets)
			{
				pooledWallTargets.Push(wallTarget);
				wallTarget.SetActive(false);
			}

			// Clear temporary helper lists
			tempWallTargetEntitiesAdded.Clear();
			tempWallTargets.Clear();
			
			// The transforms of the new wall targets normally updates each FixedUpdate Frame. This makes sure that
			// the following raycast is able to detect the newly added wall targets.
			Physics.SyncTransforms();
		}

		private void UpdateTileObjectTargets(int2 posInt2)
		{
			// Move active walls to temp walls. temp walls will then be distributed to pooled or active walls as necessary.
			foreach (var toTarget in m_ActiveTOTargets)
			{
				m_TempTOTargets.Add(toTarget);
			}
			m_ActiveTOTargets.Clear();

			var tiles = GameClient.World.GetSurroundingTilesInRadius(posInt2, TO_ADDON_RANGE);
			foreach (var tilePos in tiles)
			{
				var tileObject = GameClient.World.GetTileObject(tilePos.x, tilePos.y);

				if (tileObject == null)
					continue;

				if (!tileObject.HasComponent<BuildingAddonComponentClient>())
					continue;

				GameObject alreadyPresentTOTarget = null;
				foreach (var target in m_TempTOTargets)
				{
					// Check if this edge object already has a corresponding, active wall target GameObject
					if (
						target.transform.position.x == tileObject.Position.x &&
						target.transform.position.z == tileObject.Position.y)
					{
						alreadyPresentTOTarget = target;
					}
				}

				if (alreadyPresentTOTarget == null)
				{
					GameObject newTargetMarker = null;
					if (m_PooledTOTargets.Count > 0)
					{
						newTargetMarker = m_PooledTOTargets.Pop();
						newTargetMarker.SetActive(true);
					}
					else
					{
						newTargetMarker = Instantiate(Resources.Load<GameObject>("Prefab/Entity/Player/TOMP"));
					}

					var pos = (tileObject.Position + new float2(0.5F, 0.5F));
					newTargetMarker.transform.position = new Vector3(pos.x, 0, pos.y);
					newTargetMarker.transform.localRotation = Quaternion.Euler(0, tileObject.RotationDegrees, 0);

					m_ActiveTOTargets.Add(newTargetMarker);
				}
				else
				{
					m_TempTOTargets.Remove(alreadyPresentTOTarget);
					m_ActiveTOTargets.Add(alreadyPresentTOTarget);
				}
			}

			// The tile objects of the active hitboxes are not in range, deactivate hitboxes and pool.
			foreach (var toTarget in m_TempTOTargets)
			{
				m_PooledTOTargets.Push(toTarget);
				toTarget.SetActive(false);
			}

			// Clear temporary helper lists
			m_TempTOTargets.Clear();

			// The transforms of the new wall targets normally updates each FixedUpdate Frame. This makes sure that
			// the following raycast is able to detect the newly added wall targets.
			Physics.SyncTransforms();
			
			if (currentTOTargetMarker == null)
			{
				currentTOTargetMarker = Instantiate(Resources.Load<GameObject>("Prefab/Entity/Player/TOMP_HIT"));
				currentTOTargetMarker.SetActive(false);
			}
		}

		private void TileModeUpdate(RaycastHit hit)
		{
			var previousBuildingPoint = m_BuildingPoint;

			TileObjectSnap(hit.point.x, hit.point.z);
			
			if (!GameClient.RoomManager.IsTileWithinBase(new int2((int) m_BuildingPoint.x, (int) m_BuildingPoint.y)))
			{
				BuildingPreview.GetComponent<OutlineObject>().Color = Color.red;
				IsBlocked = true;
			}

			// Only update stuff if something has changed.
			if (m_BuildingPoint == previousBuildingPoint)
				return;

			if (m_TileStartPointSet)
			{
				m_TileBoxTarget = new int2((int)m_BuildingPoint.x, (int)m_BuildingPoint.y);

				int2 startPos = new int2(
					m_TileBoxTarget.x > m_TileStartPoint.x ? m_TileStartPoint.x : m_TileBoxTarget.x,
					m_TileBoxTarget.y > m_TileStartPoint.y ? m_TileStartPoint.y : m_TileBoxTarget.y
				);

				int2 endPos = new int2(
					m_TileStartPoint.x > m_TileBoxTarget.x ? m_TileStartPoint.x : m_TileBoxTarget.x,
					m_TileStartPoint.y > m_TileBoxTarget.y ? m_TileStartPoint.y : m_TileBoxTarget.y
				);

				BuildingPreview.transform.position = new Vector3(
					startPos.x + 0.5f + ((endPos.x - startPos.x) / 2f),
					0,
					startPos.y + 0.5f + ((endPos.y - startPos.y) / 2f)
				);

				BuildingPreview.transform.localScale = new Vector3((endPos.x - startPos.x) + 1, 0.1f, (endPos.y - startPos.y) + 1);

				Color colorVar;
				int buildabletiles;
				if (CalculateBuildableTiles(out colorVar, out buildabletiles))
				{
					BuildingPreview.GetComponent<OutlineObject>().Color = colorVar;
					IsBlocked = buildabletiles == 0;
				}
				else
				{
					BuildingPreview.GetComponent<OutlineObject>().Color = Color.green;
					IsBlocked = false;
				}

				if (buildabletiles > 0)
				{
					bool hasMaterials = true;
					foreach (MaterialDef md in m_BuildingData.requiredMaterials)
					{
						hasMaterials &= GameClient.LocalPlayerEntity.GetComponent<InventoryComponentClient>().GetItemCount(md.itemRef) >= md.amount * buildabletiles;
					}

					if (!hasMaterials)
					{
						BuildingPreview.GetComponent<OutlineObject>().Color = Color.red;
						IsBlocked = true;
					}

					UIManager.HoverBoxGen.CreateWallCostHoverBox(m_BuildingData, buildabletiles, hasMaterials);
				}
				else
				{
					HB_Factory.DestroyHoverbox();
				}
			}
			else
			{
				BuildingPreview.transform.position = new Vector3(
					m_BuildingPoint.x,
					0,
					m_BuildingPoint.y
				);
				
				bool hasMaterials = true;
				foreach (MaterialDef md in m_BuildingData.requiredMaterials)
				{
					hasMaterials &= GameClient.LocalPlayerEntity.GetComponent<InventoryComponentClient>().GetItemCount(md.itemRef) >= md.amount * 1;
				}

				var tileObject = GameClient.World.GetTileObject((int)m_BuildingPoint.x, (int)m_BuildingPoint.y);
				bool blockedTileBuilding = tileObject != null && tileObject.Data.blocksTileBuilding;

				if (TileNotBuildable((int)m_BuildingPoint.x, (int)m_BuildingPoint.y))
				{
					BuildingPreview.GetComponent<OutlineObject>().Color = Color.red;
				} else
				{
					BuildingPreview.GetComponent<OutlineObject>().Color = Color.green;
				}
			}
		}

		public void OnBuildStart()
		{
			if (m_ObjectType == BuildingType.WALL)
			{
				m_WallStartPoint = m_BuildingPoint;
				m_WallStartPointSet = true;

				m_WallDX = 0;
				m_WallDY = 0;

				IsBlocked = false;
			}
			else if (m_ObjectType == BuildingType.TILE)
			{
				if (TileNotBuildable((int)m_BuildingPoint.x, (int)m_BuildingPoint.y))
				{
					BuildingPreview.GetComponent<OutlineObject>().Color = Color.red;
				}
				else
				{
					BuildingPreview.GetComponent<OutlineObject>().Color = Color.green;
				}
				IsBlocked = false;
				
				m_TileStartPoint = new int2((int)m_BuildingPoint.x, (int)m_BuildingPoint.y);
				m_TileStartPointSet = true;

				m_TileBoxTarget = new int2(m_TileStartPoint.x, m_TileStartPoint.y);

				bool hasMaterials = true;
				foreach (MaterialDef md in m_BuildingData.requiredMaterials)
				{
					hasMaterials &= GameClient.LocalPlayerEntity.GetComponent<InventoryComponentClient>().GetItemCount(md.itemRef) >= md.amount;
				}

				IsBlocked = !hasMaterials;
			}
			else if (m_OutlineObject.activeInHierarchy && m_ObjectType == BuildingType.ADDON)
			{
				FNEEntity clickedEntity = null;
				if (m_OutlineObject.name.Contains("WMP"))
				{
					clickedEntity = GameClient.World.GetEdgeObjectAtPosition(
						new float2(
							m_OutlineObject.transform.position.x,
							m_OutlineObject.transform.position.z
						)
					);
				}else if (m_OutlineObject.name.Contains("TOMP"))
				{
					clickedEntity = GameClient.World.GetTileObject(
						(int)m_OutlineObject.transform.position.x,
						(int)m_OutlineObject.transform.position.z
					);
				}

				ShowAddonMenu(clickedEntity);
			}
			else if (!IsBlocked)
			{
				PlaceBuilding();
			}
		}

		public bool TileNotBuildable(int x, int y)
        {
			bool hasMaterials = true;
			foreach (MaterialDef md in m_BuildingData.requiredMaterials)
			{
				hasMaterials &= GameClient.LocalPlayerEntity.GetComponent<InventoryComponentClient>().GetItemCount(md.itemRef) >= md.amount * 1;
			}

			var tileObject = GameClient.World.GetTileObject(x, y);
			bool blockedTileBuilding = tileObject != null && tileObject.Data.blocksTileBuilding;

			return (
				!hasMaterials ||
				(!GameClient.RoomManager.IsTileWithinBase(new int2(x, y))) ||
				blockedTileBuilding
			);

		}

		public void ShowAddonMenu(FNEEntity clickedEntity)
		{
			foreach (Transform t in UIManager.Instance.AddonAnchor)
			{
				Destroy(t.gameObject);
			}

			ActiveAddonMenu = Instantiate(P_AddonMenu);
			ActiveAddonMenu.transform.SetParent(UIManager.Instance.AddonAnchor);
			ActiveAddonMenu.GetComponent<AddonUI>().Init(clickedEntity, this);

			m_OutlineObject.GetComponent<OutlineObject>().enabled = false;
		}

		public void ExecuteAddonOption(FNEEntity target, BuildingAddonData addonData)
		{
			GameClient.LocalPlayerEntity.GetComponent<BuilderComponentClient>().NE_Send_Build_Addon(
				target.Position.x,
				target.Position.y,
				(int)target.RotationDegrees,
				addonData.Id
			);

			previousPlayerTilePos = new int2(-100000, -100000);
		}

		public void ExecuteRemoveMountOption(FNEEntity target)
		{
			GameClient.LocalPlayerEntity.GetComponent<BuilderComponentClient>().NE_Send_Remove_Mounted_Object(
				target.Position.x,
				target.Position.y
			);

			previousPlayerTilePos = new int2(-100000, -100000);
		}

		public void OnBuildEnd()
		{
			if (m_ObjectType == BuildingType.WALL)
			{
				m_WallStartPointSet = false;

				HB_Factory.DestroyHoverbox();

				BuildingPreview.transform.localScale = new Vector3(0.1f, 2f, 0.1f);
				m_BuildingPoint = Vector2.zero;

				if (m_WallDX != 0)
				{
					int startX = m_WallDX > 0 ? (int)m_WallStartPoint.x : (int)m_WallStartPoint.x + m_WallDX;
					int endX = startX + Mathf.Abs(m_WallDX);

					PlaceWall(startX, (int)m_WallStartPoint.y, endX, (int)m_WallStartPoint.y);
				}

				if (m_WallDY != 0)
				{
					int startY = m_WallDY > 0 ? (int)m_WallStartPoint.y : (int)m_WallStartPoint.y + m_WallDY;
					int endY = startY + Mathf.Abs(m_WallDY);

					PlaceWall((int)m_WallStartPoint.x, startY, (int)m_WallStartPoint.x, endY);
				}
			}
			else if (m_ObjectType == BuildingType.TILE)
			{
				m_TileStartPointSet = false;

				HB_Factory.DestroyHoverbox();

				BuildingPreview.transform.localScale = new Vector3(1f, 0.1f, 1f);
				m_BuildingPoint = Vector2.zero;

				int2 startPos = new int2(
					m_TileBoxTarget.x > m_TileStartPoint.x ? m_TileStartPoint.x : m_TileBoxTarget.x,
					m_TileBoxTarget.y > m_TileStartPoint.y ? m_TileStartPoint.y : m_TileBoxTarget.y
				);

				int2 endPos = new int2(
					m_TileStartPoint.x > m_TileBoxTarget.x ? m_TileStartPoint.x : m_TileBoxTarget.x,
					m_TileStartPoint.y > m_TileBoxTarget.y ? m_TileStartPoint.y : m_TileBoxTarget.y
				);

				PlaceTiles((int)startPos.x, (int)startPos.y, endPos.x, endPos.y);
			}

			BuildingPreview.GetComponent<OutlineObject>().Color = Color.green;
		}


		private void TileObjectSnap(float x, float y)
		{
			Vector2 pos = new Vector2(Mathf.Round(x), Mathf.Round(y)); //round pos so we get tile left bottom corner

			pos.x = ((int)x) + 0.5f;
			pos.y = ((int)y) + 0.5f;

			m_BuildingPoint = pos;
		}

		private bool DoesTileOverlap()
		{
			string tileId = IdTranslator.Instance.GetId<TileData>(
				GameClient.World.GetTileIdCode((int)m_BuildingPoint.x, (int)m_BuildingPoint.y)
			);

			if (tileId == m_ProductId)
				return true;

			var tileObject = GameClient.World.GetTileObject((int)m_BuildingPoint.x, (int)m_BuildingPoint.y);

			return tileObject != null && tileObject.Data.blocking;
		}

		private bool DoesTileObjectCollide()
		{
			var tileObject = GameClient.World.GetTileObject((int)m_BuildingPoint.x, (int)m_BuildingPoint.y);
			if (tileObject != null && tileObject.Data.blocksBuilding)
			{
				return true;
			}

			var remotePlayers = GameClient.RemotePlayerEntities;
			foreach (var player in remotePlayers)
			{
				if (FNECollisionUtils.Intersects(new FNECircle(player.Position, 0.3f), new FNERectangle((int)m_BuildingPoint.x, (int)m_BuildingPoint.y, 1, 1)))
				{
					return true;
				}
			}

			if (FNECollisionUtils.Intersects(new FNECircle(GameClient.LocalPlayerEntity.Position, 0.3f), new FNERectangle((int)m_BuildingPoint.x, (int)m_BuildingPoint.y, 1, 1)))
			{
				return true;
			}

			bool tileBlocked = ValidTiles != null && ValidTiles.Count > 0 ? true : false;
			foreach (string tileRef in ValidTiles)
			{
				string tileId = IdTranslator.Instance.GetId<TileData>(GameClient.World.GetTileIdCode((int)m_BuildingPoint.x, (int)m_BuildingPoint.y));
				if (tileId == tileRef)
				{
					tileBlocked = false;
				}
			}

			if (tileBlocked)
			{
				return true;
			}

			return false;
		}

		private bool CalculateBuildableWalls(out Color colorVar, out int buildablewalls)
		{
			colorVar = Color.green;

			int buildableEdges = 0;
			if (m_WallDX != 0)
			{
				int startX = m_WallDX > 0 ? (int)m_WallStartPoint.x : (int)m_WallStartPoint.x + m_WallDX;
				int endX = startX + Mathf.Abs(m_WallDX);

				for (int i = (int)startX; i < endX; i++)
				{
					var edgePos = new float2(i + 0.5f, m_WallStartPoint.y);
					var edgeObject = GameClient.World.GetEdgeObjectAtPosition(edgePos);
					if (edgeObject == null)
					{
						buildableEdges++;
					}

					if (!GameClient.RoomManager.IsEdgeWithinBase(edgePos))
					{
						colorVar = Color.red;
						buildablewalls = 0;
						return true;
					}
				}

				buildablewalls = buildableEdges;

				if (buildableEdges == Mathf.Abs(m_WallDX))
					return false;
				else if (buildableEdges > 0)
				{
					colorVar = Color.yellow;
					return true;
				}
				else
				{
					colorVar = Color.red;
					return true;
				}

			}
			else if (m_WallDY != 0)
			{
				int startY = m_WallDY > 0 ? (int)m_WallStartPoint.y : (int)m_WallStartPoint.y + m_WallDY;
				int endY = startY + Mathf.Abs(m_WallDY);

				for (int i = startY; i < endY; i++)
				{
					var edgePos = new float2(m_WallStartPoint.x, i + 0.5f);
					var edgeObject = GameClient.World.GetEdgeObjectAtPosition(edgePos);
					if (edgeObject == null)
					{
						buildableEdges++;
					}
					
					if (!GameClient.RoomManager.IsEdgeWithinBase(edgePos))
					{
						colorVar = Color.red;
						buildablewalls = 0;
						return true;
					}
				}

				buildablewalls = buildableEdges;

				if (buildableEdges == Mathf.Abs(m_WallDY))
					return false;
				else if (buildableEdges > 0)
				{
					colorVar = Color.yellow;
					return true;
				}
				else
				{
					colorVar = Color.red;
					return true;
				}
			}

			buildablewalls = 0;

			return false;
		}

		private bool CalculateBuildableTiles(out Color colorVar, out int buildableTiles)
		{
			colorVar = Color.green;

			int buildableTilesCalc = 0;

			int2 startPos = new int2(
				m_TileBoxTarget.x > m_TileStartPoint.x ? m_TileStartPoint.x : m_TileBoxTarget.x,
				m_TileBoxTarget.y > m_TileStartPoint.y ? m_TileStartPoint.y : m_TileBoxTarget.y
			);
 
			int2 endPos = new int2(
				m_TileStartPoint.x > m_TileBoxTarget.x ? m_TileStartPoint.x : m_TileBoxTarget.x,
				m_TileStartPoint.y > m_TileBoxTarget.y ? m_TileStartPoint.y : m_TileBoxTarget.y
			);

			if (!GameClient.RoomManager.IsTileWithinBase(startPos) || !GameClient.RoomManager.IsTileWithinBase(endPos))
			{
				colorVar = Color.red;
				buildableTiles = 0;
				return true;
			}

			for (int y = (int)startPos.y; y <= endPos.y; y++)
			{
				for (int x = (int)startPos.x; x <= endPos.x; x++)
				{
					var oldTileId = GameClient.World.GetTileId(x, y);
					var tileObject = GameClient.World.GetTileObject(x, y);
					if (
						oldTileId != m_BuildingData.productRef && 
						(tileObject == null || !tileObject.Data.blocksTileBuilding)
					)
					{
						buildableTilesCalc++;
					}
				}
			}

			buildableTiles = buildableTilesCalc;

			if (buildableTiles == (Mathf.Abs(endPos.x - startPos.x) + 1) * (Mathf.Abs(endPos.y - startPos.y) + 1))
			{
				colorVar = Color.green;
				return false;
			}
			else if (buildableTiles > 0)
			{
				colorVar = Color.yellow;
				return true;
			}
			else
			{
				colorVar = Color.red;
				return true;
			}
		}

		public void StartAddonMode()
		{
			m_WallStartPointSet = false;
			m_TileStartPointSet = false;

			if (BuildingPreview != null)
			{
				Destroy(BuildingPreview);
			}

			m_ObjectType = BuildingType.ADDON;
		}

		public void SelectBuilding(BuildingData buildingData)
		{
			m_OutlineObject.SetActive(false);

			m_WallStartPointSet = false;
			m_TileStartPointSet = false;

			if (BuildingPreview != null)
			{
				Destroy(BuildingPreview);
			}

			m_BuildingData = buildingData;
			m_ProductId = buildingData.productRef;
			ValidTiles = buildingData.validTiles;

			Mesh mesh = Mesh_Cube;

			Vector3 previewMeshScale = Vector3.one;

			bool isTile = DataBank.Instance.GetAllDataIdsOfType<TileData>().Find(
			   t => t.Id.Equals(m_ProductId)
			) != null;

			BuildingPreview = Instantiate(P_BuildingPreview);

			if (isTile)
			{
				m_ObjectType = BuildingType.TILE;
				m_TileStartPointSet = false;

				HB_Factory.DestroyHoverbox();

				BuildingPreview.transform.localScale = new Vector3(1f, 0.1f, 1f);
				m_BuildingPoint = Vector2.zero;
			}
			else if (DataBank.Instance.IsIdOfType<MountedObjectData>(m_ProductId))
			{
				var mountedData = DataBank.Instance.GetData<MountedObjectData>(m_ProductId);
				var mountedViewRef = DataBank.Instance.GetData<MountedObjectData>(m_ProductId).viewVariations[0];
				var mountedViewData = DataBank.Instance.GetData<FNEEntityViewData>(mountedViewRef);

				GameObject origin = PrefabBank.GetPrefab(null, mountedViewRef);

				var meshFilter = origin.GetComponentInChildren<MeshFilter>();
				if (meshFilter != null)
				{
					mesh = meshFilter.mesh;
				}
				else
				{
					var meshRenderer = origin.GetComponentInChildren<SkinnedMeshRenderer>();
					mesh = meshRenderer.sharedMesh;
				}

				m_ObjectType = BuildingType.MOUNTED_OBJECT;
				BuildingPreview.transform.localScale = Vector3.one * 1.01f * mountedViewData.scaleMod;
				previousPlayerTilePos = new int2(-100000, -100000);
			}
			else
			{
				var entityData = DataBank.Instance.GetData<FNEEntityData>(m_ProductId);
				var entityViewRef = DataBank.Instance.GetData<FNEEntityData>(m_ProductId).entityViewVariations[0];
				var entityViewData = DataBank.Instance.GetData<FNEEntityViewData>(entityViewRef);
				if (entityData.entityType == EntityType.TILE_OBJECT)
				{
					m_ObjectType = BuildingType.TILE_OBJECT;

					GameObject origin = PrefabBank.GetPrefab(null, entityViewRef);
					if (m_ObjectType != BuildingType.TILE)
					{
						//mesh = origin.GetComponentInChildren<MeshFilter>().sharedMesh;
						var meshFilter = origin.GetComponentInChildren<MeshFilter>();
						if (meshFilter != null)
						{
							mesh = meshFilter.mesh;
							previewMeshScale = meshFilter.transform.localScale;
						}
						else
						{
							var meshRenderer = origin.GetComponentInChildren<SkinnedMeshRenderer>();
							mesh = meshRenderer.sharedMesh;
							previewMeshScale = meshRenderer.transform.localScale;
						}
					}

					//previewMeshScale = origin.GetComponentInChildren<MeshFilter>().transform.localScale;

					BuildingPreview.transform.localScale = previewMeshScale * 1.01f * entityViewData.scaleMod;
				}
				else if (entityData.entityType == EntityType.EDGE_OBJECT)
				{
					m_ObjectType = BuildingType.WALL;
					BuildingPreview.transform.localScale = new Vector3(0.1f, 2f, 0.1f);
					previousPlayerTilePos = new int2(-100000, -100000);
				}
				else
				{
					return;
				}
			}

			BuildingPreview.GetComponent<MeshFilter>().mesh = mesh;
			var mr = BuildingPreview.GetComponent<MeshRenderer>(); //set transparent material here
			mr.sharedMaterial = Resources.Load<Material>("Material/UI/Build");
			BuildingPreview.name = "§§§";
			BuildingPreview.transform.rotation = Quaternion.Euler(0, 0, 0);

			if (m_ObjectType != BuildingType.WALL && m_ObjectType != BuildingType.TILE)
			{
				var BuildingDirectionSprite = Instantiate(P_DirectionSprite);
				BuildingDirectionSprite.name = "Direction";
				BuildingDirectionSprite.transform.SetParent(BuildingPreview.transform);

				if (m_ObjectType == BuildingType.MOUNTED_OBJECT)
				{
					var mountedData = DataBank.Instance.GetData<MountedObjectData>(m_ProductId);
					var mountedViewRef = DataBank.Instance.GetData<MountedObjectData>(m_ProductId).viewVariations[0];
					var mountedViewData = DataBank.Instance.GetData<FNEEntityViewData>(mountedViewRef);

					BuildingDirectionSprite.transform.localPosition = new Vector3(0, 2.2f / mountedViewData.scaleMod, 0);
				}
				else
				{
					BuildingDirectionSprite.transform.localPosition = new Vector3(0, 0.2f, 0);
				}
			}

			this.enabled = true;
		}

		public void PlaceBuilding()
		{
			if (m_ObjectType == BuildingType.TILE_OBJECT)
			{
				short rot = (short)m_BuildingRotation;
				if (rot == 360) rot = 0;

				// Cast position to ints to get tile position rather than actual position (transposed by one half tile)
				GameClient.LocalPlayerEntity.GetComponent<BuilderComponentClient>().NE_Send_Build(
					(int)m_BuildingPoint.x,
					(int)m_BuildingPoint.y,
					rot,
					m_BuildingData.Id
				);
			}
			else if (m_ObjectType == BuildingType.TILE)
			{
				GameClient.LocalPlayerEntity.GetComponent<BuilderComponentClient>().NE_Send_Build(
					m_BuildingPoint.x,
					m_BuildingPoint.y,
					0,
					m_BuildingData.Id
				);
			}
			else if (m_ObjectType == BuildingType.MOUNTED_OBJECT)
			{
				GameClient.LocalPlayerEntity.GetComponent<BuilderComponentClient>().NE_Send_Build_Mounted_Object(
					BuildingPreview.transform.position.x,
					BuildingPreview.transform.position.z,
					m_MountedObjectOppositeDirection,
					m_BuildingData.Id
				);
			}
		}

		public void PlaceWall(
			int startX,
			int startY,
			int endX,
			int endY
		)
		{
			if (IsBlocked)
				return;

			GameClient.LocalPlayerEntity.GetComponent<BuilderComponentClient>().NE_Send_Build_Wall(
				startX,
				startY,
				endX,
				endY,
				m_BuildingData.Id
			);
		}

		public void PlaceTiles(
			int startX,
			int startY,
			int endX,
			int endY
		)
		{
			if (IsBlocked)
				return;

			GameClient.LocalPlayerEntity.GetComponent<BuilderComponentClient>().NE_Send_Build_Tiles(
				startX,
				startY,
				endX,
				endY,
				m_BuildingData.Id
			);
		}

		public Vector2 GetBuildVector()
		{
			return this.m_BuildingPoint;
		}

		public float GetBuildRotation()
		{
			return this.m_BuildingRotation;
		}

		public bool IsAddonMode()
		{
			return m_ObjectType == BuildingType.ADDON;
		}

		public void CancelBuild()
		{
			if (BuildingPreview)
				GameObject.Destroy(BuildingPreview);
			m_ObjectType = BuildingType.ADDON;
			
			m_OutlineObject.SetActive(true);
		}

		public void OnOpenBuildMode()
		{
			m_ObjectType = BuildingType.ADDON;
			UpdateWallTargets((int2)GameClient.LocalPlayerEntity.Position);
			UpdateTileObjectTargets((int2)GameClient.LocalPlayerEntity.Position);
		}

		public void OnExitBuildMode()
		{
			m_ObjectType = BuildingType.NADA;
			m_OutlineObject.SetActive(false);
			if (BuildingPreview)
				GameObject.Destroy(BuildingPreview);
			BuildingAddonHintsUI.Instance.ClearAddonHints();
		}
	}
}