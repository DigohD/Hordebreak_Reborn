using Assets.LevelEditor.Code.LevelEditor;
using Assets.LevelEditor.Code.LevelEditorV2.FNSFile;
using FNZ.Client.View.Prefab;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.EntityViewData;
using FNZ.Shared.Model.World;
using FNZ.Shared.Model.World.Tile;
using FNZ.Shared.Utils;
using Lidgren.Network;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.LevelEditor
{
	public class LevelEditorLoadScript
	{
		private WorldChunk[] chunkArray;
		private Color[] pixels;

		private GameObject tilemapParent;
		private GameObject edgeObjectsParent;
		private GameObject fpObjectsParent;
		private GameObject tileObjectsParent;

		private Dictionary<int, GameObject> ChunkDict = new Dictionary<int, GameObject>();

		private void Start()
		{

		}

		public void GenerateNewLevel(Texture2D tileMap, int chunkSize)
		{
			ClearLevel();
		}

		public void ClearLevel()
		{
			foreach (GameObject go in ME_Control.ChunkDict.Values)
			{
				GameObject.Destroy(go);
			}

			ME_Control.ChunkDict.Clear();

			ME_Control.LevelEditorMain.widthInTiles = 0;
			ME_Control.LevelEditorMain.heightInTiles = 0;

			if (tilemapParent != null)
				GameObject.Destroy(tilemapParent);

			if (edgeObjectsParent != null)
				GameObject.Destroy(edgeObjectsParent);

			if (fpObjectsParent != null)
				GameObject.Destroy(fpObjectsParent);

			if (tileObjectsParent != null)
				GameObject.Destroy(tileObjectsParent);
		}

		public void LoadLevel(string levelName, string folderPath, int width, int height)
		{
			ClearLevel();

			//BinaryReader br;

			tilemapParent = new GameObject();
			tilemapParent.name = "tile_map";

			edgeObjectsParent = new GameObject();
			edgeObjectsParent.name = "edge_objects";

			fpObjectsParent = new GameObject();
			fpObjectsParent.name = "float_point_objects";

			tileObjectsParent = new GameObject();
			tileObjectsParent.name = "tile_objects";

			//try
			//{
			//    br = new BinaryReader(new FileStream("Data\\Maps\\" + levelName, FileMode.Open));
			//}
			//catch (IOException e)
			//{
			//    Console.WriteLine(e.Message + "\n Cannot create file.");
			//    return;
			//}


			IdTranslator.Instance.Clear();

			NetBuffer nb = new NetBuffer();
			using (var fs = new FileStream(folderPath + levelName, FileMode.Open))
			{
				using (var br = new BinaryReader(fs))
				{
					nb.Data = br.ReadBytes((int)br.BaseStream.Length);
				}
			}

			//NetBuffer nb = new NetBuffer();

			//nb.Data = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));

			IdTranslator.Instance.Deserialize(nb);

			// terrain width in tiles
			ME_Control.LevelEditorMain.widthInTiles = nb.ReadInt16();
			// terrain height in tiles
			ME_Control.LevelEditorMain.heightInTiles = nb.ReadInt16();
			// number of tiles per chunk
			ME_Control.LevelEditorMain.chunkSizeInTiles = nb.ReadByte();

			while (nb.Position != nb.LengthBits)
			{
				ushort chunkX = nb.ReadUInt16();
				ushort chunkY = nb.ReadUInt16();

				ushort objectCount = nb.ReadUInt16();

				for (int i = 0; i < objectCount; i++)
				{
					byte type = nb.ReadByte();

					switch (type)
					{
						case 1:
							ReadTile(chunkX, chunkY, nb);
							break;
						case 2:
							ReadEdgeObject(chunkX, chunkY, nb);
							break;
						case 3:
							ReadFloatpointObject(chunkX, chunkY, nb);
							break;
						case 4:
							ReadTileObject(chunkX, chunkY, nb);
							break;
					}
				}
			}

			ME_Control.ChunkDict = ChunkDict;
			ME_Control.RedrawTileUVs();
		}

		private void ReadTile(ushort chunkX, ushort chunkY, NetBuffer br)
		{
			string id = IdTranslator.Instance.GetId<TileData>(br.ReadUInt16());
			byte dangerLevel = br.ReadByte();
			bool blocking = br.ReadBoolean();

			ushort x = br.ReadUInt16();
			ushort y = br.ReadUInt16();

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

			go.GetComponent<ME_Chunk>().SetDangerLevel(dangerLevel, x, y);
			go.GetComponent<ME_Chunk>().SetBlocking(blocking, (short)x, (short)y);

			chunk.TileIds[x, y] = id;
		}

		private void ReadEdgeObject(ushort chunkX, ushort chunkY, NetBuffer br)
		{
			string id = IdTranslator.Instance.GetId<FNEEntityData>(br.ReadUInt16());
			short rotation = br.ReadInt16();

			float x = FNEUtil.UnpackShortToFloat(br.ReadUInt16());
			float y = FNEUtil.UnpackShortToFloat(br.ReadUInt16());

			GameObject go = PrefabBank.GetInstanceOfFNEEntityPrefab(id);
			go.transform.position = new Vector3(x + (chunkX * 32), y + (chunkY * 32), 0);

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

			go.transform.parent = edgeObjectsParent.transform;
			go.tag = "EdgeObject";

			LevelEditorUtils.AddEdgeObjectEditorHitbox(go);
		}

		private void ReadFloatpointObject(ushort chunkX, ushort chunkY, NetBuffer br)
		{
			FNETransform transform;

			transform.posX = FNEUtil.UnpackShortToFloat(br.ReadUInt16());
			transform.posY = FNEUtil.UnpackShortToFloat(br.ReadUInt16());
			transform.posZ = FNEUtil.ConvertSignedShortToFloat(br.ReadInt16());

			transform.rotX = FNEUtil.UnpackShortToFloat(br.ReadUInt16());
			transform.rotY = FNEUtil.UnpackShortToFloat(br.ReadUInt16());
			transform.rotZ = FNEUtil.UnpackShortToFloat(br.ReadUInt16());

			transform.scaleX = FNEUtil.UnpackShortToFloat(br.ReadUInt16());
			transform.scaleY = FNEUtil.UnpackShortToFloat(br.ReadUInt16());
			transform.scaleZ = FNEUtil.UnpackShortToFloat(br.ReadUInt16());

			transform.entityId = IdTranslator.Instance.GetId<FNEEntityData>(br.ReadUInt16());

			GameObject go = PrefabBank.GetInstanceOfFNEEntityPrefab(transform.entityId);

			go.transform.position = new Vector3(transform.posX + chunkX * 32, transform.posY + chunkY * 32, transform.posZ);
			go.transform.rotation = Quaternion.Euler(transform.rotX, transform.rotY, transform.rotZ);
			go.transform.localScale = new Vector3(transform.scaleX, transform.scaleY, transform.scaleZ);

			go.transform.parent = fpObjectsParent.transform;
		}

		private void ReadTileObject(ushort chunkX, ushort chunkY, NetBuffer br)
		{
			string id = IdTranslator.Instance.GetId<FNEEntityData>(br.ReadUInt16());

			ushort x = br.ReadUInt16();
			ushort y = br.ReadUInt16();
			
			float rotation = FNEUtil.UnpackShortToFloat(br.ReadUInt16());

			float scaleX = FNEUtil.UnpackShortToFloat(br.ReadUInt16());
			float scaleY = FNEUtil.UnpackShortToFloat(br.ReadUInt16());
			float scaleZ = FNEUtil.UnpackShortToFloat(br.ReadUInt16());

			GameObject go = PrefabBank.GetInstanceOfFNEEntityPrefab(id);
			go.transform.position = new Vector3(x + 0.5f + chunkX * 32, y + 0.5f + chunkY * 32, 0);

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

			//go.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
			go.transform.parent = tileObjectsParent.transform;
			go.tag = "TileObject";

			LevelEditorUtils.AddTileObjectEditorHitbox(go);
		}

		private int GetChunkHash(ushort x, ushort y)
		{
			return 523 * x + 541 * y;
		}

		public void ImportSite(
			string filePath,
			int startWorldX, 
			int startWorldY
		)
		{
			NetBuffer nb = new NetBuffer();
			using (var fs = new FileStream(filePath, FileMode.Open))
			{
				using (var br = new BinaryReader(fs))
				{
					nb.Data = br.ReadBytes((int)br.BaseStream.Length);
				}
			}

			var code = nb.ReadInt32();

            switch (code)
            {
				case (int) FileUtils.FNS_File_Version_Code.FNS_FILE_VERSION_1:
					FNSV1.ImportSite(
						filePath,
						startWorldX,
						startWorldY
					);
					break;

				default:
					FNSLegacy.ImportSite(
						filePath,
						startWorldX,
						startWorldY
					);
					break;
            }
		}

		public int GetFileVersionCode(string filePath)
		{
			NetBuffer nb = new NetBuffer();
			using (var fs = new FileStream(filePath, FileMode.Open))
			{
				using (var br = new BinaryReader(fs))
				{
					nb.Data = br.ReadBytes((int)br.BaseStream.Length);
				}
			}

			return nb.ReadInt32();
		}

		public int2 GetFileSiteSizeInTiles(string filePath)
		{
			NetBuffer nb = new NetBuffer();
			using (var fs = new FileStream(filePath, FileMode.Open))
			{
				using (var br = new BinaryReader(fs))
				{
					nb.Data = br.ReadBytes((int)br.BaseStream.Length);
				}
			}

			var code = nb.ReadInt32();

			IdTranslator.Instance.Deserialize(nb);

			// site width in tiles
			var width = ME_Control.LevelEditorMain.widthInTiles = nb.ReadInt16();
			// site height in tiles
			var height = ME_Control.LevelEditorMain.heightInTiles = nb.ReadInt16();

			return new int2(width, height);
		}

		public int2 GetFileSiteSizeInTiles_Legacy(string filePath)
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
	}
}