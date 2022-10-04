using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FNZ.Server;
using FNZ.Server.Controller;
using FNZ.Server.FarNorthZMigrationStuff;
using FNZ.Server.Utils;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components.EnemyStats;
using FNZ.Shared.Model.World;
using FNZ.Shared.Model.World.Tile;
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
        
        private readonly ConcurrentQueue<FNEEntity> m_TickableEntitiesToRemove = new ConcurrentQueue<FNEEntity>();
        private readonly ConcurrentQueue<FNEEntity> m_TickableEntitiesToAdd = new ConcurrentQueue<FNEEntity>();
        private readonly ConcurrentQueue<FlowFieldGenData> m_FlowFieldsToSpawn = new ConcurrentQueue<FlowFieldGenData>();
        private readonly Stack<ILateTickable> m_LateTickableEntities = new Stack<ILateTickable>();
        
        public readonly RealEffectManagerServer RealEffectManager;

        public ServerWorldInstance(int width, int height, int seedX, int seedY) 
            : base(width, height)
        {
            SeedX = seedX;
            SeedY = seedY;

            Id = Guid.NewGuid();
            
            m_Players = new List<FNEEntity>();
            m_TickableEntities = new List<FNEEntity>();
            
            RealEffectManager = new RealEffectManagerServer();

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    Tiles[x, y] = new GameWorldTile();
                }
            }
        }
        
        public void QueueFlowField(FlowFieldGenData data)
        {
            m_FlowFieldsToSpawn.Enqueue(data);
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
            try
            {
                foreach (var entity in m_TickableEntities)
                {
                    if (!entity.Enabled) continue;
                
                    var tickableAndActiveComps = entity.Components
                        .Where(comp => comp is ITickable && comp.Enabled).ToList();

                    foreach (var fneComponent in tickableAndActiveComps)
                    {
                        var comp = (ITickable) fneComponent;
                        comp.Tick(deltaTime);
                        if (comp is ILateTickable && !m_LateTickableEntities.Contains(comp))
                            m_LateTickableEntities.Push((ILateTickable) comp);
                    }
                }
            }
            catch (InvalidOperationException e)
            {
                GameServer.NetAPI.Error_SendErrorMessage_BA("TickableEntities Concurrent Modification!", "");
                Debug.LogError(e);
            }

            try
            {
                for(int i = 0; i < m_LateTickableEntities.Count; i++)
                {
                    m_LateTickableEntities.Pop().LateTick(deltaTime);
                }
            }
            catch (InvalidOperationException e)
            {
                GameServer.NetAPI.Error_SendErrorMessage_BA("LateTickableEntities Concurrent Modification!", "");
                m_LateTickableEntities.Clear();
                Debug.LogError(e);
            }
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