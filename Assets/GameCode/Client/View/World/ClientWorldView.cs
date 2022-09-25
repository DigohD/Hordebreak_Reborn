using FNZ.Client.Model.World;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Client.View.World
{
	public class ClientWorldView
	{
		private ClientWorld m_WorldModel;
		private List<ClientWorldChunkView> m_WorldChunkViews;

		public ClientWorldView(ClientWorld worldModel)
		{
			m_WorldModel = worldModel;
			m_WorldChunkViews = new List<ClientWorldChunkView>();
		}

		public void AddChunkView(ClientWorldChunkView chunkView)
		{
			m_WorldChunkViews.Add(chunkView);
		}

		public void RemoveChunkView(byte chunkX, byte chunkY)
		{
			m_WorldChunkViews.RemoveAll(chunkView => chunkX == chunkView.ChunkX && chunkY == chunkView.ChunkY);
		}

		public ClientWorldChunkView GetChunkView(float2 position)
		{
			int cx = m_WorldModel.GetChunkIndices(new float2((int)position.x, (int)position.y)).x;
			int cy = m_WorldModel.GetChunkIndices(new float2((int)position.x, (int)position.y)).y;

			foreach (var chunkView in m_WorldChunkViews)
			{
				if (chunkView == null) continue;
				if (cx == chunkView.ChunkX && cy == chunkView.ChunkY)
					return chunkView;
			}

			return null;
		}
	}
}