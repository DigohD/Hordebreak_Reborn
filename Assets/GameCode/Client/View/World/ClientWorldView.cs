using FNZ.Client.Model.World;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Client.View.World
{
	public class ClientWorldView
	{
		private ClientWorld m_WorldModel;
		private ClientWorldChunkView m_WorldChunkView;

		public ClientWorldView(ClientWorld worldModel)
		{
			m_WorldModel = worldModel;
		}

		public void AddChunkView(ClientWorldChunkView chunkView)
		{
			m_WorldChunkView = chunkView;
		}

		public void RemoveChunkView()
		{
			m_WorldChunkView = null;       
		}

		public ClientWorldChunkView GetChunkView(float2 position)
		{
		return m_WorldChunkView;
		}
	}
}