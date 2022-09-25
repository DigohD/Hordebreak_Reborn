using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.WorldEvent;

namespace FNZ.Server.WorldEvents
{
    public class ReplaceTileObjectEvent : IWorldEvent
    {
        private readonly FNEEntity m_Parent;
        private readonly WorldEventData m_Data;
        private long m_UniqueId;
        
        public ReplaceTileObjectEvent(FNEEntity entity, WorldEventData data)
        {
            m_Data = data;
            m_Parent = entity;
            
            m_UniqueId = WorldEventManager.EventId++;
        }
        
        public void OnTrigger()
        {
            
        }

        public void OnFinished()
        {
            if (!string.IsNullOrEmpty(m_Data.TransformedEntityRef))
                GameServer.EntityFactory.QueueEntityForReplacement(m_Parent, m_Data.TransformedEntityRef);
        }

        public void Tick(float deltaTime)
        {
            GameServer.EventManager.RemoveEvent(this);
        }
    }
}