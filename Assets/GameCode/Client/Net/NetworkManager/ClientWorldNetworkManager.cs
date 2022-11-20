using FNZ.Client.Model.World;
using FNZ.Client.Systems;
using FNZ.Client.View.UI.Manager;
using FNZ.Client.View.World;
using FNZ.Shared.Model;
using FNZ.Shared.Model.World.Tile;
using FNZ.Shared.Net;
using FNZ.Shared.Utils;
using Lidgren.Network;
using System.Collections.Generic;
using System.Threading;
using FNZ.Shared.Model.World;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace FNZ.Client.Net.NetworkManager
{
	public delegate void NewHour(byte hour);
	public delegate void OnSiteMapUpdate(Dictionary<int, MapManager.RevealedSiteData> siteMap);

	public class ClientWorldNetworkManager : INetworkManager
	{
		private List<GameObject> arrows = new List<GameObject>();
		private readonly ClientChunkWorker m_ChunkWorkerThread;

		private struct FFNode
		{
			public Vector2 worldTiles;
			public float totalcost;
		}

		private struct VectorFieldNode
		{
			public Vector2 vector;
		}

		public ClientWorldNetworkManager()
		{
			GameClient.NetConnector.Register(NetMessageType.WORLD_SETUP, OnWorldSetupMessageReceived);
			GameClient.NetConnector.Register(NetMessageType.LOAD_CHUNK, OnLoadChunkMessageReceived);
			GameClient.NetConnector.Register(NetMessageType.UNLOAD_CHUNK, OnUnloadChunkMessageReceived);
			GameClient.NetConnector.Register(NetMessageType.CHANGE_TILE, OnChangeTileMessageReceived);
			GameClient.NetConnector.Register(NetMessageType.ROOM_MANAGER, OnRoomManagerMessageReceived);
			GameClient.NetConnector.Register(NetMessageType.ENVIRONMENT, OnEnvironmentMessageReceived);
			GameClient.NetConnector.Register(NetMessageType.CHUNK_MAP_UPDATE, OnChunkMapUpdateReceived);

			//DEV ONLY!!!
			GameClient.NetConnector.Register(NetMessageType.DEVONLY_FLOWFIELD_DEVONLY, OnFlowFieldMessageReceived);
			//DEV ONLY!!!

			m_ChunkWorkerThread = new ClientChunkWorker();
		}

		public static NewHour d_NewHour;
		public static OnSiteMapUpdate d_OnSiteMapUpdate;
		
		private void OnWorldSetupMessageReceived(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			IdTranslator.Instance.Deserialize(incMsg);

			GameClient.World.WIDTH = incMsg.ReadInt32();
			GameClient.World.HEIGHT = incMsg.ReadInt32();
			GameClient.World.CHUNK_SIZE = incMsg.ReadByte();

			GameClient.World.InitializeWorld<ClientWorldChunk>();

			GameClient.NetAPI.CMD_World_RequestWorldSpawn();
		}

		private void OnLoadChunkMessageReceived(ClientNetworkConnector netConnector, NetIncomingMessage incMsg)
		{
			Profiler.BeginSample("OnLoadChunkMessage - Deserialize");
			var chunkX = incMsg.ReadByte();
			var chunkY = incMsg.ReadByte();
			var sizeInBytes = incMsg.ReadInt32();
			var data = FNEUtil.Decompress(incMsg.ReadBytes(sizeInBytes));
			Profiler.EndSample();

			lock (m_ChunkWorkerThread.Lock)
			{
				m_ChunkWorkerThread.QueueChunk(new ChunkGenData 
				{
					ChunkX = chunkX,
					ChunkY = chunkY,
					Data = data
				});

				Monitor.Pulse(m_ChunkWorkerThread.Lock);
				m_ChunkWorkerThread.DoWork = true;
			}
		}

		private void OnUnloadChunkMessageReceived(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			var chunkX = incMsg.ReadByte();
			var chunkY = incMsg.ReadByte();

			// var enemyCount = incMsg.ReadUInt16();
			//
			// if (enemyCount > 0)
			// {
			// 	var entitiesToDestroy = new NativeArray<Entity>(enemyCount, Allocator.Temp);
			//
			// 	for (var i = 0; i < enemyCount; i++)
			// 	{
			// 		var netId = incMsg.ReadInt32();
			// 		var enemy = net.GetEntity(netId);
			// 		if (enemy == null) 
			// 		{
			// 			Debug.LogError($"[CLIENT] entity with netId: {netId} already unsynced");
			// 			continue; 
			// 		}
			// 		var view = GameClient.ViewConnector.GetEntity(netId);
			// 		entitiesToDestroy[i] = view;
			// 		
			// 		GameClient.World.RemoveEnemyFromTile(enemy);
			// 		GameClient.ViewConnector.RemoveView(netId);
			//
			// 		GameClient.NetConnector.UnsyncEntity(enemy);
			// 	}
			//
			// 	GameClient.ECS_ClientWorld.EntityManager.DestroyEntity(entitiesToDestroy);
			// 	entitiesToDestroy.Dispose();
			// }

			var chunk = GameClient.World.GetWorldChunk<ClientWorldChunk>();
			chunk.ClearChunk();
			GameClient.World.SetChunk<ClientWorldChunk>(null);
			GameClient.WorldView.RemoveChunkView();
			GameClient.NetAPI.CMD_World_ConfirmChunkUnloaded(chunk);
		}

		private void OnChangeTileMessageReceived(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			int tileX = incMsg.ReadInt32(); 
			int tileY = incMsg.ReadInt32();
			string id = IdTranslator.Instance.GetId<TileData>(incMsg.ReadUInt16());

			var chunk = GameClient.World.GetWorldChunk<ClientWorldChunk>();

			var CHUNKI_SIZE = GameClient.World.CHUNK_SIZE;
			int index = ((int)tileX - chunk.ChunkX * CHUNKI_SIZE) + (tileY - chunk.ChunkY * CHUNKI_SIZE) * CHUNKI_SIZE;
			chunk.TileIdCodes[index] = IdTranslator.Instance.GetIdCode<TileData>(id);
			chunk.BlockingTiles[index] = DataBank.Instance.GetData<TileData>(id).isBlocking;
			chunk.DelegateInvokeRerender();
		}

		private void OnRoomManagerMessageReceived(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			GameClient.RoomManager.Deserialize(incMsg);
			UIManager.Instance.ForceUpdatePlayerRoomAndEnvironment();
		}

		private void OnEnvironmentMessageReceived(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			var previousHour = GameClient.Environment.Hour;
			GameClient.Environment.Deserialize(incMsg);

			if (previousHour != GameClient.Environment.Hour)
				d_NewHour?.Invoke(GameClient.Environment.Hour);

		}

		private void OnFlowFieldMessageReceived(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			foreach (var go in arrows)
			{
				GameObject.Destroy(go);
			}

			var gridSize = incMsg.ReadInt32();
			var FFNodeMatrix = new FFNode[gridSize, gridSize];

			for (int i = 0; i < gridSize; i++)
			{
				for (int j = 0; j < gridSize; j++)
				{
					var node = new FFNode();

					node.worldTiles.x = incMsg.ReadInt32();
					node.worldTiles.y = incMsg.ReadInt32();
					node.totalcost = incMsg.ReadFloat();

					FFNodeMatrix[i, j] = node;
				}
			}

			var vectorArraySize = incMsg.ReadInt32();
			var vectorFieldNodeMatrix = new VectorFieldNode[vectorArraySize, vectorArraySize];

			for (int i = 0; i < vectorArraySize; i++)
			{
				for (int j = 0; j < vectorArraySize; j++)
				{
					var vfn = new VectorFieldNode();

					vfn.vector.x = incMsg.ReadFloat();
					vfn.vector.y = incMsg.ReadFloat();

					vectorFieldNodeMatrix[i, j] = vfn;
				}
			}
			
			for (int i = 0; i < vectorArraySize; i++)
			{
				for (int j = 0; j < vectorArraySize; j++)
				{
					var xValue = 0;
					var nodeVector = vectorFieldNodeMatrix[i, j].vector;

					var angle = (math.atan2(nodeVector.x, nodeVector.y) / math.PI) * 180;
					angle *= -1;

					if (nodeVector.x == 0 && nodeVector.y == 0)
						xValue = -90;

					var go = GameObject.Instantiate(Resources.Load<GameObject>("_DEV/ArrowLol"),
						new Vector3(FFNodeMatrix[i, j].worldTiles.x + 0.5f, FFNodeMatrix[i, j].worldTiles.y + 0.5f, -0.5f),
						Quaternion.Euler(0,0,0));

					go.GetComponentInChildren<MeshRenderer>().transform.parent.localRotation = Quaternion.Euler(xValue, 0, angle);
					go.GetComponentInChildren<TextMeshPro>().text = $"{FFNodeMatrix[i, j].totalcost}";					

					arrows.Add(go);
				}
			}
		}

		private void OnChunkMapUpdateReceived(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			var chunkX = incMsg.ReadByte();
			var chunkY = incMsg.ReadByte();

			var chunkSize = GameClient.World.CHUNK_SIZE;

			ushort[] tileIdCodes = new ushort[chunkSize * chunkSize];
			
			for (int i = 0; i < tileIdCodes.Length; i++)
			{
				tileIdCodes[i] = incMsg.ReadUInt16();
			}

			GameClient.World.WorldMap.HandleRevealedChunk(chunkX, chunkY, tileIdCodes);
		}
	}
}