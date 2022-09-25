using Assets.LevelEditor.Code.LevelEditor;
using FNZ.Client.View.Prefab;
using FNZ.LevelEditor;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.EntityViewData;
using FNZ.Shared.Model.World;
using FNZ.Shared.Model.World.Tile;
using FNZ.Shared.Utils;
using Lidgren.Network;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.LevelEditor.Code.LevelEditorV2.FNSFile 
{
	public class FNSLegacy : MonoBehaviour
	{
		private struct TileSaveData
		{
			public short RelativeX, RelativeY, WorldX, WorldY;

			public GameObject WestEdgeObject;
			public GameObject SouthEdgeObject;
			public GameObject FloatPointObject;
			public GameObject TileObject;

			// Edge cases
			public GameObject EastEdgeObject;
			public GameObject NorthEdgeObject;

			public bool IsNull;
			public string TileId;
		}

		static List<Vector2> savedTileObjPositions;
		static List<Vector2> savedEdgeObjPositions;

		public static void SaveFile(
			string levelName,
			string folderPath,
			int widthInTiles,
			int heightInTiles,
			int originWorldX,
			int originWorldY
		)
		{
			Debug.LogWarning("SAVE SITE: " + levelName);

			savedTileObjPositions = new List<Vector2>();
			savedEdgeObjPositions = new List<Vector2>();

			Debug.Log("Width, height:" + widthInTiles + "|" + heightInTiles);

			IdTranslator.Instance.Clear();

			TileSaveData[,] tileData = new TileSaveData[widthInTiles, heightInTiles];

			for (int x = 0; x < widthInTiles; x++)
			{
				for (int y = 0; y < heightInTiles; y++)
				{
					tileData[x, y] = new TileSaveData()
					{
						RelativeX = (short)x,
						RelativeY = (short)y,
						WorldX = (short)(originWorldX + x),
						WorldY = (short)(originWorldY + y)
					};
				}
			}

			GameObject[] edgeObjects = GameObject.FindGameObjectsWithTag("EdgeObject");

			foreach (GameObject go in edgeObjects)
			{
				if (!IsPositionWithinRect(go.transform.position, originWorldX, originWorldY, widthInTiles, heightInTiles))
				{
					continue;
				}

				bool cont = false;
				foreach (var pos in savedEdgeObjPositions)
				{
					if (pos.x == go.transform.position.x && pos.y == go.transform.position.y)
					{
						Debug.LogWarning("Found multiple TileObjects at: " + pos.x + ":" + pos.y);
						cont = true;
						continue;
					}
				}

				if (cont)
					continue;

				IdTranslator.Instance.GenerateIdIfNeeded<FNEEntityData>(go.GetComponentInChildren<GameObjectIdHolder>().id);

				savedEdgeObjPositions.Add(new Vector2(go.transform.position.x, go.transform.position.y));

				int relativeTileX = (int)(go.transform.position.x - originWorldX);
				int relativeTileY = (int)(go.transform.position.y - originWorldY);

				// East wall of right-most tile
				if (relativeTileX == widthInTiles && go.transform.position.x % 1 == 0)
				{
					tileData[relativeTileX - 1, relativeTileY].EastEdgeObject = go;
					continue;
				}
				// North wall of upper-most tile
				else if (relativeTileY == heightInTiles && go.transform.position.y % 1 == 0)
				{
					tileData[relativeTileX, relativeTileY - 1].NorthEdgeObject = go;
					continue;
				}

				// normal south or west edge objects
				if (go.transform.position.x % 1 == 0)
				{
					tileData[relativeTileX, relativeTileY].WestEdgeObject = go;
				}
				else
				{
					tileData[relativeTileX, relativeTileY].SouthEdgeObject = go;
				}
			}

			/*GameObject[] floatpointObjects = GameObject.FindGameObjectsWithTag("FloatpointObject");

			foreach (GameObject go in floatpointObjects)
			{
				IdTranslator.Instance.GenerateIdIfNeeded<FNEEntityData>(go.GetComponentInChildren<GameObjectIdHolder>().id);

				chunkdata[(int)(go.transform.position.x / 32), (int)(go.transform.position.y / 32)].FloatPointObjects.Add(go);
			}*/

			GameObject[] tileObjects = GameObject.FindGameObjectsWithTag("TileObject");

			foreach (GameObject go in tileObjects)
			{
				if (!IsPositionWithinRect(go.transform.position, originWorldX, originWorldY, widthInTiles, heightInTiles))
				{
					continue;
				}

				bool doCont = false;
				foreach (var pos in savedTileObjPositions)
				{
					if (pos.x == go.transform.position.x && pos.y == go.transform.position.y)
					{
						Debug.LogWarning("Found multiple TileObjects at: " + pos.x + ":" + pos.y);
						doCont = true;
						continue;
					}
				}

				if (doCont) continue;

				IdTranslator.Instance.GenerateIdIfNeeded<FNEEntityData>(go.GetComponentInChildren<GameObjectIdHolder>().id);

				savedTileObjPositions.Add(new Vector2(go.transform.position.x, go.transform.position.y));

				tileData[(int)(go.transform.position.x - originWorldX), (int)(go.transform.position.y - originWorldY)].TileObject = go;
			}

			for (int x = 0; x < widthInTiles; x++)
			{
				for (int y = 0; y < heightInTiles; y++)
				{
					var td = tileData[x, y];
					ME_Chunk chunkComp = ME_Control.ChunkDict[ME_Control.GetChunkHash((short)(td.WorldX / 32), (short)(td.WorldY / 32))].GetComponent<ME_Chunk>();
					tileData[x, y].IsNull = chunkComp.NullMask[td.WorldX % 32, td.WorldY % 32];
					IdTranslator.Instance.GenerateIdIfNeeded<TileData>(chunkComp.TileIds[td.WorldX % 32, td.WorldY % 32]);
					tileData[x, y].TileId = chunkComp.TileIds[td.WorldX % 32, td.WorldY % 32];
				}
			}


			// -------------------- START WRITING ---------------------
			NetBuffer nb = new NetBuffer();

			IdTranslator.Instance.Serialize(nb);

			// terrain width in chunks
			nb.Write((ushort)(widthInTiles));
			// terrain height in chunks
			nb.Write((ushort)(heightInTiles));

			for (int x = 0; x < widthInTiles; x++)
			{
				for (int y = 0; y < heightInTiles; y++)
				{
					var td = tileData[x, y];

					nb.Write(td.IsNull);
					if (td.IsNull)
						continue;

					WriteTileSite(td.TileId, nb);

					nb.Write(td.WestEdgeObject != null);
					if (td.WestEdgeObject != null)
					{
						WriteEdgeObjectSite(td.WestEdgeObject, nb);
					}

					nb.Write(td.SouthEdgeObject != null);
					if (td.SouthEdgeObject != null)
					{
						WriteEdgeObjectSite(td.SouthEdgeObject, nb);
					}

					nb.Write(td.EastEdgeObject != null);
					if (td.EastEdgeObject != null)
					{
						WriteEdgeObjectSite(td.EastEdgeObject, nb);
					}

					nb.Write(td.NorthEdgeObject != null);
					if (td.NorthEdgeObject != null)
					{
						WriteEdgeObjectSite(td.NorthEdgeObject, nb);
					}

					/*foreach (GameObject go in cd.FloatPointObjects)
					{
						WriteFloatpointObject(go, cd.chunk, nb);
					}*/

					nb.Write(td.TileObject != null);
					if (td.TileObject != null)
					{
						WriteTileObjectSite(td.TileObject, nb);
					}
				}
			}

			// Actually write to file
			using (var fs = new FileStream(folderPath + levelName + LevelEditorUtils.SITE_FILE_ENDING, FileMode.Create))
			{
				using (var bw = new BinaryWriter(fs))
				{
					bw.Write(nb.Data, 0, nb.LengthBytes);
				}
			}

			var file = new FileInfo(folderPath + levelName + LevelEditorUtils.SITE_FILE_ENDING);

			while (IsFileLocked(file)) { }

			Debug.Log("Save done!");
		}


		protected static bool IsFileLocked(FileInfo file)
		{
			FileStream stream = null;

			try
			{
				stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
			}
			catch (IOException)
			{
				return true;
			}
			finally
			{
				if (stream != null)
					stream.Close();
			}

			//file is not locked
			return false;
		}

		public static void WriteChunk(GameObject chunk, NetBuffer bw)
		{
			ME_Chunk MEChunk = chunk.GetComponent<ME_Chunk>();
			for (int i = 0; i < 32; i++)
			{
				for (int j = 0; j < 32; j++)
				{
					WriteTile(new Vector2(i, j), MEChunk.TileIds[i, j], MEChunk.DangerLevels[i, j], MEChunk.BlockingMask[i, j], bw);
				}
			}
		}

		public static void WriteTile(Vector2 tilePos, string id, byte dangerLevel, bool blocking, NetBuffer bw)
		{
			bw.Write((byte)1);

			bw.Write(IdTranslator.Instance.GetIdCode<TileData>(id));
			bw.Write(dangerLevel);
			bw.Write(blocking);

			bw.Write((ushort)Mathf.RoundToInt(tilePos.x));
			bw.Write((ushort)Mathf.RoundToInt(tilePos.y));
		}

		public static void WriteEdgeObject(GameObject edgeObject, GameObject chunk, NetBuffer bw)
		{
			var id = edgeObject.GetComponentInChildren<GameObjectIdHolder>().id;

			float xRounded = Mathf.Round(edgeObject.transform.position.x * 2) / 2;
			float yRounded = Mathf.Round(edgeObject.transform.position.y * 2) / 2;

			ushort x = FNEUtil.PackFloatAsShort(xRounded - chunk.transform.position.x);
			ushort y = FNEUtil.PackFloatAsShort(yRounded - chunk.transform.position.y);

			// This extremely dirty rotation shit is due to quick fix for level editor world
			// fake rotation.
			var euler = edgeObject.transform.rotation.eulerAngles;
			var xRot = Mathf.Round(euler.x);
			var yRot = Mathf.Round(euler.y);
			var zRot = Mathf.Round(euler.z);
			short finalZ = 0;
			if (xRot == 270 && yRot == 0 && zRot == 0)
			{
				finalZ = 0;
			}
			else if (xRot == 0 && yRot == 270 && zRot == 90)
			{
				finalZ = 90;
			}
			else if (xRot == 90 && yRot == 180 && zRot == 0)
			{
				finalZ = 180;
			}
			else if (xRot == 0 && yRot == 90 && zRot == 270)
			{
				finalZ = 270;
			}

			// finalZ = (short)Mathf.RoundToInt(FNEUtil.ConvertAngleToBetween0And360(edgeObject.transform.rotation.eulerAngles.z));

			bw.Write((byte)2);
			bw.Write(IdTranslator.Instance.GetIdCode<FNEEntityData>(id));
			bw.Write(finalZ);
			bw.Write(x);
			bw.Write(y);
		}

		public static void WriteFloatpointObject(GameObject floatpointObject, GameObject chunk, NetBuffer bw)
		{
			bw.Write((byte)3);

			bw.Write(FNEUtil.PackFloatAsShort(floatpointObject.transform.position.x - chunk.transform.position.x));
			bw.Write(FNEUtil.PackFloatAsShort(floatpointObject.transform.position.y - chunk.transform.position.y));
			bw.Write(FNEUtil.ConvertFloatToSignedShort(floatpointObject.transform.position.z));

			bw.Write(FNEUtil.PackFloatAsShort(floatpointObject.transform.rotation.eulerAngles.x));
			bw.Write(FNEUtil.PackFloatAsShort(floatpointObject.transform.rotation.eulerAngles.y));
			bw.Write(FNEUtil.PackFloatAsShort(floatpointObject.transform.rotation.eulerAngles.z));

			bw.Write(FNEUtil.PackFloatAsShort(floatpointObject.transform.localScale.x));
			bw.Write(FNEUtil.PackFloatAsShort(floatpointObject.transform.localScale.y));
			bw.Write(FNEUtil.PackFloatAsShort(floatpointObject.transform.localScale.z));

			string id = floatpointObject.GetComponentInChildren<GameObjectIdHolder>().id;
			bw.Write(IdTranslator.Instance.GetIdCode<FNEEntityData>(id));
		}

		public static void WriteTileObject(GameObject tileObject, GameObject chunk, NetBuffer bw)
		{
			ushort x = (ushort)(tileObject.transform.position.x - chunk.transform.position.x);
			ushort y = (ushort)(tileObject.transform.position.y - chunk.transform.position.y);

			bw.Write((byte)4);
			ushort id = IdTranslator.Instance.GetIdCode<FNEEntityData>(tileObject.GetComponentInChildren<GameObjectIdHolder>().id);
			bw.Write(id);
			bw.Write(x);
			bw.Write(y);

			// This extremely dirty rotation shit is due to quick fix for level editor world
			// fake rotation.
			var euler = tileObject.transform.rotation.eulerAngles;
			var xRot = Mathf.Round(euler.x);
			var yRot = Mathf.Round(euler.y);
			var zRot = Mathf.Round(euler.z);
			short finalZ = 0;
			if (xRot == 270 && yRot == 0 && zRot == 0)
			{
				finalZ = 0;
			}
			else if (xRot == 0 && yRot == 270 && zRot == 90)
			{
				finalZ = 90;
			}
			else if (xRot == 90 && yRot == 180 && zRot == 0)
			{
				finalZ = 180;
			}
			else if (xRot == 0 && yRot == 90 && zRot == 270)
			{
				finalZ = 270;
			}

			bw.Write(FNEUtil.PackFloatAsShort(finalZ));

			bw.Write(FNEUtil.PackFloatAsShort(tileObject.transform.localScale.x));
			bw.Write(FNEUtil.PackFloatAsShort(tileObject.transform.localScale.y));
			bw.Write(FNEUtil.PackFloatAsShort(tileObject.transform.localScale.z));
		}

		private static bool IsPositionWithinRect(Vector3 position, int startX, int startY, int width, int height)
		{
			return position.x >= startX && position.x <= startX + width && position.y >= startY && position.y <= startY + height;
		}

		public static void WriteEdgeObjectSite(GameObject edgeObject, NetBuffer bw)
		{
			var id = edgeObject.GetComponentInChildren<GameObjectIdHolder>().id;

			// This extremely dirty rotation shit is due to quick fix for level editor world
			// fake rotation.
			var euler = edgeObject.transform.rotation.eulerAngles;
			var xRot = Mathf.Round(euler.x);
			var yRot = Mathf.Round(euler.y);
			var zRot = Mathf.Round(euler.z);
			short finalZ = 0;
			if (xRot == 270 && yRot == 0 && zRot == 0)
			{
				finalZ = 0;
			}
			else if (xRot == 0 && yRot == 270 && zRot == 90)
			{
				finalZ = 90;
			}
			else if (xRot == 90 && yRot == 180 && zRot == 0)
			{
				finalZ = 180;
			}
			else if (xRot == 0 && yRot == 90 && zRot == 270)
			{
				finalZ = 270;
			}

			// short zRot = (short)Mathf.RoundToInt(FNEUtil.ConvertAngleToBetween0And360(edgeObject.transform.rotation.eulerAngles.z));

			bw.Write(IdTranslator.Instance.GetIdCode<FNEEntityData>(id));
			bw.Write(finalZ);
		}

		public static void WriteTileObjectSite(GameObject tileObject, NetBuffer bw)
		{
			ushort id = IdTranslator.Instance.GetIdCode<FNEEntityData>(tileObject.GetComponentInChildren<GameObjectIdHolder>().id);
			bw.Write(id);

			// This extremely dirty rotation shit is due to quick fix for level editor world
			// fake rotation.
			var euler = tileObject.transform.rotation.eulerAngles;
			var xRot = Mathf.Round(euler.x);
			var yRot = Mathf.Round(euler.y);
			var zRot = Mathf.Round(euler.z);
			ushort finalZ = 0;
			if (xRot == 270 && yRot == 0 && zRot == 0)
			{
				finalZ = 0;
			}
			else if (xRot == 0 && yRot == 270 && zRot == 90)
			{
				finalZ = 90;
			}
			else if (xRot == 90 && yRot == 180 && zRot == 0)
			{
				finalZ = 180;
			}
			else if (xRot == 0 && yRot == 90 && zRot == 270)
			{
				finalZ = 270;
			}

			bw.Write(finalZ);
		}

		public static void WriteTileSite(string id, NetBuffer bw)
		{

			bw.Write(IdTranslator.Instance.GetIdCode<TileData>(id));
		}

		// READ

		private static WorldChunk[] chunkArray;
		private static Color[] pixels;

		private static GameObject tilemapParent;
		private static GameObject edgeObjectsParent;
		private static GameObject fpObjectsParent;
		private static GameObject tileObjectsParent;

		private static Dictionary<int, GameObject> ChunkDict = new Dictionary<int, GameObject>();

		public static void ImportSite(
			string filePath,
			int startWorldX,
			int startWorldY
		)
		{
			ChunkDict = ME_Control.ChunkDict;

			if (tilemapParent == null)
			{
				tilemapParent = new GameObject();
				tilemapParent.name = "tile_map";
			}

			if (edgeObjectsParent == null)
			{
				edgeObjectsParent = new GameObject();
				edgeObjectsParent.name = "edge_objects";
			}

			if (fpObjectsParent == null)
			{
				fpObjectsParent = new GameObject();
				fpObjectsParent.name = "float_point_objects";
			}

			if (tileObjectsParent == null)
			{
				tileObjectsParent = new GameObject();
				tileObjectsParent.name = "tile_objects";
			}

			IdTranslator.Instance.Clear();

			NetBuffer nb = new NetBuffer();
			using (var fs = new FileStream(filePath, FileMode.Open))
			{
				using (var br = new BinaryReader(fs))
				{
					nb.Data = br.ReadBytes((int)br.BaseStream.Length);
				}
			}

			//NetBuffer nb = new NetBuffer();

			//nb.Data = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));

			var idTransl = new IdTranslator();
			idTransl.Deserialize(nb);

			// site width in tiles
			var widthInTiles = nb.ReadInt16();
			// site height in tiles
			var heightInTiles = nb.ReadInt16();

			for (int x = 0; x < widthInTiles; x++)
			{
				for (int y = 0; y < heightInTiles; y++)
				{
					bool isNullTile = nb.ReadBoolean();
					if (!isNullTile)
					{
						ReadTileSite(x + startWorldX, y + startWorldY, nb, idTransl);
						ReadTileEdgeObjectsSite(x + startWorldX, y + startWorldY, nb, idTransl);
						ReadTileObjectSite(x + startWorldX, y + startWorldY, nb, idTransl);
					}
				}
			}

			ME_Control.ChunkDict = ChunkDict;
			ME_Control.RedrawTileUVs();
		}

		private static void ReadTileSite(int worldX, int worldY, NetBuffer br, IdTranslator idTransl)
		{

			string id = idTransl.GetId<TileData>(br.ReadUInt16());

			ushort chunkX = (ushort)(worldX / 32);
			ushort chunkY = (ushort)(worldY / 32);

			GameObject go;

			if (!ChunkDict.ContainsKey(GetChunkHash(chunkX, chunkY)))
			{
				go = GameObject.Instantiate(
					BlueprintBank.LoadBlueprintPrefab(BlueprintBank.BlueprintType.TileChunk)
				);

				go.transform.position = new Vector3(chunkX * 32, chunkY * 32, 0);

				go.transform.parent = tilemapParent.transform;

				go.GetComponent<ME_Chunk>().Init(chunkX, chunkY);

				ChunkDict.Add(GetChunkHash(chunkX, chunkY), go);

				Debug.LogWarning("Generate chunk in " + GetChunkHash(chunkX, chunkY));
			}
			else
			{
				go = ChunkDict[GetChunkHash(chunkX, chunkY)];
			}

			if (go == null)
				Debug.LogWarning("No chunk at " + GetChunkHash(chunkX, chunkY));

			ME_Chunk chunk = go.GetComponent<ME_Chunk>();

			go.GetComponent<ME_Chunk>().PaintTile(id, worldX % 32, worldY % 32);

			chunk.TileIds[worldX % 32, worldY % 32] = id;
		}

		private static void ReadTileEdgeObjectsSite(int tileWorldX, int tileWorldY, NetBuffer br, IdTranslator idTransl)
		{
			bool hasWestEdge = br.ReadBoolean();
			if (hasWestEdge)
			{
				CreateEdgeObject(
					idTransl.GetId<FNEEntityData>(br.ReadUInt16()),
					tileWorldX,
					tileWorldY + 0.5f,
					br.ReadInt16()
				);
			}

			bool hasSouthEdge = br.ReadBoolean();
			if (hasSouthEdge)
			{
				CreateEdgeObject(
					idTransl.GetId<FNEEntityData>(br.ReadUInt16()),
					tileWorldX + 0.5f,
					tileWorldY,
					br.ReadInt16()
				);
			}

			bool hasEastEdge = br.ReadBoolean();
			if (hasEastEdge)
			{
				CreateEdgeObject(
					idTransl.GetId<FNEEntityData>(br.ReadUInt16()),
					tileWorldX + 1,
					tileWorldY + 0.5f,
					br.ReadInt16()
				);
			}

			bool hasNorthEdge = br.ReadBoolean();
			if (hasNorthEdge)
			{
				CreateEdgeObject(
					idTransl.GetId<FNEEntityData>(br.ReadUInt16()),
					tileWorldX + 0.5f,
					tileWorldY + 1,
					br.ReadInt16()
				);
			}
		}

		private static GameObject CreateEdgeObject(string id, float worldX, float worldY, float rotation)
		{
			GameObject go = PrefabBank.GetInstanceOfFNEEntityPrefab(id);
			go.transform.position = new Vector3(worldX, worldY, 0);

			// This extremely dirty rotation shit is due to quick fix world rotation in
			// level editor
			switch (rotation)
			{
				case 0:
					go.transform.rotation = Quaternion.Euler(270, 0, 0);
					break;

				case 90:
					go.transform.rotation = Quaternion.Euler(0, 270, 90);
					break;

				case 180:
					go.transform.rotation = Quaternion.Euler(90, 180, 0);
					break;

				case 270:
					go.transform.rotation = Quaternion.Euler(0, 90, 270);
					break;
			}

			// go.transform.rotation = Quaternion.Euler(0, 0, rotation);

			var entityData = DataBank.Instance.GetData<FNEEntityData>(id);
			var viewData = DataBank.Instance.GetData<FNEEntityViewData>(entityData.entityViewVariations[0]);
			go.transform.localScale = Vector3.one * viewData.scaleMod;

			go.transform.parent = edgeObjectsParent.transform;
			go.tag = "EdgeObject";

			LevelEditorUtils.AddEdgeObjectEditorHitbox(go);

			return go;
		}

		private static void ReadTileObjectSite(int tileWorldX, int tileWorldY, NetBuffer br, IdTranslator idTransl)
		{
			bool hasTileObject = br.ReadBoolean();
			if (!hasTileObject)
				return;

			string id = idTransl.GetId<FNEEntityData>(br.ReadUInt16());
			ushort rotation = br.ReadUInt16();

			GameObject go = PrefabBank.GetInstanceOfFNEEntityPrefab(id);
			go.transform.position = new Vector3(tileWorldX + 0.5f, tileWorldY + 0.5f, 0);

			// This extremely dirty rotation shit is due to quick fix world rotation in
			// level editor
			switch (rotation)
			{
				case 0:
					go.transform.rotation = Quaternion.Euler(270, 0, 0);
					break;

				case 90:
					go.transform.rotation = Quaternion.Euler(0, 270, 90);
					break;

				case 180:
					go.transform.rotation = Quaternion.Euler(90, 180, 0);
					break;

				case 270:
					go.transform.rotation = Quaternion.Euler(0, 90, 270);
					break;
			}

			//go.transform.rotation = Quaternion.Euler(0, 0, rotation);

			var entityData = DataBank.Instance.GetData<FNEEntityData>(id);
			var viewData = DataBank.Instance.GetData<FNEEntityViewData>(entityData.entityViewVariations[0]);
			go.transform.localScale = Vector3.one * viewData.scaleMod;

			//go.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
			go.transform.parent = tileObjectsParent.transform;
			go.tag = "TileObject";

			LevelEditorUtils.AddTileObjectEditorHitbox(go);
		}

		public static  int2 GetFileSiteSizeInTiles(string filePath)
		{
			NetBuffer nb = new NetBuffer();
			using (var fs = new FileStream(filePath, FileMode.Open))
			{
				using (var br = new BinaryReader(fs))
				{
					nb.Data = br.ReadBytes((int)br.BaseStream.Length);
				}
			}

			IdTranslator.Instance.Deserialize(nb);

			// site width in tiles
			var width = ME_Control.LevelEditorMain.widthInTiles = nb.ReadInt16();
			// site height in tiles
			var height = ME_Control.LevelEditorMain.heightInTiles = nb.ReadInt16();

			return new int2(width, height);
		}

		private static int GetChunkHash(ushort x, ushort y)
		{
			return 523 * x + 541 * y;
		}
	}
}