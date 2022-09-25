using UnityEngine;

namespace FNZ.LevelEditor
{
	public class BlueprintBank
	{
		public enum BlueprintType
		{
			Tile,
			TileChunk
		}

		public static GameObject LoadBlueprintPrefab(BlueprintType type)
		{
			switch (type)
			{
				//case BlueprintType.Wall:
				//return (GameObject)Resources.Load("Blueprints/EdgeObject_BP");
				case BlueprintType.Tile:
					return (GameObject)Resources.Load("Blueprints/Tile_BP");
				case BlueprintType.TileChunk:
					return (GameObject)Resources.Load("EditorChunk");
			}

			throw new System.Exception("Prefab of type " + type + " does not exist in LoadBlueprintPrefab!");
		}
	}
}