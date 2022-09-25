using UnityEngine;
namespace FNZ.LevelEditor
{
	public class ME_CopyChunkHandler : MonoBehaviour
	{
		private short startX, startY, width, height, targetX, targetY;

		public GameObject G_Popup;

		public GameObject P_TileChunk;

		public void OnEXECUTE()
		{
			if (targetX >= startX)
			{
				short tX = (short)(targetX + width - 1);
				if (targetY >= startY)
					for (short i = (short)(startX + width - 1); i >= startX; i--)
					{
						short tY = (short)(targetY + height - 1);
						for (short j = (short)(startY + height - 1); j >= startY; j--)
						{
							MoveChunk(i, j, tX, tY);
							tY--;
						}
						tX--;
					}
				if (targetY < startY)
					for (short i = (short)(startX + width - 1); i >= startX; i--)
					{
						short tY = targetY;
						for (short j = startY; j < startY + height; j++)
						{
							MoveChunk(i, j, tX, tY);
							tY++;
						}
						tX--;
					}
			}

			if (targetX < startX)
			{
				short tX = targetX;
				if (targetY >= startY)
					for (short i = startX; i < startX + width; i++)
					{
						short tY = (short)(targetY + height - 1);
						for (short j = (short)(startY + height - 1); j >= startY; j--)
						{
							MoveChunk(i, j, tX, tY);
							tY--;
						}
						tX++;
					}
				if (targetY < startY)
					for (short i = startX; i < startX + width; i++)
					{
						short tY = targetY;
						for (short j = startY; j < startY + height; j++)
						{
							MoveChunk(i, j, tX, tY);
							tY++;
						}
						tX++;
					}
			}
		}

		private void MoveChunk(short sourceX, short sourceY, short targetX, short targetY)
		{
			Debug.LogWarning("Moving " + sourceX + ", " + sourceY + " to " + targetX + ", " + targetY);

			if (targetX == sourceX && targetY == sourceY)
				return;


			ME_Chunk toMove = null;

			if (ME_Control.ChunkDict.ContainsKey(ME_Control.GetChunkHash(sourceX, sourceY)))
			{
				toMove = ME_Control.ChunkDict[ME_Control.GetChunkHash(sourceX, sourceY)].GetComponent<ME_Chunk>();
			}
			else
			{
				Debug.LogError("No chunk exists at coords: " + sourceX + ", " + sourceY);
				return;
			}

			ME_Chunk toReplace = null;

			if (ME_Control.ChunkDict.ContainsKey(ME_Control.GetChunkHash(targetX, targetY)))
			{
				toReplace = ME_Control.ChunkDict[ME_Control.GetChunkHash(targetX, targetY)].GetComponent<ME_Chunk>();
			}
			else
			{
				while (1 + (targetX) > ME_Control.LevelEditorMain.widthInTiles)
					ME_Control.LevelEditorMain.widthInTiles += 1;

				while (1 + (targetY) > ME_Control.LevelEditorMain.heightInTiles)
					ME_Control.LevelEditorMain.heightInTiles += 1;

				for (short i = 0; i < targetX + 1; i++)
				{
					for (short j = 0; j < ME_Control.LevelEditorMain.heightInTiles; j++)
					{
						if (!ME_Control.ChunkDict.ContainsKey(ME_Control.GetChunkHash(i, j)))
						{
							GameObject newChunk = GameObject.Instantiate(
								P_TileChunk
							);
							newChunk.transform.position = new Vector2(i * 32, j * 32);

							ME_Control.ChunkDict.Add(ME_Control.GetChunkHash(i, j), newChunk);

							newChunk.GetComponent<ME_Chunk>().PaintTile("default_tile", 0, 0);
						}
					}
				}

				for (short i = 0; i < targetY + 1; i++)
				{
					for (short j = 0; j < ME_Control.LevelEditorMain.widthInTiles; j++)
					{
						if (!ME_Control.ChunkDict.ContainsKey(ME_Control.GetChunkHash(j, i)))
						{
							GameObject newChunk = GameObject.Instantiate(
								P_TileChunk
							);
							newChunk.transform.position = new Vector2(j * 32, i * 32);

							ME_Control.ChunkDict.Add(ME_Control.GetChunkHash(j, i), newChunk);

							newChunk.GetComponent<ME_Chunk>().PaintTile("default_tile", 0, 0);
						}
					}
				}

				toReplace = ME_Control.ChunkDict[ME_Control.GetChunkHash(targetX, targetY)].GetComponent<ME_Chunk>();
			}

			GameObject[] edgeObjects = GameObject.FindGameObjectsWithTag("EdgeObject");

			foreach (GameObject go in edgeObjects)
			{
				if ((int)go.transform.position.x / 32 == sourceX && (int)go.transform.position.y / 32 == sourceY)
					go.transform.SetParent(toMove.transform);
				else if ((int)go.transform.position.x / 32 == targetX && (int)go.transform.position.y / 32 == targetY)
					go.transform.SetParent(toReplace.transform);
			}

			GameObject[] floatpointObjects = GameObject.FindGameObjectsWithTag("FloatpointObject");

			foreach (GameObject go in floatpointObjects)
			{
				if ((int)go.transform.position.x / 32 == sourceX && (int)go.transform.position.y / 32 == sourceY)
					go.transform.SetParent(toMove.transform);
				else if ((int)go.transform.position.x / 32 == targetX && (int)go.transform.position.y / 32 == targetY)
					go.transform.SetParent(toReplace.transform);
			}

			GameObject[] tileObjects = GameObject.FindGameObjectsWithTag("TileObject");

			foreach (GameObject go in tileObjects)
			{
				if ((int)go.transform.position.x / 32 == sourceX && (int)go.transform.position.y / 32 == sourceY)
					go.transform.SetParent(toMove.transform);
				else if ((int)go.transform.position.x / 32 == targetX && (int)go.transform.position.y / 32 == targetY)
					go.transform.SetParent(toReplace.transform);
			}

			toMove.transform.position = new Vector2(targetX * 32, targetY * 32);
			toReplace.transform.position = new Vector2(sourceX * 32, sourceY * 32);

			ME_Control.ChunkDict.Remove(ME_Control.GetChunkHash(targetX, targetY));
			ME_Control.ChunkDict.Add(ME_Control.GetChunkHash(targetX, targetY), toMove.gameObject);

			ME_Control.ChunkDict.Remove(ME_Control.GetChunkHash(sourceX, sourceY));
			ME_Control.ChunkDict.Add(ME_Control.GetChunkHash(sourceX, sourceY), toReplace.gameObject);

			foreach (Transform t in toMove.transform)
			{
				t.SetParent(null);
			}

			foreach (Transform t in toReplace.transform)
			{
				Destroy(t.gameObject);
				toReplace.WipeChunk();
			}

			if (toMove != null) toMove.GenerateFloorUVs();
			if (toReplace != null) toReplace.GenerateFloorUVs();
		}

		public void TogglePopup()
		{
			G_Popup.SetActive(!G_Popup.activeInHierarchy);
		}

		public void OnStartXChange(string newX)
		{
			this.startX = short.Parse(newX);
		}

		public void OnStartYChange(string newY)
		{
			this.startY = short.Parse(newY);
		}

		public void OnWidthChange(string newWidth)
		{
			this.width = short.Parse(newWidth);
		}

		public void OnHeightChange(string newHeight)
		{
			this.height = short.Parse(newHeight);
		}

		public void OnTargetXChange(string newX)
		{
			this.targetX = short.Parse(newX);
		}

		public void OnTargetYChange(string newY)
		{
			this.targetY = short.Parse(newY);
		}
	}
}