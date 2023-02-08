using System.Collections.Generic;
using FNZ.Server.Controller;
using FNZ.Server.Controller.Systems;
using FNZ.Server.FarNorthZMigrationStuff;
using FNZ.Server.Model.World;
using FNZ.Server.Utils;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components.EnemyStats;
using FNZ.Shared.Model.World.Site;
using FNZ.Shared.Net.Dto.Hordes;
using FNZ.Shared.Utils;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Server.Services 
{
	public class EntityAPI
	{
		public FNEEntity CreateEntityImmediate(string entityId, float2 position, int worldInstanceIndex, float rotation = 0)
		{
			if (string.IsNullOrEmpty(entityId))
            {
				Debug.LogError("[SERVER, CreateEntityImmediate] EntityId cannot be null or empty string");
				return null;
            }

			return GameServer.EntityFactory.CreateEntity(entityId, position, worldInstanceIndex, rotation, false);
		}

		public FNEEntity SpawnEntityImmediate(string entityId, float2 position, int worldInstanceIndex, float rotation = 0)
		{
			var entity = GameServer.EntityFactory.CreateEntity(entityId, position, worldInstanceIndex, rotation);

			if (entity == null)
			{
				Debug.LogError($"[SERVER]: Could not spawn entity model on server with id: {entityId}");
			}

			AddEntityToWorldStateImmediate(entity);

			return entity;
		}

		public FNEEntity NetSpawnEntityImmediate(string entityId, float2 position, int worldInstanceIndex, float rotation = 0)
		{
			var entity = SpawnEntityImmediate(entityId, position, worldInstanceIndex, rotation);
			NetSyncEntityToRelevantClients(entity);
			return entity;
		}

		public void DestroyEntityImmediate(FNEEntity entity, bool isChunkUnload = false)
		{
			entity.Enabled = false;
			if (entity.HasComponent<ObstacleComponentServer>())
				AgentSimulationSystem.Instance.RemoveObstacle(entity);

			if (entity.Agent != null)
				entity.Agent.active = false;

			var world = GameServer.WorldInstanceManager.GetWorldInstance(entity.WorldInstanceIndex);

			if (IsEntityTickable(entity))
			{
				if (isChunkUnload && entity.EntityType == EntityType.ECS_ENEMY)
				{
					world.RemoveTickableImmediate(entity);
				}
				else
				{
					world.RemoveTickableEntity(entity);
				}
			}
			
			switch (entity.EntityType)
			{
				case EntityType.TILE_OBJECT:
					world.RemoveTileObject(entity);
					break;

				case EntityType.EDGE_OBJECT:
					world.RemoveEdgeObject(entity);
					break;

				case EntityType.ECS_ENEMY:
					world.RemoveEnemyFromTile(entity);
					break;
				case EntityType.GO_ENEMY:
					world.RemoveEnemyFromTile(entity);
					break;
			}
			
			GameServer.NetAPI.Entity_RemoveFromPosAndRotUpdateBatch(entity);

			if (isChunkUnload)
			{
				GameServer.NetConnector.UnsyncEntity(entity);
            }
		}

		public void NetDestroyEntityImmediate(FNEEntity entity)
		{
			DestroyEntityImmediate(entity);
			NetUnsyncEntityToRelevantClients(entity);
			GameServer.NetAPI.Entity_RemoveFromPosAndRotUpdateBatch(entity);
		}

		public void NetSyncEntityToRelevantClients(FNEEntity entity)
		{
			GameServer.NetConnector.SyncEntity(entity);
			GameServer.NetAPI.Entity_SpawnEntity_BAR(entity);
		}

		public void NetUnsyncEntityToRelevantClients(FNEEntity entity)
		{
			GameServer.NetConnector.UnsyncEntity(entity);
			GameServer.NetAPI.Entity_DestroyEntity_BAR(entity);
		}

		public void AddEntityToWorldStateImmediate(FNEEntity entity, bool addTickablesImmediate = false)
		{
			var world = GameServer.WorldInstanceManager.GetWorldInstance(entity.WorldInstanceIndex);
			
			if (IsEntityTickable(entity))
			{
				if (addTickablesImmediate)
                {
	                world.AddTickableEntityImmediate(entity);
                }
                else
                {
	                world.AddTickableEntity(entity);
				}
				
			}

			switch (entity.EntityType)
			{
				case EntityType.TILE_OBJECT:
					world.AddTileObject(entity);
					break;

				case EntityType.EDGE_OBJECT:
					world.AddEdgeObject(entity);
					break;

				case EntityType.ECS_ENEMY:
				case EntityType.GO_ENEMY:
					world.AddEnemyToTile(entity);
					break;
			}
		}

		 public void SpawnEnemiesWithBudget(
			 int budget,
			 List<EnemySpawnData> enemies,
			 float2 spawnPoint,
			 float innerSpawnRadius,
			 float outerSpawnRadius,
			 bool generateFlowfield,
			 float2 flowfieldPosition,
			 int flowfieldRadius,
			 float playerBudgetScale,
			 string enemySpawnEffect,
			 int worldInstanceIndex
		 )
		 { 
			 var world = GameServer.GetWorldInstance(worldInstanceIndex);
			var budgetMul = 1f + (GameServer.NetConnector.GetConnectedClientsCount() - 1) * 0.75f;

			budget = (int)(budget * budgetMul);

			var hordeEntitySpawnDataBatch = new List<HordeEntitySpawnData>();
	        
            List<FNEEntityData> enemiesWeCanBuy = new List<FNEEntityData>(enemies.Count);

            for (int i = 0; i < enemies.Count; i++)
            {
                var enemyEntity = DataBank.Instance.GetData<FNEEntityData>(enemies[i].enemyRef);
                if (enemyEntity.GetComponentData<EnemyStatsComponentData>().budgetCost <= budget)
                {
                    enemiesWeCanBuy.Add(DataBank.Instance.GetData<FNEEntityData>(enemies[i].enemyRef));
                }
            }
            
            while (enemiesWeCanBuy.Count > 0)
            {
                var totalWeight = 0;
                foreach(var e in enemies)
                {
                    if(enemiesWeCanBuy.Exists(c => c.Id == e.enemyRef)) {
                        totalWeight += e.weight;
                    }
                }

                var spawnRoll = FNERandom.GetRandomIntInRange(0, totalWeight);
                string enemyToSpawn = null;

                foreach (var enemySpawnData in enemies)
                {
                    var enemyEntity = enemiesWeCanBuy.Find(e => e.Id == enemySpawnData.enemyRef);
                    if (enemyEntity != null)
                    {
                        totalWeight -= enemySpawnData.weight;
                        if(spawnRoll >= totalWeight)
                        {
                            enemyToSpawn = enemySpawnData.enemyRef;
                            budget -= enemyEntity.GetComponentData<EnemyStatsComponentData>().budgetCost;
                            break;
                        }
                    }
                }

                for (var i = enemiesWeCanBuy.Count-1; i >= 0; i--)
                {
                    if(enemiesWeCanBuy[i].GetComponentData<EnemyStatsComponentData>().budgetCost > budget)
                    {
                        enemiesWeCanBuy.RemoveAt(i);
                    }
                }

                innerSpawnRadius = innerSpawnRadius > outerSpawnRadius ? innerSpawnRadius : outerSpawnRadius;
                var distance = FNERandom.GetRandomFloatInRange(innerSpawnRadius, outerSpawnRadius);
                var v = new Vector2(distance, 0);
				
                var finalOffset = Quaternion.Euler(0, 0, FNERandom.GetRandomFloatInRange(0, 360)) * v;

                var spawnPosition = new float2(spawnPoint.x + finalOffset.x, spawnPoint.y + finalOffset.y);
                var isTileBlocking = world.IsTileBlocking((int) spawnPosition.x, (int) spawnPosition.y);
                if (isTileBlocking != null && isTileBlocking.Value || world.GetTileObject((int) spawnPosition.x, (int) spawnPosition.y) != null)
                {
	                continue;
                }
                
                var entity = GameServer.EntityAPI.SpawnEntityImmediate(enemyToSpawn, spawnPosition, worldInstanceIndex);

                GameServer.NetConnector.SyncEntity(entity);

                if (!string.IsNullOrEmpty(enemySpawnEffect))
                {
	                GameServer.NetAPI.Effect_SpawnEffect_BAR(world, enemySpawnEffect, spawnPosition, 0);
                }
                
                hordeEntitySpawnDataBatch.Add(new HordeEntitySpawnData
                {
                    NetId = entity.NetId,
                    EntityIdCode = IdTranslator.Instance.GetIdCode<FNEEntityData>(entity.EntityId),
                    Position = spawnPosition,
                    Rotation = entity.RotationDegrees,
                });
            }
                
            GameServer.NetAPI.Entity_SpawnHordeEntity_Batched_BAR(hordeEntitySpawnDataBatch);
            if (generateFlowfield)
            {
	            FlowFieldUtility.GenerateFlowFieldAndUpdateEnemies(world, flowfieldPosition, flowfieldRadius, FlowFieldType.Sound);
            }
        }
		 
		 // public float2 SpawnSingleEnemyWithinRadius(
			//  string id,
			//  float2 spawnPoint,
			//  float spawnRadius
		 // )
		 // { var distance = FNERandom.GetRandomFloatInRange(0, spawnRadius);
			// var v = new Vector2(distance, 0);
		 //
			// var finalOffset = Quaternion.Euler(0, 0, FNERandom.GetRandomFloatInRange(0, 360)) * v;
		 //
			// var spawnPosition = new float2(spawnPoint.x + finalOffset.x, spawnPoint.y + finalOffset.y);
		 //
			// var entity = GameServer.EntityAPI.SpawnEntityImmediate("shrubber", spawnPosition);
		 //
			// GameServer.NetConnector.SyncEntity(entity);
			//
			// var hordeEntitySpawnDataBatch = new List<HordeEntitySpawnData>();
			// 	
			// hordeEntitySpawnDataBatch.Add(new HordeEntitySpawnData
			// {
			// 	NetId = entity.NetId,
			// 	EntityIdCode = IdTranslator.Instance.GetIdCode<FNEEntityData>(entity.EntityId),
			// 	Position = spawnPosition,
			// 	Rotation = entity.RotationDegrees,
			// });
			//
			// GameServer.NetAPI.Entity_SpawnHordeEntity_Batched_BAR(hordeEntitySpawnDataBatch);
		 //
			// return spawnPosition;
		 // }
		
		private bool IsEntityTickable(FNEEntity entity)
		{
			return entity.Components.FindAll(e => e is ITickable).Count > 0;
		}
	}
}