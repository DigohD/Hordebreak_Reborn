using FNZ.Shared.Model.Entity;
using FNZ.Shared.Net.Dto.Hordes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FNZ.Server.Model.World
{
	public class PlayerChunkState
	{
		public readonly object Lock = new object();

		public List<ServerWorldChunk> CurrentlyLoadedChunks = new List<ServerWorldChunk>();
		public List<ServerWorldChunk> ChunksAwaitingLoad = new List<ServerWorldChunk>();
		public List<Tuple<ServerWorldChunk, long>> ChunksAwaitingUnload = new List<Tuple<ServerWorldChunk, long>>();
		public List<ServerWorldChunk> ChunksSentForLoadAwaitingConfirm = new List<ServerWorldChunk>();
		public List<ServerWorldChunk> ChunksSentForUnloadAwaitingConfirm = new List<ServerWorldChunk>();
		public List<HordeEntitySpawnData> EntitiesToLoad = new List<HordeEntitySpawnData>();
		public List<HordeEntityDestroyData> EntitiesToUnload = new List<HordeEntityDestroyData>();
		public List<int> MovingEntitiesSynced = new List<int>();

		public bool IsChunkInLoadedState(ServerWorldChunk chunk)
		{
			var b1 = CurrentlyLoadedChunks.Contains(chunk);
			var b2 = ChunksSentForLoadAwaitingConfirm.Contains(chunk);
			var b3 = ChunksAwaitingUnload.FindAll(t => t.Item1 == chunk).Count == 1;
			return b1 || b2 || b3;
		}
	}
}

