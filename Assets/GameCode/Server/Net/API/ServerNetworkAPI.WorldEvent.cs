using FNZ.Shared.Net;
using Unity.Mathematics;

namespace FNZ.Server.Net.API
{
    public partial class ServerNetworkAPI
    {
        public void WorldEvent_SpawnWorldEventToPlayersInRange(INetSerializeableData eventData, float2 position, float radius)
        {
            var message = m_WorldEventMessageFactory.CreateWorldEventMessage(eventData);
            Broadcast_All_InProximity(message, position, radius);
        }

        public void WorldEvent_EndWorldEvent(long uniqueId, bool success)
        {
            var message = m_WorldEventMessageFactory.CreateEndWorldEventMessage(uniqueId, success);
            Broadcast_All(message);
        }
    }
}