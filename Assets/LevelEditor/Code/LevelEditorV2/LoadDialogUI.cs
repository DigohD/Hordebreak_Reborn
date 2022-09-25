using Assets.LevelEditor.Code.LevelEditor;
using FNZ.LevelEditor;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.LevelEditor.Code.LevelEditorV2
{
	public class LoadDialogUI : MonoBehaviour
	{
		public enum FileMode
		{
			EDITOR = 0,
			SITE = 1
		}

		private string m_CreationName = "";
		private string m_FolderPath = Application.streamingAssetsPath + "\\Your_Editor_Saves\\";

		public GameObject P_File;
		public GameObject P_Folder;

		public Transform T_FileList;
		public LevelEditorApplication m_LevelEditorApplication;

		public Text TXT_FolderPath;

		public FileMode ActiveFileMode;

		public void OpenUIEditorMode()
		{
			ActiveFileMode = FileMode.EDITOR;
			RerenderContent();
		}

		public void OpenUISiteMode()
		{
			ActiveFileMode = FileMode.SITE;
			RerenderContent();
		}

		public void RerenderContent()
		{
			Debug.LogWarning("GO: " + m_FolderPath);
            if (!Directory.Exists(m_FolderPath))
            {
				Directory.CreateDirectory(m_FolderPath);
            }
			var directories = Directory.GetDirectories(m_FolderPath);
			var files = Directory.GetFiles(m_FolderPath);

			TXT_FolderPath.text = m_FolderPath;

			foreach (Transform t in T_FileList)
			{
				Destroy(t.gameObject);
			}

			if (!m_FolderPath.Equals(Application.streamingAssetsPath + "\\Your_Editor_Saves\\"))
			{
				var newButton = Instantiate(P_Folder);
				string parentDir = m_FolderPath.Substring(0, m_FolderPath.LastIndexOf("\\"));
				parentDir = parentDir.Substring(0, parentDir.LastIndexOf("\\") + 1);

				newButton.GetComponentInChildren<Text>().text = "./";
				newButton.GetComponent<Button>().onClick.AddListener(() =>
				{
					m_FolderPath = parentDir;
					RerenderContent();
				}
				);

				newButton.transform.SetParent(T_FileList);
			}

			foreach (var dir in directories)
			{
				var newButton = Instantiate(P_Folder);
				string dirName = dir.Substring(dir.LastIndexOf("\\") + 1);

				newButton.GetComponentInChildren<Text>().text = dirName;
				newButton.GetComponent<Button>().onClick.AddListener(() =>
				{
					m_FolderPath = m_FolderPath + dirName + "\\";
					RerenderContent();
				}
				);

				newButton.transform.SetParent(T_FileList);
			}

			foreach (var file in files)
			{
				if (ActiveFileMode == FileMode.EDITOR && !file.Contains(LevelEditorUtils.EDITOR_FILE_ENDiNG))
					continue;

				else if (ActiveFileMode == FileMode.SITE && !file.Contains(LevelEditorUtils.SITE_FILE_ENDING))
					continue;

				var newButton = Instantiate(P_File);
				string fileName = file.Substring(file.LastIndexOf("\\") + 1);

				if (fileName.Contains(".meta"))
					continue;

				newButton.GetComponentInChildren<Text>().text = fileName;
				newButton.GetComponent<Button>().onClick.AddListener(() =>
				{
					m_CreationName = fileName;
					HighlightFile();
				});

				newButton.transform.SetParent(T_FileList);
			}
		}

		public void HighlightFile()
		{
			foreach (Transform t in T_FileList)
			{
				Text textComp = t.GetComponentInChildren<Text>();
				if (textComp.text.ToLower().Equals(m_CreationName.ToLower()))
				{
					textComp.color = new Color(0.4f, 0.65f, 0.4f);
				}
				else
				{
					textComp.color = new Color(1f, 1f, 1f);
				}
			}
		}

		public void OnLoadClick()
		{
			if (!File.Exists(m_FolderPath + m_CreationName))
			{
				return;
			}

			if (ActiveFileMode == FileMode.EDITOR)
			{
				m_LevelEditorApplication.LoadLevel(m_CreationName, m_FolderPath);
			}
			else
			{
				m_LevelEditorApplication.StartImportSite(m_CreationName, m_FolderPath);
			}
			
			gameObject.SetActive(false);
		}
	}
}