using UnityEngine;

namespace FNZ.LevelEditor
{
	public class TileBank
	{
		public enum TileType
		{
			ROAD,
			SIDEWALK,
			FLOOR1,
			FLOOR2,
			FLOOR3,
			FLOOR4,
			BORDER,
			GRASS
		}

		public static Material GetTileMaterial(TileType tileType)
		{
			string rootFolder = "TileMaterials/";

			switch (tileType)
			{
				case TileType.ROAD:
					return Resources.Load(rootFolder + "M_Road", typeof(Material)) as Material;
				case TileType.SIDEWALK:
					return Resources.Load(rootFolder + "M_Sidewalk", typeof(Material)) as Material;
				case TileType.FLOOR1:
					return Resources.Load(rootFolder + "M_Floor_1", typeof(Material)) as Material;
				case TileType.FLOOR2:
					return Resources.Load(rootFolder + "M_Floor_2", typeof(Material)) as Material;
				case TileType.FLOOR3:
					return Resources.Load(rootFolder + "M_Floor_3", typeof(Material)) as Material;
				case TileType.FLOOR4:
					return Resources.Load(rootFolder + "M_Floor_4", typeof(Material)) as Material;
				case TileType.BORDER:
					return Resources.Load(rootFolder + "M_Border", typeof(Material)) as Material;
				case TileType.GRASS:
					return Resources.Load(rootFolder + "M_Grass", typeof(Material)) as Material;
			}

			throw new System.Exception("NO SUCH MATERIAL BOIIIII!");
		}
	}
}