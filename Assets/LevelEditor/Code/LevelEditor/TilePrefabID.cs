using UnityEngine;

namespace FNZ.LevelEditor
{
	public class TilePrefabID : MonoBehaviour
	{
		public TileBank.TileType TileType;
		[HideInInspector]
		public byte ID;

		public bool IsBlocking;
		public bool CanSeeThrough;

		public void OnValidate()
		{
			GetComponent<MeshRenderer>().material =
				TileBank.GetTileMaterial(TileType);
			ID = (byte)TileType;
		}

		public void ME_PaintTile(TileBank.TileType newType)
		{
			TileType = newType;
			OnValidate();
		}
	}
}