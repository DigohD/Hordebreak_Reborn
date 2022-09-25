using Assets.LevelEditor.Code.LevelEditorV2;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.MountedObject;
using FNZ.Shared.Model.World.Tile;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FNZ.LevelEditor
{
	public class LevelEditorUI : MonoBehaviour
	{
		public GameObject P_TileCategory;
		public Transform T_TileContent;

		public GameObject P_WallCategory;
		public Transform T_WallContent;

		public GameObject P_WallObjectsCategory;
		public Transform T_WallObjectsContent;

		public GameObject P_TileObjectCategory;
		public Transform T_TileObjectContent;

		public GameObject P_FPObjectCategory;
		public Transform T_FPObjectContent;

		void Start()
		{
			GenerateTileCategories();
			GenerateWallCategories();
			GenerateWallObjectsCategories();
			GenerateTileObjectCategories();
			GenerateFPObjectCategories();
		}

		private void GenerateTileCategories()
		{
			var tiles = DataBank.Instance.GetAllDataIdsOfType<TileData>();

			var newCategory = Instantiate(P_TileCategory);
			newCategory.GetComponent<TileSheetUI>().GenerateButtons(tiles);
			newCategory.transform.SetParent(T_TileContent);
		}

		private void GenerateWallCategories()
		{
			Dictionary<string, List<FNEEntityData>> wallDict = new Dictionary<string, List<FNEEntityData>>();

			var allWalls = DataBank.Instance.GetAllDataIdsOfType<FNEEntityData>().Where(entity =>
				entity.entityType.Equals("EdgeObject")
			);

			foreach (var wall in allWalls)
			{
				var cat = wall.editorCategoryName;
				if (!wallDict.ContainsKey(cat))
				{
					wallDict.Add(cat, new List<FNEEntityData>());
				}
				wallDict[cat].Add(wall);
			}

			foreach (var cat in wallDict.Keys)
			{
				var newCategory = Instantiate(P_WallCategory);

				newCategory.GetComponent<WallsUI>().GenerateList(wallDict[cat], cat);

				newCategory.transform.SetParent(T_WallContent);
			}
		}

		private void GenerateWallObjectsCategories()
		{
			Dictionary<string, List<MountedObjectData>> wallObjectsDict = new Dictionary<string, List<MountedObjectData>>();

			var allWallObjects = DataBank.Instance.GetAllDataIdsOfType<MountedObjectData>();

			foreach (var wallObject in allWallObjects)
			{
				var cat = wallObject.editorCategoryName;
				if (cat == null)
					cat = "Uncategorized";

				if (!wallObjectsDict.ContainsKey(cat))
				{
					wallObjectsDict.Add(cat, new List<MountedObjectData>());
				}
				wallObjectsDict[cat].Add(wallObject);
			}

			foreach (var cat in wallObjectsDict.Keys)
			{
				var newCategory = Instantiate(P_WallObjectsCategory);

				newCategory.GetComponent<WallObjectsUI>().GenerateList(wallObjectsDict[cat], cat);

				newCategory.transform.SetParent(T_WallObjectsContent);
			}
		}

		private void GenerateTileObjectCategories()
		{
			Dictionary<string, List<FNEEntityData>> TODict = new Dictionary<string, List<FNEEntityData>>();

			var allTOs = DataBank.Instance.GetAllDataIdsOfType<FNEEntityData>().Where(entity =>
				entity.entityType.Equals("TileObject")
			);

			foreach (var TO in allTOs)
			{
				var cat = TO.editorCategoryName;
				if (!TODict.ContainsKey(cat))
				{
					TODict.Add(cat, new List<FNEEntityData>());
				}
				TODict[cat].Add(TO);
			}

			foreach (var cat in TODict.Keys)
			{
				var newCategory = Instantiate(P_TileObjectCategory);

				newCategory.GetComponent<TileObjectsUI>().GenerateList(TODict[cat], cat);

				newCategory.transform.SetParent(T_TileObjectContent);
			}
		}

		private void GenerateFPObjectCategories()
		{
			Dictionary<string, List<FNEEntityData>> FPDict = new Dictionary<string, List<FNEEntityData>>();

			var allFPs = DataBank.Instance.GetAllDataIdsOfType<FNEEntityData>().Where(entity =>
				entity.entityType.Equals("FloatpointObject")
			);

			foreach (var FP in allFPs)
			{
				var cat = FP.editorCategoryName;
				if (!FPDict.ContainsKey(cat))
				{
					FPDict.Add(cat, new List<FNEEntityData>());
				}
				FPDict[cat].Add(FP);
			}

			foreach (var cat in FPDict.Keys)
			{
				var newCategory = Instantiate(P_FPObjectCategory);

				newCategory.GetComponent<FPObjectsUI>().GenerateList(FPDict[cat], cat);

				newCategory.transform.SetParent(T_FPObjectContent);
			}
		}


	}
}