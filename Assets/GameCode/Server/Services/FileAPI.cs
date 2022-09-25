using System.Collections.Generic;
using System.IO;
using FNZ.Server.Model.World;
using FNZ.Shared.Utils;
using Lidgren.Network;
using UnityEngine;
using static FNZ.Server.Model.World.Blueprint.WorldBlueprintGen;

namespace FNZ.Server.Services
{
	public class FileAPI
	{
		public void SaveWorldChunkToFile(ServerWorldChunk chunk)
		{
			var netBuffer = new NetBuffer();
			var predictedBufferSize = chunk.TotalBitsFileBuffer();
			netBuffer.EnsureBufferSize(predictedBufferSize);
			//Debug.Log($"[SERVER, SaveWorldChunkToFile]: ChunkX: {chunk.ChunkX} , ChunkY: {chunk.ChunkY}");
			chunk.FileSerialize(netBuffer);
			var actualBufferSizeAfterSerialization = netBuffer.LengthBits;
			if (actualBufferSizeAfterSerialization > predictedBufferSize)
            {
				Debug.Log($"The BufferSize after serialization of chunk is: {actualBufferSizeAfterSerialization} bits which is larger then the predicted size of: {predictedBufferSize} bits.");
            }

			FileUtils.WriteFile(GameServer.FilePaths.GetOrCreateChunkFilePath(chunk), netBuffer.Data);
		}

		public void WriteFile(string path, byte[] data)
		{
			FileUtils.WriteFile(path, data);
		}

		public void LoadWorldChunkFromFile(string filePath, ServerWorldChunk chunk)
		{
			var netBuffer = new NetBuffer
			{
				Data = FileUtils.ReadFile(filePath)
			};

			chunk.FileDeserialize(netBuffer);
		}

		public Dictionary<int, SiteMetaData> LoadSiteMetaDataFromFile(string filePath)
		{
			var netBuffer = new NetBuffer
			{
				Data = FileUtils.ReadFile(filePath)
			};

			int count = netBuffer.ReadInt32();
			Dictionary<int, SiteMetaData> siteMetaData = new Dictionary<int, SiteMetaData>();
			for (int i = 0; i < count; i++)
            {
				var metaData = new SiteMetaData();
				metaData.Deserialize(netBuffer);
				siteMetaData[metaData.chunkX + metaData.chunkY * GameServer.World.WIDTH_IN_CHUNKS] = metaData;
			}

			return siteMetaData;
		}

		public ServerRoomManager LoadRoomManagerFromFile(string filePath)
		{
			if (!File.Exists(filePath))
			{
				return null;
			}
			
			var netBuffer = new NetBuffer
			{
				Data = FileUtils.ReadFile(filePath)
			};

			var result = new ServerRoomManager();
			result.FileDeserialize(netBuffer);
			return result;
		}
	}
}