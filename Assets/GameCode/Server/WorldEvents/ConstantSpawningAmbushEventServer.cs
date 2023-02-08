using System.Collections;
using System.Collections.Generic;
using FNZ.Server.Model.Entity.Components.Player;
using FNZ.Server.Model.Entity.Components.SpawnPoint;
using FNZ.Server.Services.QuestManager;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.WorldEvent;
using FNZ.Shared.Net.Dto.Events;
using FNZ.Shared.Utils;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Server.WorldEvents 
{

	public class ConstantSpawningAmbushEventServer : IWorldEvent
	{
        private float m_Timer;
        private float m_Progress;
        
        private readonly WorldEventData m_Data;
        private readonly FNEEntity m_Parent;

        private readonly long m_UniqueId;

        private float2 m_Position;
        
        private bool m_EventSuccess = true;
        
        public ConstantSpawningAmbushEventServer(FNEEntity entity, WorldEventData data)
        {
            m_Position = entity.Position;
            m_Data = data;
            m_Parent = entity;

            m_UniqueId = WorldEventManager.EventId++;
        }

        public void OnTrigger()
        {
            SpawnEnemies();
        }

        private void SpawnEnemies()
        {
            if (m_Data.Enemies == null || m_Data.Enemies.Count == 0)
            {
                return;
            }
            
            GameServer.EntityAPI.SpawnEnemiesWithBudget(
                m_Data.SpawnBudget,
                m_Data.Enemies,
                m_Position,
                m_Data.SpawnRadius,
                m_Data.SpawnRadius,
                true,
                m_Position,
                m_Data.SpawnRadius,
                0.75f, 
                m_Data.EnemySpawnEffectRef,
                m_Parent.WorldInstanceIndex
            );
        }
        
        public void OnFinished()
        {
            if (!string.IsNullOrEmpty(m_Data.OnSuccessEventRef))
            {
                GameServer.EventManager.SpawnWorldEvent(m_Data.OnSuccessEventRef, m_Parent);
                
                QuestManager.OnEventSuccess(m_Data.Id);
            }
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
                    m_Timer = 0;
                    SpawnEnemies();
                }
            }
        }
	}
}