using System.Collections.Generic;
using FNZ.Server.FarNorthZMigrationStuff;
using FNZ.Server.Model.World;
using FNZ.Shared.Model.World;
using FNZ.Shared.Utils;
using Lidgren.Network;
using Unity.Mathematics;

namespace FNZ.Server.Net.API
{
	public partial class ServerNetworkAPI
	{
		/// <summary>
		///		Net API module: World
		///		NetMessage Type: WorldSetup
		///		Send method: STC (Send To Client)
		///		
		///		Sends the total world width and height in tiles and the chunksize in tiles to the given client connection.
		///		
		/// </summary>
		/// <param name="widthInTiles"> Total world width in tiles</param>
		/// <param name="heightInTiles">Total world height in tiles</param>
		/// <param name="chunkSize">Chunk size in tiles. E.g: chunkSize = 32 means one chunk is 32x32 tiles</param>
		/// <param name="clientConnection">The connection to the client</param>
		public void World_WorldSetup_STC(int widthInTiles, int heightInTiles, byte chunkSize, NetConnection clientConnection)
		{
			GameServer.ChunkManager.AddClientToChunkStreamingSystem(clientConnection, new PlayerChunkState());
			var message = m_WorldMessageFactory.CreateWorldSetupMessage(widthInTiles, heightInTiles, chunkSize);
			SendToClient(message, clientConnection);
		}

		public void World_LoadChunk_STC(WorldChunk chunk, byte[] data, NetConnection clientConnection)
		{
			var message = m_WorldMessageFactory.CreateLoadChunkMessage(chunk.ChunkX, chunk.ChunkY, FNEUtil.Compress(data));
			SendToClient(message, clientConnection);
		}

		public void World_UnloadChunk_STC(WorldChunk chunk, NetConnection clientConnection)
		{
			var message = m_WorldMessageFactory.CreateUnloadChunkMessage(chunk.ChunkX, chunk.ChunkY);
			SendToClient(message, clientConnection);
		}

		public void World_ChangeTile_BAR(int x, int y, string id)
		{
			var message = m_WorldMessageFactory.CreateChangeTileMessage(x, y, id);
			Broadcast_All_Relevant(message, new float2(x, y));
		}

		public void World_RoomManager_BA()
		{
			var message = m_WorldMessageFactory.CreateRoomManagerMessage();
			Broadcast_All(message);
		}
		
		public void World_RoomManager_STC(NetConnection clientConnection)
		{
			var message = m_WorldMessageFactory.CreateRoomManagerMessage();
			SendToClient(message, clientConnection);
		}

		public void World_Environment_BA()
		{
			var message = m_WorldMessageFactory.CreateEnvironmentMessage();
			Broadcast_All(message);
		}

		public void World_Environment_STC(NetConnection clientConnection)
		{
			var message = m_WorldMessageFactory.CreateEnvironmentMessage();
			SendToClient(message, clientConnection);
		}
		
		public void World_ChunkMapUpdate_STC(NetConnection clientConnection, ServerWorldChunk chunk)
		{
			var message = m_WorldMessageFactory.CreateChunkMapUpdateMessage(chunk);
			SendToClient(message, clientConnection);
		}
		
		public void World_ChunkMapUpdate_BA(ServerWorldChunk chunk)
		{
			var message = m_WorldMessageFactory.CreateChunkMapUpdateMessage(chunk);
			Broadcast_All(message);
		}
		
		public void World_SiteMapUpdate_STC(NetConnection clientConnection, Dictionary<int, MapManager.RevealedSiteData> siteMap)
		{
			var message = m_WorldMessageFactory.CreateSiteMapUpdateMessage(siteMap);
			SendToClient(message, clientConnection);
		}
		
		public void World_SiteMapUpdate_BA(Dictionary<int, MapManager.RevealedSiteData> siteMap)
		{
			var message = m_WorldMessageFactory.CreateSiteMapUpdateMessage(siteMap);
			Broadcast_All(message);
		}

		//DEV ONLY!!!
		public void World_SendFlowField_STC(FNEFlowField ff, NetConnection clientConnection)
		{
			var message = m_WorldMessageFactory.CreateFlowFieldMessage(ff);
			SendToClient(message, clientConnection);
		}
		//DEV ONLY!!!
	}
}