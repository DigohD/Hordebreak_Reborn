using FNZ.Shared.Utils;
using System.IO;
using UnityEngine;
using static FNZ.Shared.Utils.Localization;

namespace FNZ.Client.View.UI.Utils
{

	public class ClientSettings
	{
		public static void SaveSettings()
		{
			string path = Application.persistentDataPath + "/settings.txt";

			using (StreamWriter sw = File.CreateText(path))
			{
				sw.WriteLine("Language=" + (byte)Localization.GetActiveLanguage());
				sw.Close();
			}
		}

		public static void LoadSettings()
		{
			string path = Application.persistentDataPath + "/settings.txt";

			try
			{
				StreamReader reader = new StreamReader(path, true);
				string lan = reader.ReadLine();

				SetActiveLanguage((Language)byte.Parse(lan.Split('=')[1]));
				reader.Close();
			}
			catch (FileNotFoundException e)
			{
				Debug.Log(e);
			}
		}
	}
}