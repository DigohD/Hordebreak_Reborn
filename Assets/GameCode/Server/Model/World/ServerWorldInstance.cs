using System.Collections.Concurrent;
using System.Collections.Generic;
using FNZ.Server.FarNorthZMigrationStuff;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components.EnemyStats;
using FNZ.Shared.Model.World;
using FNZ.Shared.Utils;
using UnityEngine;

namespace GameCode.Server.Model.World
{
    public class ServerWorldInstance : GameWorldInstance
    {
        public int SeedX;
        public int SeedY;
        
        private readonly List<FNEEntity> m_Players;
        private readonly List<FNEEntity> m_TickableEntities;
        
        private readonly ConcurrentQueue<FNEEntity> m_TickableEntitiesToRemove;
        private readonly ConcurrentQueue<FNEEntity> m_TickableEntitiesToAdd;

        public ServerWorldInstance(int width, int height, int seedX, int seedY) 
            : base(width, height)
        {
            SeedX = seedX;
            SeedY = seedY;
            
            m_Players = new List<FNEEntity>();
            m_TickableEntities = new List<FNEEntity>();

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    Tiles[x, y] = new GameWorldTile();
                }
            }
        }

        public void AddTickableEntity(FNEEntity entity)
        {
            m_TickableEntities.Add(entity);
            if (entity.EntityType == EntityType.ECS_ENEMY)
            {
                var stats = entity.Data.GetComponentData<EnemyStatsComponentData>();

                AgentSimulationSystem.Instance.AddAgent(
                    entity,
                    new Vector2(entity.Position.x, entity.Position.y),
                    stats.agentRadius,
                    FNERandom.GetRandomFloatInRange(stats.minSpeed, stats.maxSpeed)
                );
            }
        }

        public void Tick(float deltaTime)
        {
            
        }
        
        public void RemoveTickableEntity(FNEEntity entity)
        {
            entity.Enabled = false;
            m_TickableEntitiesToRemove.Enqueue(entity);
        }
        
        public void RemovePlayerEntity(FNEEntity playerToRemove)
        {
            m_Players.Remove(playerToRemove);
            RemoveTickableEntity(playerToRemove);
        }
        
        public List<FNEEntity> GetAllPlayers()
        {
            return m_Players;
        }
    }
}