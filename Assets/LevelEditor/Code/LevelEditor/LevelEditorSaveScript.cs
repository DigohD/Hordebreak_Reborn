using Assets.LevelEditor.Code.LevelEditor;
using FNZ.Client.View.Prefab;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.World.Tile;
using FNZ.Shared.Utils;
using Lidgren.Network;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace FNZ.LevelEditor
{
	public class LevelEditorSaveScript
	{
		List<Vector2> savedTileObjPositions;
		List<Vector2> savedEdgeObjPositions;

		private struct ChunkData
		{
			public GameObject chunk;

			public List<GameObject> edgeObjects;
			public List<GameObject> FloatPointObjects;
			public List<GameObject> TileObjects;
		}

		void Start()
		{

		}

		/*public void SaveEditorScene(string levelName, string folderPath, int widthInTiles, int heightInTiles, int chunkSizeInTiles)
		{
			Debug.LogWarning("SAVE SCENE: " + levelName);

			savedTileObjPositions = new List<Vector2>();
			savedEdgeObjPositions = new List<Vector2>();

			//BinaryWriter bw;

			//try
			//{
			//    bw = new BinaryWriter(new FileStream("Data\\Maps\\" + levelName, FileMode.Create));
			//}
			//catch (IOException e)
			//{
			//    Console.WriteLine(e.Message + "\n Cannot create file.");
			//    return;
			//}

			Debug.Log("Width, height:" + widthInTiles + "|" + heightInTiles);

			IdTranslator.Instance.Clear();
			//IdTranslator.Instance.GenerateIds();

			ChunkData[,] chunkdata = new ChunkData[widthInTiles, heightInTiles];

			GameObject[] chunks = ME_Control.ChunkDict.Values.ToArray();

			foreach (GameObject go in chunks)
			{
				ME_Chunk MEChunk = go.GetComponent<ME_Chunk>();

				int chunkX = ((int)MEChunk.transform.position.x / 32);
				int chunkY = ((int)MEChunk.transform.position.y / 32);

				chunkdata[chunkX, chunkY] = new ChunkData();
				chunkdata[chunkX, chunkY].chunk = go;
				chunkdata[chunkX, chunkY].edgeObjects = new List<GameObject>();
				chunkdata[chunkX, chunkY].FloatPointObjects = new List<GameObject>();
				chunkdata[chunkX, chunkY].TileObjects = new List<GameObject>();
			}

			GameObject[] edgeObjects = GameObject.FindGameObjectsWithTag("EdgeObject");

			foreach (GameObject go in edgeObjects)
			{
				foreach (var pos in savedEdgeObjPositions)
				{
					if (pos.x == go.transform.position.x && pos.y == go.transform.position.y)
					{
						Debug.LogWarning("Found multiple TileObjects at: " + pos.x + ":" + pos.y);
						continue;
					}
				}

				IdTranslator.Instance.GenerateIdIfNeeded<FNEEntityData>(go.GetComponentInChildren<GameObjectIdHolder>().id);

				savedEdgeObjPositions.Add(new Vector2(go.transform.position.x, go.transform.position.y));

				chunkdata[(int)(go.transform.position.x / 32), (int)(go.transform.position.y / 32)].edgeObjects.Add(go);
			}

			GameObject[] floatpointObjects = GameObject.FindGameObjectsWithTag("FloatpointObject");

			foreach (GameObject go in floatpointObjects)
			{
				IdTranslator.Instance.GenerateIdIfNeeded<FNEEntityData>(go.GetComponentInChildren<GameObjectIdHolder>().id);

				chunkdata[(int)(go.transform.position.x / 32), (int)(go.transform.position.y / 32)].FloatPointObjects.Add(go);
			}

			GameObject[] tileObjects = GameObject.FindGameObjectsWithTag("TileObject");

			foreach (GameObject go in tileObjects)
			{
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

				chunkdata[(int)(go.transform.position.x / 32), (int)(go.transform.position.y / 32)].TileObjects.Add(go);
			}

			foreach (ChunkData cd in chunkdata)
			{
				ME_Chunk MEChunk = cd.chunk.GetComponent<ME_Chunk>();
				for (int i = 0; i < 32; i++)
				{
					for (int j = 0; j < 32; j++)
					{
						IdTranslator.Instance.GenerateIdIfNeeded<TileData>(MEChunk.TileIds[i, j]);
					}
				}
			}


			// -------------------- START WRITING ---------------------
			NetBuffer nb = new NetBuffer();

			IdTranslator.Instance.Serialize(nb);

			// terrain width in chunks
			nb.Write((ushort)(widthInTiles));
			// terrain height in chunks
			nb.Write((ushort)(heightInTiles));
			// number of tiles per chunk
			nb.Write((byte)32);

			//write chunks
			foreach (ChunkData cd in chunkdata)
			{
				nb.Write((ushort)(cd.chunk.transform.position.x / 32));
				nb.Write((ushort)(cd.chunk.transform.position.y / 32));

				nb.Write((ushort)(cd.edgeObjects.Count + cd.FloatPointObjects.Count + cd.TileObjects.Count + 32 * 32));

				WriteChunk(cd.chunk, nb);

				foreach (GameObject go in cd.edgeObjects)
				{
					WriteEdgeObject(go, cd.chunk, nb);
				}

				foreach (GameObject go in cd.FloatPointObjects)
				{
					WriteFloatpointObject(go, cd.chunk, nb);
				}

				foreach (GameObject go in cd.TileObjects)
				{
					WriteTileObject(go, cd.chunk, nb);
				}
			}

			// Actually write to file
			using (var fs = new FileStream(folderPath + levelName + LevelEditorUtils.EDITOR_FILE_ENDiNG, FileMode.Create))
			{
				using (var bw = new BinaryWriter(fs))
				{
					bw.Write(nb.Data, 0, nb.LengthBytes);
				}
			}

			Debug.Log("Save done!");
		}*/

		public void ExportSite(
			string levelName, 
			string folderPath, 
			int widthInTiles, 
			int heightInTiles,
			int originWorldX,
			int originWorldY
		)
		{
			FNSV1.SaveFile(
				levelName,
				folderPath,
				widthInTiles,
				heightInTiles,
				originWorldX,
				originWorldY
			);
		}

	}
}
