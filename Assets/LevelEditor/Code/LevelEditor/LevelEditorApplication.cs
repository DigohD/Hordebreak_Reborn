using FNZ.Shared.Model;
using FNZ.Shared.Model.World.Tile;
using FNZ.Shared.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace FNZ.LevelEditor
{
	public class LevelEditorApplication : MonoBehaviour
	{
		public Texture2D tilemap;
		public int chunkSizeInTiles;

		public int widthInTiles;
		public int heightInTiles;

		private LevelEditorLoadScript levelLoader;
		private LevelEditorSaveScript levelSaver;

		private float saveTimer;

		public static Dictionary<string, Material> chunkMaterials = new Dictionary<string, Material>();

		public ME_Control EditorControl;

		void Start()
		{
			levelLoader = new LevelEditorLoadScript();
			levelSaver = new LevelEditorSaveScript();

			widthInTiles = 0;
			heightInTiles = 0;

			ME_Control.LevelEditorMain = this;

			InitChunkMaterials();
		}

		private void InitChunkMaterials()
		{
			foreach (TileSheetData tsd in DataBank.Instance.GetAllDataIdsOfType<TileSheetData>())
			{
				Material mat = new Material(Shader.Find("Standard"));
				mat.mainTexture = Resources.Load<Texture2D>(tsd.tileSheetPath);

				mat.SetFloat("_Metallic", 0);
				mat.SetFloat("_Glossiness", 0);

				chunkMaterials.Add(tsd.Id, mat);
			}
		}

		public void GenerateNewLevel()
		{
			levelLoader.GenerateNewLevel(tilemap, chunkSizeInTiles);
		}

		public void SaveEditorScene(string sceneName, string folderPath)
		{
			//levelSaver.SaveEditorScene(sceneName, folderPath, widthInTiles, heightInTiles, chunkSizeInTiles);
		}

		public void ExportSite(string siteName, string folderPath)
		{
			int siteWidth = ME_Control.ExportEndX - ME_Control.ExportStartX;
			int siteHeight = ME_Control.ExportEndY - ME_Control.ExportStartY;

			if (siteWidth == 0 || siteHeight == 0)
				return;

			levelSaver.ExportSite(
				siteName, 
				folderPath, 
				siteWidth, 
				siteHeight,
				ME_Control.ExportStartX,
				ME_Control.ExportStartY
			);
		}

		public void LoadLevel(string levelName, string folderPath)
		{
			levelLoader.LoadLevel(levelName, folderPath, widthInTiles, heightInTiles);
		}

		public void StartImportSite(string siteName, string folderPath)
		{
			var code = levelLoader.GetFileVersionCode(folderPath + siteName);

            switch (code)
            {
				case (int) FileUtils.FNS_File_Version_Code.FNS_FILE_VERSION_1:
					var siteSize = levelLoader.GetFileSiteSizeInTiles(folderPath + siteName);
					EditorControl.StartImportSite(folderPath + siteName, siteSize.x, siteSize.y);
					break;

				default:
					siteSize = levelLoader.GetFileSiteSizeInTiles_Legacy(folderPath + siteName);
					EditorControl.StartImportSite(folderPath + siteName, siteSize.x, siteSize.y);
					break;
            }
		}

		public void ExecuteSiteImport(string filePath, int startX, int startY)
		{
			levelLoader.ImportSite(
				filePath,
				startX,
				startY
			);
		}

		void Update()
		{
			saveTimer += Time.deltaTime;

			/*if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
			{
				Debug.Log("Save done!");
				levelSaver.SaveEditorScene("quicksave", "Data\\Maps\\", widthInTiles, heightInTiles, chunkSizeInTiles);
			}*/

			//if (saveTimer > 5f)
			//{
			//    Debug.Log("Auto save done!");
			//    levelSaver.SaveLevel("testmap_AutoSave", widthInTiles, heightInTiles, chunkSizeInTiles);
			//    saveTimer = 0;
			//}

			if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.N))
			{
				GenerateNewLevel();
			}
		}
	}
}