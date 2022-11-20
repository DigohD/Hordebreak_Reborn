using System.Collections.Generic;
using FNZ.Server.Model.Entity.Components.Player;
using FNZ.Server.Model.Entity.Components.SpawnPoint;
using FNZ.Server.Services.QuestManager;
using FNZ.Server.Utils;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components.EnemyStats;
using FNZ.Shared.Model.WorldEvent;
using FNZ.Shared.Net.Dto.Events;
using FNZ.Shared.Net.Dto.Hordes;
using FNZ.Shared.Utils;
using Unity.Mathematics;

namespace FNZ.Server.WorldEvents
{
    public class SurvivalEventServer : IWorldEvent
    {
        private float m_Timer;
        private float m_Progress;
        
        private readonly List<float2> m_SpawnPoints = new List<float2>();

        private readonly float2 m_Position;
        private readonly WorldEventData m_Data;
        private readonly FNEEntity m_Parent;

        private readonly long m_UniqueId;

        private bool m_EventSuccess = false;
        
        public SurvivalEventServer(FNEEntity entity, WorldEventData data)
        {
            m_Position = entity.Position;
            m_Data = data;
            m_Parent = entity;

            m_UniqueId = WorldEventManager.EventId++;
        }

        public void OnTrigger()
        {
            var eventDto = new SurvivalWorldEventDto
            {
                Position = m_Position,
                IdCode = IdTranslator.Instance.GetIdCode<WorldEventData>(m_Data.Id),
                StartTimeStamp = FNEUtil.NanoTime(),
                UniqueId = m_UniqueId
            };
				
            GameServer.NetAPI.WorldEvent_SpawnWorldEventToPlayersInRange(eventDto, m_Position, m_Data.PlayerRangeRadius);
            
            if (m_SpawnPoints.Count <= 0)
            {
                var startY = (int)m_Position.y - m_Data.SpawnRadius;
                var startX = (int)m_Position.x - m_Data.SpawnRadius;

                if (startY < 0) startY = 0;
                if (startX < 0) startX = 0;
            
                var gridSizeX = m_Data.SpawnRadius * 2;
                var gridSizeY = m_Data.SpawnRadius * 2;
            
                if (startX + gridSizeX > GameServer.MainWorld.WIDTH) gridSizeX = GameServer.MainWorld.WIDTH - startX;
                if (startY + gridSizeY > GameServer.MainWorld.HEIGHT) gridSizeY = GameServer.MainWorld.HEIGHT - startY;

                for (var y = startY; y < gridSizeY + startY; y++)
                {
                    for (var x = startX; x < gridSizeX + startX; x++)
                    {
                        var tileObject = GameServer.MainWorld.GetTileObject(x, y);

                        if (tileObject == null) continue;
                        if (tileObject.HasComponent<SpawnPointComponentServer>())
                        {
                            m_SpawnPoints.Add(tileObject.Position);
                        }
                    }
                }
            }

            SpawnEnemies();
        }

        private void SpawnEnemies()
        {
            if (m_SpawnPoints.Count <= 0 || m_Data.Enemies == null || m_Data.Enemies.Count == 0)
            {
                return;
            }
            
            var index = FNERandom.GetRandomIntInRange(0, m_SpawnPoints.Count);
            var spawnPoint = m_SpawnPoints[index];
            
            GameServer.EntityAPI.SpawnEnemiesWithBudget(
                m_Data.SpawnBudget,
                m_Data.Enemies,
                spawnPoint,
                0f,
                0.5f,
                true,
                m_Position,
                m_Data.SpawnRadius,
                0.75f,
                m_Data.EnemySpawnEffectRef
            );
        }
        
        public void OnFinished()
        {
            GameServer.NetAPI.WorldEvent_EndWorldEvent(
                m_UniqueId,
                m_EventSuccess
            );

            if (m_EventSuccess)
            {
                if (!string.IsNullOrEmpty(m_Data.OnSuccessEventRef))
                {
                    GameServer.EventManager.SpawnWorldEvent(m_Data.OnSuccessEventRef, m_Parent);
                    
                    QuestManager.OnEventSuccess(m_Data.Id);
                    
                    if (!string.IsNullOrEmpty(m_Data.SuccessIconRef) 
                        && !string.IsNullOrEmpty(m_Data.SuccessNotificationColorRef)
                        && !string.IsNullOrEmpty(m_Data.SuccessNotificationTextRef))
                    {
                        GameServer.NetAPI.Notification_SendNotification_BA(
                            m_Data.SuccessIconRef, 
                            m_Data.SuccessNotificationColorRef, 
                            "false", 
                            $"{Localization.GetString(m_Data.SuccessNotificationTextRef)}"
                        );
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(m_Data.OnFailureEventRef))
                {
                    GameServer.EventManager.SpawnWorldEvent(m_Data.OnFailureEventRef, m_Parent);

                    if (!string.IsNullOrEmpty(m_Data.FailedIconRef)
                        && !string.IsNullOrEmpty(m_Data.FailedNotificationColorRef)
                        && !string.IsNullOrEmpty(m_Data.FailedNotificationTextRef))
                    {
                        GameServer.NetAPI.Notification_SendNotification_BA(
                            m_Data.FailedIconRef, 
                            m_Data.FailedNotificationColorRef, 
                            "false", 
                            $"{Localization.GetString(m_Data.FailedNotificationTextRef)}"
                        );
                    }
                }
            }
        }

        private bool ArePlayersWithinBoundary()
        {
            var players = GameServer.MainWorld.GetAllPlayers();
            var eventSuccess = false;
            
            foreach (var player in players)
            {
                if (player.GetComponent<PlayerComponentServer>().IsDead)
                    continue;
                
                var playerPos = player.Position;
                if (math.distance(playerPos, m_Position) <= m_Data.PlayerRangeRadius && !player.IsDead)
                {
                    eventSuccess = true;
                    break;
                }
            }

            return !eventSuccess;
        }
        
        public void Tick(float deltaTime)
        {
            if (m_Progress >= m_Data.Duration)
            {
                m_EventSuccess = true;
                GameServer.EventManager.RemoveEvent(this);
            }
            else
            {
                m_Progress += deltaTime;
                m_Timer += deltaTime;

                if (m_Timer >= m_Data.SpawnFrequency)
                {
                    if (ArePlayersWithinBoundary())
                    {
                        GameServer.EventManager.RemoveEvent(this);
                    }
                    else
                    {
                        m_Timer = 0;
                        SpawnEnemies();
                    }
                }
            }
        }
    }
}