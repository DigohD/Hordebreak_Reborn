using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;

namespace FNZ.Editor
{
	public class InsertNamespace : UnityEditor.AssetModificationProcessor
	{
		public static void OnWillCreateAsset(string path)
		{
			string assetPath = Regex.Replace(path, @".meta$", string.Empty);
			if (!assetPath.EndsWith(".cs")) return;

			var code = File.ReadAllLines(assetPath).ToList();
			if (code.Any(line => line.Contains("namespace"))) return;//already added by IDE

			//insert namespace
			int idx = code.FindIndex(line => line
				.Contains("class " + Path.GetFileNameWithoutExtension(assetPath)));
			code.Insert(idx, Regex.Replace(
			assetPath.Replace('/', '.'), @"^([\w+.]+)\.\w+\.cs$", "namespace $1 \n{\n"));
			code.Add("}");

			//correct indentation
			for (int i = idx + 1; i < code.Count - 1; i++)
				code[i] = '\t' + code[i];

			int index = 0;

			foreach (var line in code)
			{
				if (line.Contains("namespace"))
				{
					break;
				}

				index++;
			}

			var newCode = code.ToArray();
			newCode[index] = newCode[index].Replace("Assets.GameCode", "FNZ");

			var finalCode = string.Join("\n", newCode);

			File.WriteAllText(assetPath, finalCode);

			AssetDatabase.Refresh();
		}
	}
}

