using FNZ.Server.FarNorthZMigrationStuff;
using FNZ.Server.Model.Entity.Components.AI;
using FNZ.Server.Model.Entity.Components.EdgeObject;
using FNZ.Server.Model.Entity.Components.Name;
using FNZ.Server.Model.Entity.Components.Polygon;
using FNZ.Server.Model.Entity.Components.TileObject;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components;
using FNZ.Shared.Utils;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using FNZ.Server.Model.Entity.Components.EquipmentSystem;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Server.Model
{
	public class ServerEntityFactory
	{
		private Dictionary<Type, Type> m_CompDataToCompDictionary = new Dictionary<Type, Type>();

		//private FNEEntityPool m_EntityPool;

		private Stack<Tuple<FNEEntity, string>> ReplacementStack = new Stack<Tuple<FNEEntity, string>>();

		public ServerEntityFactory()
		{
			//m_EntityPool = new FNEEntityPool(1000);
			BuildTable();
		}

		public FNEEntity CreateEntity(string entityId, float2 position, int worldInstanceIndex, float rotation = 0, bool enabled = true)
		{
			var entity = new FNEEntity();

			entity.Init(entityId, position, rotation, enabled);
			entity.WorldInstanceIndex = worldInstanceIndex;

			var componentsFromXML = DataBank.Instance.GetData<FNEEntityData>(entity.EntityId).components;

			foreach (var compData in componentsFromXML)
			{
				entity.AddComponent(m_CompDataToCompDictionary[compData.GetComponentType()], compData);
			}

			foreach (var comp in entity.Components)
			{
				comp.InitComponentLinks();
			}

			switch (entity.EntityType)
			{
				case EntityType.TILE_OBJECT:
					InitTileObject(entity);
					break;

				case EntityType.EDGE_OBJECT:
					InitEdgeObject(entity);
					break;

				case EntityType.ECS_ENEMY:
				case EntityType.GO_ENEMY:
					InitEnemy(entity);
					break;
			}

			return entity;
		}

		public void RecycleEntity(FNEEntity entity)
        {
			//m_EntityPool.ReturnEntity(entity);
		}

		private void InitTileObject(FNEEntity entity)
		{
			var position = entity.Position;
			var tileObjectComp = entity.AddComponent<TileObjectComponentServer>();
			tileObjectComp.cost = entity.Data.pathingCost;
			tileObjectComp.seeThrough = entity.Data.seeThrough;
			tileObjectComp.hittable = entity.Data.hittable;

			entity.Scale = new float3(1, 1, 1);

			if (entity.Data.blocking)
			{
				var polyComp = entity.AddComponent<PolygonComponentServer>();

				if (entity.Data.smallCollisionBox)
				{
					polyComp.relativePoints.Add(new Vector2(-0.25f, -0.25f));
					polyComp.relativePoints.Add(new Vector2(-0.25f, 0.25f));
					polyComp.relativePoints.Add(new Vector2(0.25f, 0.25f));
					polyComp.relativePoints.Add(new Vector2(0.25f, -0.25f));
					polyComp.offsetToMiddle = new Vector2(0.5f, 0.5f);
				}
				else
				{
					polyComp.relativePoints.Add(new Vector2(-0.5f, -0.5f));
					polyComp.relativePoints.Add(new Vector2(-0.5f, 0.5f));
					polyComp.relativePoints.Add(new Vector2(0.5f, 0.5f));
					polyComp.relativePoints.Add(new Vector2(0.5f, -0.5f));
					polyComp.offsetToMiddle = new Vector2(0.5f, 0.5f);

					var obs = entity.AddComponent<ObstacleComponentServer>();
					obs.x = position.x;
					obs.y = position.y;
					obs.width = 1.0f;
					obs.height = 1.0f;
					obs.InitObstacle();
				}
			}
		}

		private void InitEdgeObject(FNEEntity entity)
		{
			entity.Scale = new float3(1, 1, 1);
			var rotation = entity.RotationDegrees;
			var position = entity.Position;

			var eoc = entity.AddComponent<EdgeObjectComponentServer>();

			bool isPassable = true;
			if (entity.Data != null)
			{
				//this is a bit strange. I am not sure how to handle weather the wall should have a hitbox or not.
				isPassable = entity.Data.pathingCost == 0; 
			}

			eoc.IsPassable = isPassable;
			eoc.IsDestroyed = false;
			eoc.IsHittable = entity.Data.hittable;
			eoc.IsSeethrough = entity.Data.seeThrough;

			if (!eoc.IsPassable)
			{
				//Test for polygon colliders
				var polyComp = entity.AddComponent<PolygonComponentServer>();
				polyComp.relativePoints.Add(new Vector2(-0.5f, -0.2f));
				polyComp.relativePoints.Add(new Vector2(-0.5f, 0.2f));
				polyComp.relativePoints.Add(new Vector2(0.5f, 0.2f));
				polyComp.relativePoints.Add(new Vector2(0.5f, -0.2f));
				//polyComp.offsetToMiddle = new Vector2(0.5f, 0.5f);

				float wallShort = 0.3f;
				float wallLong = 1.0f;

				var obs = entity.AddComponent<ObstacleComponentServer>();

				if (rotation == 0 || rotation == 180)
				{
					obs.x = (int)position.x;
					obs.y = (int)position.y - (wallShort / 2.0f);
					obs.width = wallLong;
					obs.height = wallShort;
				}
				else if (rotation == 90 || rotation == 270)
				{
					obs.x = (int)position.x;
					obs.y = (int)position.y - (wallShort / 2.0f);
					obs.width = wallShort;
					obs.height = wallLong;
				}

				obs.InitObstacle();
			}
		}

		public void InitEnemy(FNEEntity entity)
		{
			if (entity.EntityType == EntityType.GO_ENEMY)
			{
				InitGOEnemy(entity);
			}
			else if (entity.EntityType == EntityType.ECS_ENEMY)
			{
				InitECSEnemy(entity);
			}
		}

		private void InitGOEnemy(FNEEntity entity)
		{
			entity.AddComponent<AIComponentServer>();
		}

		private void InitECSEnemy(FNEEntity entity)
		{
			entity.AddComponent<FlowFieldComponentServer>();
			entity.AddComponent<NPCPlayerAwareComponentServer>();
		}

		public void QueueEntityForReplacement(FNEEntity entity, string newEntityId)
		{
			ReplacementStack.Push(new Tuple<FNEEntity, string>(entity, newEntityId));
		}

		public void ExecuteEntityReplacementQueue()
		{
			foreach (var tuple in ReplacementStack)
			{
				var tmpPos = tuple.Item1.Position;
				var tmpRotation = tuple.Item1.RotationDegrees;

				GameServer.EntityAPI.NetDestroyEntityImmediate(tuple.Item1);
				var newEntity = GameServer.EntityAPI.NetSpawnEntityImmediate(tuple.Item2, tmpPos, tmpRotation);

				foreach (var comp in tuple.Item1.Components)
				{
					comp.OnReplaced(newEntity);
				}
			}

			ReplacementStack.Clear();
		}

		public FNEEntity CreatePlayer(float2 position, string name)
		{
			var entity = new FNEEntity();
			entity.Init("player", position);
			entity.WorldInstanceIndex = 0;

			var nameComp = entity.AddComponent<NameComponentServer>();
			nameComp.entityName = name;

			entity.Enabled = false;

			var componentsFromXML = DataBank.Instance.GetData<FNEEntityData>(entity.EntityId).components;

			foreach (var compData in componentsFromXML)
			{
				entity.AddComponent(m_CompDataToCompDictionary[compData.GetComponentType()], compData);
			}

			foreach (var comp in entity.Components)
			{
				comp.InitComponentLinks();
			}
			
			var path = GameServer.FilePaths.GetSavedEntityPathFromName(name);
			if (path != string.Empty)
			{
				LoadPlayerEntityFromPath(path, entity);
			}
			else
			{
				entity.GetComponent<EquipmentSystemComponentServer>().GivePlayerStartArmorSet();
			}

			GameServer.GetWorldInstance(entity.WorldInstanceIndex).AddPlayerEntity(entity);
			return entity;
		}

		public void RemovePlayer(NetConnection connection)
		{
			var entity = GameServer.NetConnector.GetPlayerFromConnection(connection);
			var leavingPlayerName = entity.GetComponent<NameComponentServer>().entityName;

			var netBuffer = new NetBuffer();
			netBuffer.EnsureBufferSize(entity.TotalBitsFileBuffer());
			entity.FileSerialize(netBuffer);
			FileUtils.WriteFile(GameServer.FilePaths.CreatePlayerEntityFilePath(leavingPlayerName), netBuffer.Data);

			GameServer.NetConnector.RemoveDisconnectedClient(entity);
			GameServer.MainWorld.RemovePlayerEntity(entity);
			GameServer.NetAPI.Player_RemoveRemote_BA(entity);

			GameServer.NetAPI.Effect_SpawnEffect_BAR(EffectIdConstants.TELEPORT, entity.Position, 0);
			GameServer.NetAPI.Chat_SendMessage_BO($"{leavingPlayerName} has left the server.", connection, Utils.ChatColorMessage.MessageType.SERVER);
		}

		public void LoadPlayerEntityFromPath(string filePath, FNEEntity entity)
		{
			var netBuffer = new NetBuffer
			{
				Data = FileUtils.ReadFile(filePath)
			};

			entity.FileDeserialize(netBuffer);
		}

		//private void DestroyEntity(FNEEntity entityToDestroy, bool isChunkUnload = false)
		//{
		//	if (entityToDestroy.HasComponent<ObstacleComponentServer>())
		//		AgentSimulationSystem.Instance.RemoveObstacle(entityToDestroy);

		//	if (entityToDestroy.Agent != null)
		//		AgentSimulationSystem.Instance.RemoveAgent(entityToDestroy);

		//	GameServer.World.RemoveTickableEntity(entityToDestroy);
		//	GameServer.NetAPI.Entity_RemoveFromPosAndRotUpdateBatch(entityToDestroy);
		//	GameServer.NetConnector.UnsyncEntity(entityToDestroy);

		//	if (!isChunkUnload)
		//	{
		//		if (entityToDestroy.EntityType == EntityType.TILE_OBJECT)
		//		{
		//			GameServer.World.RemoveTileObject(entityToDestroy);
		//		}
		//		else if (entityToDestroy.EntityType == EntityType.EDGE_OBJECT)
		//		{
		//			GameServer.World.RemoveEdgeObject(entityToDestroy);
		//		}
		//		else if (entityToDestroy.EntityType == EntityType.GO_ENEMY || entityToDestroy.EntityType == EntityType.ECS_ENEMY)
		//		{
		//			var currentChunkCell = GameServer.World.GetChunkCellData((int)entityToDestroy.Position.x, (int)entityToDestroy.Position.y);

		//			if (currentChunkCell != null)
		//			{
		//				currentChunkCell.RemoveEnemy(entityToDestroy);
		//			}

		//			//GameServer.World.RemoveEnemyFromTile(entityToDestroy);
		//		}

		//		GameServer.NetAPI.Entity_DestroyEntity_BAR(entityToDestroy);
		//	}

		//	// m_EntityPool.ReturnEntityToPool(entityToDestroy);
		//}

		private void BuildTable()
		{
			var allSharedTypes = typeof(DataComponent).Assembly.GetTypes();
			var allSharedComponenttypes = allSharedTypes.Where(t =>
				t.IsSubclassOf(typeof(FNEComponent))
			).ToList();

			foreach (var type in allSharedComponenttypes)
			{
				AddServerComponentType(type);
			}
		}

		private void AddServerComponentType(Type dataType)
		{
			var allServerTypes = typeof(ServerEntityFactory).Assembly.GetTypes();
			var matchingComponent = allServerTypes.Where(t =>
			{
				return t.IsSubclassOf(dataType);
			}
			).ToList();

			if (matchingComponent.Count == 0)
			{
				Debug.LogError("WARNING: " + dataType + " Does not have a matching server component!");
				return;
			}
			else if (matchingComponent.Count > 1)
			{
				Debug.LogError("WARNING: " + dataType + " Has more than one matching server component!");
				return;
			}

			m_CompDataToCompDictionary.Add(dataType, matchingComponent.First());
		}
	}
}

