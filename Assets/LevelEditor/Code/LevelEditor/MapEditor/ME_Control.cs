using Assets.LevelEditor.Code.LevelEditor;
using FNZ.Client.View.Prefab;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.EntityViewData;
using FNZ.Shared.Model.Entity.MountedObject;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FNZ.LevelEditor
{
	public class ME_Control : MonoBehaviour
	{

		public static LevelEditorApplication LevelEditorMain;

		public GameObject P_TileChunk;

		public GameObject P_FPObject;

		public GameObject G_ExportSelection;
		public GameObject G_ImportSelection;

		public static GameObject G_PreviewCube;
		public static GameObject G_FPObjectHandler;

		public static Dictionary<int, GameObject> ChunkDict = new Dictionary<int, GameObject>();

		public enum BrushSize
		{
			PIXEL = 1,
			FOUR = 2,
			NINE = 3,
			SIXTEEN = 4,
			TWENTYFIVE = 5,
			THIRTYSIX = 6,
			FORTYNINE = 7,
			SIXTYFOUR = 8,
			HUNDRED = 10,
			FOURHUNDRED = 20,
			FULLCHUNK = 32,
		}

		public static BrushSize activeBrushSize = BrushSize.PIXEL;

		public enum PaintMode
		{
			NONE,
			TILE,
			WALL,
			FLOATING_ENTITY,
			TILE_OBJECT,
			DANGER_LEVEL,
			BLOCKING_MASK,
			WALL_CONSTRUCT,
			FPOBJECT_MANIPULATOR,
			EXPORT_SELECTION,
			IMPORT_SITE,
			NULL_TILE,
			MOUNTED_OBJECT
		}

		public enum SnapMode
		{
			CORNER,
			TILE,
			NONE
		}

		public static PaintMode ActivePaintMode = PaintMode.NONE;
		public static PaintMode activePaintMode
		{
			get
			{
				return ActivePaintMode;
			}
			set
			{
				ActivePaintMode = value;

				G_PreviewCube.GetComponentInChildren<MeshFilter>().sharedMesh =
					G_PreviewCube.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh;

				G_PreviewCube.transform.rotation = Quaternion.Euler(
					0, 0, 0
				);
			}
		}
		public static SnapMode activeSnapMode = SnapMode.CORNER;

		public static string SelectedTile = "default_tile";
		public static byte SelectedDangerLevel = 0;
		public static string SelectedWall = "wall";
		private static string selectedMountedObject;
		public static string SelectedMountedObject
		{
			get
			{
				return selectedMountedObject;
			}
			set
			{
				selectedMountedObject = value;

				if (value != null)
				{
					G_PreviewCube.GetComponentInChildren<MeshFilter>().sharedMesh =
					   PrefabBank.GetPrefab(null, DataBank.Instance.GetData<MountedObjectData>(selectedMountedObject).viewVariations[0]).
						   GetComponentInChildren<MeshFilter>().sharedMesh;

					G_PreviewCube.transform.rotation = Quaternion.Euler(
						0, 180, 90
					);

					G_PreviewCube.transform.localScale = PrefabBank.GetPrefab(null, DataBank.Instance.GetData<MountedObjectData>(selectedMountedObject).viewVariations[0]).transform.localScale;
				}
			}
		}

		private static string selectedFPObject;
		public static string SelectedFPObject
		{
			get
			{
				return selectedFPObject;
			}
			set
			{
				selectedFPObject = value;

				if (value != null)
				{
					G_PreviewCube.GetComponentInChildren<MeshFilter>().sharedMesh =
					   PrefabBank.GetPrefab(null, DataBank.Instance.GetData<FNEEntityData>(selectedFPObject).entityViewVariations[0]).
						   GetComponentInChildren<MeshFilter>().sharedMesh;

					G_PreviewCube.transform.rotation = Quaternion.Euler(
						0, 180, 90
					);
					G_PreviewCube.transform.localScale = PrefabBank.GetPrefab(null, DataBank.Instance.GetData<FNEEntityData>(selectedFPObject).entityViewVariations[0]).transform.localScale;
				}
			}
		}
		private static string selectedTileObject;
		public static string SelectedTileObject
		{
			get
			{
				return selectedTileObject;
			}
			set
			{
				selectedTileObject = value;

				var prefab = PrefabBank.GetPrefab(null, DataBank.Instance.GetData<FNEEntityData>(selectedTileObject).entityViewVariations[0]);
				if(prefab.GetComponentInChildren<MeshFilter>() != null)
				{
					G_PreviewCube.GetComponentInChildren<MeshFilter>().sharedMesh = prefab.GetComponentInChildren<MeshFilter>().sharedMesh;
				}
				else
				{
					G_PreviewCube.GetComponentInChildren<MeshFilter>().sharedMesh = prefab.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh;
				}

				G_PreviewCube.transform.rotation = Quaternion.Euler(
					270, 0, 0
				);

				var entityData = DataBank.Instance.GetData<FNEEntityData>(value);
				var viewData = DataBank.Instance.GetData<FNEEntityViewData>(entityData.entityViewVariations[0]);
				G_PreviewCube.transform.localScale = Vector3.one * viewData.scaleMod;
			}
		}

		private static Vector2 WallStartPos;

		EventSystem m_EventSystem;
		GraphicRaycaster m_Raycaster;

		public static int ExportStartX = -1, ExportStartY = -1;
		public static int ExportEndX = -1, ExportEndY = -1;

		private static string SiteToImportFilePath;
		private static int2 SiteToImportSize;

		public void Start()
		{
			m_Raycaster = GameObject.FindObjectOfType<Canvas>().GetComponent<GraphicRaycaster>();
			//Fetch the Event System from the Scene
			m_EventSystem = GameObject.FindObjectOfType<EventSystem>();

			G_PreviewCube = transform.GetChild(0).gameObject;
			G_PreviewCube.transform.rotation = Quaternion.Euler(0, 180, -90);

			G_FPObjectHandler = transform.GetChild(1).gameObject;
			G_FPObjectHandler.SetActive(false);
		}

		public void Update()
		{

			if (Input.GetKeyDown(KeyCode.F11))
				foreach (GameObject wall in GameObject.FindGameObjectsWithTag("EdgeObject"))
				{
					float x = wall.transform.position.x;
					float y = wall.transform.position.y;

					float chunkX = x / 32.0f;
					float chunkY = y / 32.0f;

					if (chunkX >= 4 && chunkX <= 9 && chunkY >= 4 && chunkY <= 9)
					{
						if (((int)wall.transform.rotation.eulerAngles.z) % 180 == 0)
							wall.transform.Rotate(Vector3.back, 180);
					}
				}


			if (ME_Chunk.ActivechunkMode == ME_Chunk.ChunkMode.DANGERMAP && activePaintMode != PaintMode.DANGER_LEVEL)
			{
				ME_Chunk.ActivechunkMode = ME_Chunk.ChunkMode.FLOOR;
			}

			if (ME_Chunk.ActivechunkMode == ME_Chunk.ChunkMode.BLOCKINGMASK && activePaintMode != PaintMode.BLOCKING_MASK)
			{
				ME_Chunk.ActivechunkMode = ME_Chunk.ChunkMode.FLOOR;
			}

			if (Input.GetMouseButtonDown(1) && ME_FPObjectHandler.ActiveHandle != ME_FPObjectHandler.HandleType.NONE)
			{
				ME_FPObjectHandler.ActiveHandle = ME_FPObjectHandler.HandleType.NONE;
				return;
			}
			else if (Input.GetMouseButtonDown(1))
			{
				activePaintMode = PaintMode.NONE;
				SelectedWall = null;
				SelectedFPObject = null;
				selectedTileObject = null;
				SelectedDangerLevel = 0;
				G_ImportSelection.transform.position = Vector3.one * -100000;
			}

			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Input.GetMouseButton(0))
			{
				if (activePaintMode == PaintMode.NONE)
					return;

				if (CheckUIHit())
					return;

				Vector3 impactPoint = Vector3.zero;

				if (Physics.Raycast(ray, out hit))
				{
					if (activePaintMode == PaintMode.WALL &&
						hit.collider.gameObject.layer == LayerMask.NameToLayer("EditorCol"))
					{
						Debug.LogWarning("PaintWall");
						Transform objectHit = hit.transform;

						var newWall = PrefabBank.GetInstanceOfFNEEntityPrefab(SelectedWall);
						newWall.tag = "EdgeObject";

						newWall.transform.position = hit.collider.transform.parent.position;
						newWall.transform.rotation = hit.collider.transform.parent.rotation;

						newWall.name = SelectedWall;

						var entityData = DataBank.Instance.GetData<FNEEntityData>(SelectedWall);
						var viewData = DataBank.Instance.GetData<FNEEntityViewData>(entityData.entityViewVariations[0]);
						newWall.transform.localScale = Vector3.one * viewData.scaleMod;
						LevelEditorUtils.AddEdgeObjectEditorHitbox(newWall);

						Destroy(hit.collider.transform.parent.gameObject);
					}

					if (activePaintMode == PaintMode.MOUNTED_OBJECT &&
						hit.collider.gameObject.layer == LayerMask.NameToLayer("EditorCol"))
					{
                        if (GetChildMountedObject(hit.collider.transform.parent))
                        {
							return;
                        }

						Debug.LogWarning("MOUNT OBJECTERINO!");
						Transform objectHit = hit.transform;

						var entityData = DataBank.Instance.GetData<MountedObjectData>(SelectedMountedObject);
						var viewData = DataBank.Instance.GetData<FNEEntityViewData>(entityData.viewVariations[0]);

						var newMounted = PrefabBank.GetInstanceOfMountedObjectPrefab(SelectedMountedObject);
						newMounted.tag = "MountedObject";

						newMounted.transform.position = hit.collider.transform.parent.position;
						newMounted.transform.rotation = hit.collider.transform.parent.rotation;

						newMounted.name = SelectedMountedObject;

						newMounted.transform.localScale = Vector3.one * viewData.scaleMod;

						newMounted.transform.SetParent(hit.collider.transform.parent);
					}
				}
			}

			G_PreviewCube.SetActive(false);

			int layerMask = 1 << LayerMask.NameToLayer("EditorGrid");
			if (Physics.Raycast(ray, out hit, 1000f, layerMask))
			{
				if (CheckUIHit())
					return;

				if (activePaintMode == PaintMode.FLOATING_ENTITY)
				{
					if (activeSnapMode == SnapMode.CORNER)
					{
						G_PreviewCube.SetActive(true);

						G_PreviewCube.transform.position = new Vector3(
							Mathf.RoundToInt(hit.point.x),
							Mathf.RoundToInt(hit.point.y),
							0
						);

						//G_PreviewCube.GetComponentInChildren<MeshFilter>().mesh =
						//    FPObjectBank.GetFPObjectPrefab(selectedFPObject).GetComponentInChildren<MeshFilter>().sharedMesh;

						if (Input.GetKeyDown(KeyCode.R))
						{
							G_PreviewCube.transform.Rotate(Vector3.back, -90);
						}

						if (Input.GetMouseButtonDown(0))
						{
							GameObject newObject = PrefabBank.GetInstanceOfFNEEntityPrefab(selectedFPObject);

							newObject.transform.position = new Vector3(
								Mathf.RoundToInt(hit.point.x),
								Mathf.RoundToInt(hit.point.y),
								0
							);

							newObject.transform.rotation = G_PreviewCube.transform.rotation;

							//newObject.GetComponent<FPObjectPrefabID>().ME_PaintFPObject(selectedFPObject);
						}
					}

					if (activeSnapMode == SnapMode.TILE)
					{
						G_PreviewCube.SetActive(true);

						float posX = hit.point.x;
						float posY = hit.point.y;

						posX = hit.point.x > Mathf.RoundToInt(hit.point.x) ? Mathf.RoundToInt(hit.point.x) + 0.5f : Mathf.RoundToInt(hit.point.x) - 0.5f;
						posY = hit.point.y > Mathf.RoundToInt(hit.point.y) ? Mathf.RoundToInt(hit.point.y) + 0.5f : Mathf.RoundToInt(hit.point.y) - 0.5f;

						G_PreviewCube.transform.position = new Vector3(
							posX,
							posY,
							0
						);

						if (Input.GetKeyDown(KeyCode.R))
						{
							G_PreviewCube.transform.Rotate(Vector3.back, -90);
						}

						if (Input.GetMouseButtonDown(0))
						{
							GameObject newObject = PrefabBank.GetInstanceOfFNEEntityPrefab(selectedFPObject);

							newObject.transform.position = new Vector3(
								posX,
								posY,
								0
							);

							newObject.transform.rotation = G_PreviewCube.transform.rotation;

							//newObject.GetComponent<FPObjectPrefabID>().ME_PaintFPObject(selectedFPObject);
						}
					}

					if (activeSnapMode == SnapMode.NONE)
					{
						G_PreviewCube.SetActive(true);

						float posX = hit.point.x;
						float posY = hit.point.y;

						G_PreviewCube.transform.position = new Vector3(
							posX,
							posY,
							0
						);

						if (Input.GetKeyDown(KeyCode.R))
						{
							G_PreviewCube.transform.Rotate(Vector3.back, -90);
						}

						if (Input.GetMouseButtonDown(0))
						{
							GameObject newObject = PrefabBank.GetInstanceOfFNEEntityPrefab(selectedFPObject);

							newObject.transform.position = new Vector3(
								posX,
								posY,
								0
							);

							newObject.transform.rotation = G_PreviewCube.transform.rotation;

							//newObject.GetComponent<FPObjectPrefabID>().ME_PaintFPObject(selectedFPObject);
						}
					}
				}
				else if (activePaintMode == PaintMode.TILE_OBJECT)
				{
					G_PreviewCube.SetActive(true);

					float posX = hit.point.x;
					float posY = hit.point.y;

					posX = hit.point.x > Mathf.RoundToInt(hit.point.x) ? Mathf.RoundToInt(hit.point.x) + 0.5f : Mathf.RoundToInt(hit.point.x) - 0.5f;
					posY = hit.point.y > Mathf.RoundToInt(hit.point.y) ? Mathf.RoundToInt(hit.point.y) + 0.5f : Mathf.RoundToInt(hit.point.y) - 0.5f;

					G_PreviewCube.transform.position = new Vector3(
						posX,
						posY,
						0
					);

					if (Input.GetKeyDown(KeyCode.R))
					{
						G_PreviewCube.transform.Rotate(Vector3.up, -90);
					}

					if (Input.GetMouseButtonDown(0))
					{
						GameObject newObject = PrefabBank.GetInstanceOfFNEEntityPrefab(selectedTileObject);
						newObject.tag = "TileObject";

						newObject.transform.position = new Vector3(
							posX,
							posY,
							0
						);

						newObject.transform.rotation = G_PreviewCube.transform.rotation;

						var entityData = DataBank.Instance.GetData<FNEEntityData>(selectedTileObject);
						var viewData = DataBank.Instance.GetData<FNEEntityViewData>(entityData.entityViewVariations[0]);
						newObject.transform.localScale = Vector3.one * viewData.scaleMod;
						LevelEditorUtils.AddTileObjectEditorHitbox(newObject);
						// newObject.transform.Rotate(Vector3.left, 180);

						//newObject.GetComponent<FPObjectPrefabID>().ME_PaintFPObject(selectedFPObject);
					}
				}
				else if (activePaintMode == PaintMode.WALL_CONSTRUCT)
				{
					G_PreviewCube.SetActive(true);

					G_PreviewCube.transform.localScale = new Vector3(0.2f, 0.2f, 3f);

					int mouseX = Mathf.RoundToInt(hit.point.x);
					int mouseY = Mathf.RoundToInt(hit.point.y);

					G_PreviewCube.transform.position = new Vector3(
						mouseX,
						mouseY,
						0
					);

					int dX = (int)mouseX - (int)WallStartPos.x;
					int dY = (int)mouseY - (int)WallStartPos.y;

					if (Mathf.Abs(dX) >= Mathf.Abs(dY))
					{
						dY = 0;
					}
					else
					{
						dX = 0;
					}

					if (Input.GetMouseButtonDown(0))
					{
						WallStartPos = new Vector2(mouseX, mouseY);
					}
					else if (Input.GetMouseButtonUp(0))
					{
						if (dX != 0)
						{
							int startX = dX > 0 ? (int)WallStartPos.x : (int)WallStartPos.x + dX;
							int endX = startX + Mathf.Abs(dX);

							for (int i = (int)startX; i < endX; i++)
							{
								GameObject newWall = PrefabBank.GetInstanceOfFNEEntityPrefab(SelectedWall);
								newWall.tag = "EdgeObject";

								Debug.LogWarning("Created VERTICAL wall at " + i + 0.5f + ", " + WallStartPos.y);

								newWall.transform.position = new Vector2(i + 0.5f, WallStartPos.y);
								if (dX > 0)
									newWall.transform.rotation = Quaternion.Euler(-90, 0, 0);
								else
									newWall.transform.rotation = Quaternion.Euler(90, 180, 0);

								newWall.name = SelectedWall;

								var entityData = DataBank.Instance.GetData<FNEEntityData>(SelectedWall);
								var viewData = DataBank.Instance.GetData<FNEEntityViewData>(entityData.entityViewVariations[0]);
								newWall.transform.localScale = Vector3.one * viewData.scaleMod;
								LevelEditorUtils.AddEdgeObjectEditorHitbox(newWall);
							}
						}

						if (dY != 0)
						{
							int startY = dY > 0 ? (int)WallStartPos.y : (int)WallStartPos.y + dY;
							int endY = startY + Mathf.Abs(dY);

							for (int i = startY; i < endY; i++)
							{
								GameObject newWall = PrefabBank.GetInstanceOfFNEEntityPrefab(SelectedWall);
								newWall.tag = "EdgeObject";

								newWall.transform.position = new Vector2(WallStartPos.x, i + 0.5f);

								Debug.LogWarning("Created VERTICAL wall at " + WallStartPos.x + ", " + i + 0.5f);

								if (dY > 0)
									newWall.transform.rotation = Quaternion.Euler(0, 90, -90);
								else
									newWall.transform.rotation = Quaternion.Euler(0, -90, 90);

								newWall.name = SelectedWall;

								var entityData = DataBank.Instance.GetData<FNEEntityData>(SelectedWall);
								var viewData = DataBank.Instance.GetData<FNEEntityViewData>(entityData.entityViewVariations[0]);
								newWall.transform.localScale = Vector3.one * viewData.scaleMod;
								LevelEditorUtils.AddEdgeObjectEditorHitbox(newWall);
							}
						}

						WallStartPos = Vector2.zero;
					}
					else if (Input.GetMouseButton(0))
					{
						G_PreviewCube.transform.position = new Vector3(
							WallStartPos.x + (dX / 2f),
							WallStartPos.y + (dY / 2f),
							0
						);

						G_PreviewCube.transform.localScale = new Vector3(dY + 0.2f, dX + 0.2f, 3f);
						G_PreviewCube.transform.rotation = Quaternion.Euler(0, 180, 90);
					}
				}

				if (activePaintMode == PaintMode.TILE || 
					activePaintMode == PaintMode.DANGER_LEVEL || 
					activePaintMode == PaintMode.BLOCKING_MASK ||
					activePaintMode == PaintMode.NULL_TILE
				)
				{
					G_PreviewCube.SetActive(true);

					G_PreviewCube.transform.localScale = new Vector3(
						(int)activeBrushSize,
						(int)activeBrushSize,
						0.1f
					);

					float posX = hit.point.x - ((int)activeBrushSize * 0.5f);
					float posY = hit.point.y - ((int)activeBrushSize * 0.5f);

					posX = hit.point.x > Mathf.RoundToInt(hit.point.x) ? Mathf.RoundToInt(hit.point.x) + 0.5f : Mathf.RoundToInt(hit.point.x) - 0.5f;
					posY = hit.point.y > Mathf.RoundToInt(hit.point.y) ? Mathf.RoundToInt(hit.point.y) + 0.5f : Mathf.RoundToInt(hit.point.y) - 0.5f;

					float evenOffset = (int)activeBrushSize % 2 == 0 ? 0.5f : 0;

					G_PreviewCube.transform.position = new Vector3(
						posX + evenOffset,
						posY + evenOffset,
						0
					);
				}
			}

			layerMask = 1 << LayerMask.NameToLayer("EditorGrid");
			if (activePaintMode == PaintMode.TILE
				&& Input.GetMouseButton(0))
			{
				if (Physics.Raycast(ray, out hit, 1000f, layerMask))
				{

					float startPointX = G_PreviewCube.transform.position.x - ((int)activeBrushSize * 0.5f);
					float startPointY = G_PreviewCube.transform.position.y - ((int)activeBrushSize * 0.5f);

					/*if ((int) activeBrushSize % 2 == 0)
                    {
                        centerPointX -= 0.5f;
                        centerPointY -= 0.5f;
                    }*/

					int hitX = Mathf.RoundToInt(startPointX);
					int hitY = Mathf.RoundToInt(startPointY);

					if (hitX < 0) hitX = 0;
					if (hitY < 0) hitY = 0;

					for (int i = hitX; i < hitX + (int)activeBrushSize; i++)
					{
						for (int j = hitY; j < hitY + (int)activeBrushSize; j++)
						{
							PaintTile(i, j);
						}
					}

					short startChunkX = (short)(hitX / 32);
					short startChunkY = (short)(hitY / 32);

					short endChunkX = (short)((hitX + (int)activeBrushSize) / 32);
					short endChunkY = (short)((hitY + (int)activeBrushSize) / 32);

					for (short i = startChunkX; i <= endChunkX; i++)
					{
						for (short j = startChunkY; j <= endChunkY; j++)
						{
							if (ChunkDict.ContainsKey(GetChunkHash(i, j)))
							{
								ChunkDict[GetChunkHash(i, j)].GetComponent<ME_Chunk>().GenerateFloorUVs();
							}
						}
					}
				}
			}

			if (activePaintMode == PaintMode.DANGER_LEVEL
				&& Input.GetMouseButton(0))
			{
				if (Physics.Raycast(ray, out hit, 1000f, layerMask))
				{

					float startPointX = G_PreviewCube.transform.position.x - ((int)activeBrushSize * 0.5f);
					float startPointY = G_PreviewCube.transform.position.y - ((int)activeBrushSize * 0.5f);

					/*if ((int) activeBrushSize % 2 == 0)
                    {
                        centerPointX -= 0.5f;
                        centerPointY -= 0.5f;
                    }*/

					int hitX = Mathf.RoundToInt(startPointX);
					int hitY = Mathf.RoundToInt(startPointY);

					for (int i = hitX; i < hitX + (int)activeBrushSize; i++)
					{
						for (int j = hitY; j < hitY + (int)activeBrushSize; j++)
						{
							if (Input.GetKey(KeyCode.LeftControl))
								SubtractDangerLevel(i, j);
							else
								AddDangerLevel(i, j);
						}
					}
				}
			}

			if (activePaintMode == PaintMode.BLOCKING_MASK && Input.GetMouseButton(0))
			{
				if (Physics.Raycast(ray, out hit, 1000f, layerMask))
				{
					float startPointX = G_PreviewCube.transform.position.x - ((int)activeBrushSize * 0.5f);
					float startPointY = G_PreviewCube.transform.position.y - ((int)activeBrushSize * 0.5f);

					int hitX = Mathf.RoundToInt(startPointX);
					int hitY = Mathf.RoundToInt(startPointY);

					for (int i = hitX; i < hitX + (int)activeBrushSize; i++)
					{
						for (int j = hitY; j < hitY + (int)activeBrushSize; j++)
						{
							SetBlocking(i, j, !Input.GetKey(KeyCode.LeftControl));
						}
					}
				}
			}

			if (activePaintMode == PaintMode.NULL_TILE && Input.GetMouseButton(0))
			{
				if (Physics.Raycast(ray, out hit, 1000f, layerMask))
				{
					float startPointX = G_PreviewCube.transform.position.x - ((int)activeBrushSize * 0.5f);
					float startPointY = G_PreviewCube.transform.position.y - ((int)activeBrushSize * 0.5f);

					int hitX = Mathf.RoundToInt(startPointX);
					int hitY = Mathf.RoundToInt(startPointY);

					for (int i = hitX; i < hitX + (int)activeBrushSize; i++)
					{
						for (int j = hitY; j < hitY + (int)activeBrushSize; j++)
						{
							SetNull(i, j, !Input.GetKey(KeyCode.LeftControl));
						}
					}
				}
			}

			if (activePaintMode == PaintMode.EXPORT_SELECTION && Input.GetMouseButtonDown(0))
			{
				if (Physics.Raycast(ray, out hit, 1000f, layerMask))
				{
					float startPointX = hit.point.x;
					float startPointY = hit.point.y;

					int hitX = Mathf.RoundToInt(startPointX);
					int hitY = Mathf.RoundToInt(startPointY);

					if (hitX < 0) hitX = 0;
					if (hitY < 0) hitY = 0;

					ExportStartX = hitX;
					ExportStartY = hitY;
				}
			}else if (activePaintMode == PaintMode.EXPORT_SELECTION && Input.GetMouseButtonUp(0))
			{
				if (Physics.Raycast(ray, out hit, 1000f, layerMask))
				{
					float endPointX = hit.point.x;
					float endPointY = hit.point.y;

					int hitX = Mathf.RoundToInt(endPointX);
					int hitY = Mathf.RoundToInt(endPointY);

					if (hitX < 0) hitX = 0;
					if (hitY < 0) hitY = 0;

					ExportEndX = hitX;
					ExportEndY = hitY;

					if(ExportEndX < ExportStartX)
                    {
						var tempX = ExportStartX;
						ExportStartX = ExportEndX;
						ExportEndX = tempX;
                    }

					if (ExportEndY < ExportStartY)
					{
						var tempY = ExportStartY;
						ExportStartY = ExportEndY;
						ExportEndY = tempY;
					}

					G_ExportSelection.transform.position = new Vector3(
						ExportStartX + (float)(ExportEndX - ExportStartX) / 2f,
						ExportStartY + (float)(ExportEndY - ExportStartY) / 2f,
						G_ExportSelection.transform.position.z
					);
					G_ExportSelection.transform.localScale = new Vector3(
							(float) (ExportEndX - ExportStartX) * 0.1f,
							1,
							(float) (ExportEndY - ExportStartY) * 0.1f
					);

					Debug.LogWarning("START POINT: (" + ExportStartX + ", " + ExportStartY + ")");
					Debug.LogWarning("END POINT: (" + ExportEndX + ", " + ExportEndY + ")");
				}
			}else if(activePaintMode == PaintMode.EXPORT_SELECTION && ExportStartX >= 0 && Input.GetMouseButton(0))
            {
				float endPointX = hit.point.x;
				float endPointY = hit.point.y;

				int hitX = Mathf.RoundToInt(endPointX);
				int hitY = Mathf.RoundToInt(endPointY);

				if (hitX < 0) hitX = 0;
				if (hitY < 0) hitY = 0;

				var tempStartX = ExportStartX;
				if (ExportEndX < ExportStartX)
				{
					var tempX = tempStartX;
					tempStartX = hitX;
					hitX = tempX;
				}

				var tempStartY = ExportStartY;
				if (ExportEndY < ExportStartY)
				{
					var tempY = tempStartY;
					tempStartY = hitY;
					hitY = tempY;
				}

				G_ExportSelection.transform.position = new Vector3(
					tempStartX + (float)(hitX - tempStartX) / 2f,
					tempStartY + (float)(hitY - tempStartY) / 2f,
					G_ExportSelection.transform.position.z
				);

				G_ExportSelection.transform.localScale = new Vector3(
						(float) (hitX - tempStartX) * 0.1f,
						1,
						(float) (hitY - tempStartY) * 0.1f
				);
			}

			if (activePaintMode == PaintMode.IMPORT_SITE)
            {
				if (Physics.Raycast(ray, out hit, 1000f, layerMask))
				{
					float posX = hit.point.x;
					float posY = hit.point.y;

					posX = hit.point.x > Mathf.RoundToInt(hit.point.x) ? Mathf.RoundToInt(hit.point.x) + 0.5f : Mathf.RoundToInt(hit.point.x) - 0.5f;
					posY = hit.point.y > Mathf.RoundToInt(hit.point.y) ? Mathf.RoundToInt(hit.point.y) + 0.5f : Mathf.RoundToInt(hit.point.y) - 0.5f;

					G_ImportSelection.transform.position = new Vector3(
						SiteToImportSize.x % 2 == 0 ? posX -= 0.5f : posX,
						SiteToImportSize.y % 2 == 0 ? posY -= 0.5f : posY,
						0
					);

					if (Input.GetMouseButtonDown(0))
					{
						ExecuteSiteImport((int) posX, (int) posY);
					}
				}
			}

			if (activePaintMode == PaintMode.NONE || activePaintMode == PaintMode.WALL)
			{
				if (Input.GetKeyDown(KeyCode.R))
				{
					layerMask = 1 << LayerMask.NameToLayer("EditorCol") | 1 << LayerMask.NameToLayer("EditorFPObject");
					Debug.LogWarning("Cast Rotate Ray!");
					if (Physics.Raycast(ray, out hit, 1000f, layerMask))
					{
						var data = DataBank.Instance.GetData<FNEEntityData>(hit.collider.GetComponentInParent<GameObjectIdHolder>().id);
						if (data.entityType == EntityType.EDGE_OBJECT)
                        {
							var mounted = GetChildMountedObject(hit.transform.parent);
                            if (mounted && !Input.GetKey(KeyCode.LeftShift))
                            {
								mounted.Rotate(Vector3.up, 180);
                            }
                            else
                            {
								hit.transform.parent.Rotate(Vector3.up, 180);
							}
						}
						else
							hit.transform.parent.Rotate(Vector3.back, -90);
					}
				}
			}

			if (Input.GetKeyDown(KeyCode.Delete))
			{
				layerMask = 1 << LayerMask.NameToLayer("EditorCol") | 1 << LayerMask.NameToLayer("EditorFPObject");
				if (Physics.Raycast(ray, out hit, 1000f, layerMask))
				{
					var mounted = GetChildMountedObject(hit.transform.parent);
					if (mounted)
					{
						Destroy(mounted.gameObject);
					}
					else
					{
						Destroy(hit.transform.parent.gameObject);
					}
					
				}
			}

			if (activePaintMode == PaintMode.FPOBJECT_MANIPULATOR && Input.GetMouseButtonDown(0))
			{
				layerMask = 1 << LayerMask.NameToLayer("EditorFPObject");
				if (Physics.Raycast(ray, out hit, 1000f, layerMask))
				{
					G_FPObjectHandler.SetActive(true);
					G_FPObjectHandler.GetComponent<ME_FPObjectHandler>().SetObservedObject(hit.collider.transform.parent.gameObject);

					G_FPObjectHandler.transform.localScale = Vector3.one;

					if (ME_FPObjectHandler.ActiveHandle == ME_FPObjectHandler.HandleType.NONE)
						ME_FPObjectHandler.ActiveHandle = ME_FPObjectHandler.HandleType.TRANSLATION;
				}
			}
		}

		private bool CheckUIHit()
		{
			PointerEventData m_PointerEventData = new PointerEventData(m_EventSystem);
			//Set the Pointer Event Position to that of the mouse position
			m_PointerEventData.position = Input.mousePosition;

			//Create a list of Raycast Results
			List<RaycastResult> results = new List<RaycastResult>();

			//Raycast using the Graphics Raycaster and mouse click position
			m_Raycaster.Raycast(m_PointerEventData, results);

			return results.Count > 0;
		}

		private void PaintTile(int worldX, int worldY)
		{
			short chunkX = (short)(worldX / 32);
			short chunkY = (short)(worldY / 32);

			if (ChunkDict.ContainsKey(GetChunkHash(chunkX, chunkY)))
				ChunkDict[GetChunkHash(chunkX, chunkY)].GetComponent<ME_Chunk>().PaintTile(SelectedTile, worldX - (chunkX * 32), worldY - (chunkY * 32));
			else
			{
				while (1 + (chunkX) > LevelEditorMain.widthInTiles)
					LevelEditorMain.widthInTiles += 1;

				while (1 + (chunkY) > LevelEditorMain.heightInTiles)
					LevelEditorMain.heightInTiles += 1;

				for (short i = 0; i < chunkX + 1; i++)
				{
					for (short j = 0; j < LevelEditorMain.heightInTiles; j++)
					{
						if (!ChunkDict.ContainsKey(GetChunkHash(i, j)))
						{
							GameObject newChunk = GameObject.Instantiate(
								P_TileChunk
							);
							newChunk.transform.position = new Vector2(i * 32, j * 32);

							ChunkDict.Add(GetChunkHash(i, j), newChunk);

							newChunk.GetComponent<ME_Chunk>().Init(i, j);

							newChunk.GetComponent<ME_Chunk>().PaintTile("default_tile", 0, 0);
						}
					}
				}

				for (short i = 0; i < chunkY + 1; i++)
				{
					for (short j = 0; j < LevelEditorMain.widthInTiles / 32; j++)
					{
						if (!ChunkDict.ContainsKey(GetChunkHash(j, i)))
						{
							GameObject newChunk = GameObject.Instantiate(
								P_TileChunk
							);
							newChunk.transform.position = new Vector2(j * 32, i * 32);

							ChunkDict.Add(GetChunkHash(j, i), newChunk);

							newChunk.GetComponent<ME_Chunk>().Init(i, j);

							newChunk.GetComponent<ME_Chunk>().PaintTile("default_tile", 0, 0);
						}
					}
				}
			}
		}

		private void AddDangerLevel(int worldX, int worldY)
		{
			short chunkX = (short)(worldX / 32);
			short chunkY = (short)(worldY / 32);

			if (ChunkDict.ContainsKey(GetChunkHash(chunkX, chunkY)))
				ChunkDict[GetChunkHash(chunkX, chunkY)].GetComponent<ME_Chunk>().AddDangerLevel(SelectedDangerLevel, worldX - (chunkX * 32), worldY - (chunkY * 32));
		}

		private void SubtractDangerLevel(int worldX, int worldY)
		{
			short chunkX = (short)(worldX / 32);
			short chunkY = (short)(worldY / 32);

			if (ChunkDict.ContainsKey(GetChunkHash(chunkX, chunkY)))
				ChunkDict[GetChunkHash(chunkX, chunkY)].GetComponent<ME_Chunk>().SubtractDangerLevel(SelectedDangerLevel, worldX - (chunkX * 32), worldY - (chunkY * 32));
		}

		private void SetBlocking(int worldX, int worldY, bool blocking)
		{
			short chunkX = (short)(worldX / 32);
			short chunkY = (short)(worldY / 32);

			if (ChunkDict.ContainsKey(GetChunkHash(chunkX, chunkY)))
				ChunkDict[GetChunkHash(chunkX, chunkY)].GetComponent<ME_Chunk>().SetBlocking(blocking, worldX - (chunkX * 32), worldY - (chunkY * 32));
		}

		private void SetNull(int worldX, int worldY, bool isNull)
		{
			short chunkX = (short)(worldX / 32);
			short chunkY = (short)(worldY / 32);

			if (ChunkDict.ContainsKey(GetChunkHash(chunkX, chunkY)))
				ChunkDict[GetChunkHash(chunkX, chunkY)].GetComponent<ME_Chunk>().SetNull(isNull, worldX - (chunkX * 32), worldY - (chunkY * 32));
		}

		public static void RedrawTileUVs()
		{
			foreach (var chunk in ChunkDict.Values)
			{
				chunk.GetComponent<ME_Chunk>().GenerateFloorUVs();
			}
		}

		public static int GetChunkHash(short x, short y)
		{
			return 523 * x + 541 * y;
		}

		public static bool IsExportSelectionValid()
        {
			int width = ExportEndX - ExportStartX;
			int height = ExportEndY - ExportStartY;

			return width > 0 && height > 0;
		}

		public void StartImportSite(string filePath, int widthInTiles, int heightInTiles)
        {
			SiteToImportFilePath = filePath;
			SiteToImportSize = new int2(widthInTiles, heightInTiles);

			activePaintMode = PaintMode.IMPORT_SITE;
			G_ImportSelection.transform.localScale = new Vector3(
				(float)widthInTiles * 0.1f,
				1,
				(float)heightInTiles * 0.1f
			);

			activeSnapMode = SnapMode.TILE;
		}

		public void ExecuteSiteImport(int selectionCenterX, int selectionCenterY)
		{
			int selectionStartX = 0;
			int selectionStartY = 0;

			if (SiteToImportSize.x % 2 == 0)
            {
				selectionStartX = selectionCenterX - (SiteToImportSize.x / 2);
			}
            else
            {
				selectionStartX = selectionCenterX - (SiteToImportSize.x / 2);
			}

			if (SiteToImportSize.y % 2 == 0)
			{
				selectionStartY = selectionCenterY - (SiteToImportSize.y / 2);
			}
			else
			{
				selectionStartY = selectionCenterY - (SiteToImportSize.y / 2);
			}

			LevelEditorMain.ExecuteSiteImport(
				SiteToImportFilePath,
				selectionStartX,
				selectionStartY
			);

			activePaintMode = PaintMode.NONE;
			G_ImportSelection.transform.position = Vector3.one * -100000;
			activeSnapMode = SnapMode.NONE;

		}

		public static Transform GetChildMountedObject(Transform edgeObjectTransform)
        {
			foreach(Transform t in edgeObjectTransform)
            {
				if(t.tag == "MountedObject")
                {
					return t;
                }
            }

			return null;
        }
	}
}