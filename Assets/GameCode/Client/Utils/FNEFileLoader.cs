using System;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;

namespace FNZ.Client.Utils
{
	public static class FNEFileLoader
	{
		public static Texture2D TryLoadImage(string path, Texture2D tempTexture)
		{
			if (!string.IsNullOrEmpty(path))
			{
				try
				{
					Profiler.BeginSample("ReadAllBytes");
					var data = File.ReadAllBytes($"{Application.streamingAssetsPath}/{path}");
					Profiler.EndSample();
					
					Profiler.BeginSample("LoadImage");
					tempTexture.LoadImage(data);
					Profiler.EndSample();
				}
				catch (FileNotFoundException e)
				{
					Debug.LogError($"Error: File not found. {e}");
					return null;
				}
				catch (DirectoryNotFoundException e)
				{
					Debug.LogError($"Error: Directory not found. {e}");
					return null;
				}
				catch (UnauthorizedAccessException e)
				{
					Debug.LogError($"Error: Unauthorized access. {e}");
					return null;
				}
			}
			else
				return null;

			return tempTexture;
		}
	}
}