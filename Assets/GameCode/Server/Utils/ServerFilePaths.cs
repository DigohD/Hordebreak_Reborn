using System;
using System.Collections.Generic;
using FNZ.Shared.Model.World;
using System.IO;
using System.Text.RegularExpressions;
using Unity.Mathematics;

namespace FNZ.Server.Utils
{
	public class ServerFilePaths
	{
		private readonly string m_WorldName;

		public ServerFilePaths(string worldName)
		{
			m_WorldName = worldName;
		}

		public bool WorldFolderExists()
		{
			return Directory.Exists($"Saves\\{m_WorldName}\\chunks");
		}

		public string GetChunkFilePath(WorldChunk chunk)
		{
			string folderPath = $"Saves\\{m_WorldName}\\chunks\\world_chunk_{chunk.ChunkX}_{chunk.ChunkY}";
			return $"{folderPath}\\chunk_data.fnd";
		}

		public List<string> GetAllSavedWorlds()
		{
			var result = new List<string>();

			if (!Directory.Exists("Saves"))
				return result;

			foreach (var dir in Directory.GetDirectories("Saves"))
			{
				result.Add(dir);
			}
			
			return result;
		}

		public List<Tuple<int2, string>> GetAllChunkPaths()
		{
			List<Tuple<int2, string>> chunkPaths = new List<Tuple<int2, string>>();
			if (!Directory.Exists($"Saves\\{m_WorldName}\\chunks"))
				return chunkPaths;
					
			foreach (var dir in Directory.GetDirectories($"Saves\\{m_WorldName}\\chunks"))
			{
				var parts = dir.Split('\\');
				var chunkFolderName = parts[parts.Length - 1];

				byte x = 0;
				byte y = 0;
				
				int iteration = 0;
				var regexp = new Regex("([0-9]+)");

				var matches = regexp.Matches(chunkFolderName);
				
				foreach (Match match in matches)
				{
					var num = byte.Parse(match.Value);

					if (iteration == 0)
						x = num;
					else
						y = num;
					
					iteration++;
				}

				chunkPaths.Add(new Tuple<int2, string>(new int2(x, y), GetChunkFilePath(x, y)));
			}

			return chunkPaths;
		}
		
		public string GetChunkFilePath(byte chunkX, byte chunkY)
		{
			string folderPath = $"Saves\\{m_WorldName}\\chunks\\world_chunk_{chunkX}_{chunkY}";
			return $"{folderPath}\\chunk_data.fnd";
		}

		public string GetOrCreateChunkFilePath(WorldChunk chunk)
		{
			string folderPath = $"Saves\\{m_WorldName}\\chunks\\world_chunk_{chunk.ChunkX}_{chunk.ChunkY}";

			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
			}

			return $"{folderPath}\\chunk_data.fnd";
		}

		public string GetOrCreateChunkSiteMetaFilePath()
		{
			string folderPath = $"Saves\\{m_WorldName}\\siteMeta";

			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
			}

			return $"{folderPath}\\chunk_site_meta_data.fnsmd";
		}

		public string GetOrCreateBaseFilePath()
		{
			string folderPath = $"Saves\\{m_WorldName}\\base";

			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
			}

			return $"{folderPath}\\base_meta_data.fnd";
		}
		
		public string GetOrCreateQuestsFilePath()
		{
			string folderPath = $"Saves\\{m_WorldName}\\quest";

			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
			}

			return $"{folderPath}\\quests_meta_data.fnd";
		}
		
		public string GetQuestsFilePath()
		{
			string folderPath = $"Saves\\{m_WorldName}\\quest";
			return $"{folderPath}\\quests_meta_data.fnd";
		}
		
		public string GetBaseFilePath()
		{
			string folderPath = $"Saves\\{m_WorldName}\\base";
			return $"{folderPath}\\base_meta_data.fnd";
		}

		public string CreatePlayerEntityFilePath(string playerName)
		{
			string folderPath = $"Saves\\{m_WorldName}\\players\\{playerName}";

			if (!Directory.Exists(folderPath))
				Directory.CreateDirectory(folderPath);

			return $"{folderPath}\\FNZ.Shared.Model.Entity.FNEEntity";
		}

		public string GetSavedEntityPathFromName(string playerName)
		{
			if (File.Exists($"Saves\\{m_WorldName}\\players\\{playerName}\\FNZ.Shared.Model.Entity.FNEEntity"))
				return $"Saves\\{m_WorldName}\\players\\{playerName}\\FNZ.Shared.Model.Entity.FNEEntity";
			else
				return string.Empty;
		}

		public string GetSavedPlayersPath()
		{
			string path = $"Saves\\{m_WorldName}\\players";
			if (Directory.Exists(path))
				return path;

			return string.Empty;
		}
	}
}