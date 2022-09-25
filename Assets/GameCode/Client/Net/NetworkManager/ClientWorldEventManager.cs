using FNZ.Client;
using FNZ.Client.Net;
using FNZ.Shared.Model;
using FNZ.Shared.Model.WorldEvent;
using FNZ.Shared.Net;
using FNZ.Shared.Net.Dto.Events;
using Lidgren.Network;
using Unity.Mathematics;

namespace GameCode.Client.Net.NetworkManager
{
    public delegate void OnEventReceived(
        WorldEventReceivedData worldEvent
    );
    
    public delegate void OnEventFinishedReceived(
        WorldEventFinishedReceived worldEventFinished
    );

    public struct WorldEventReceivedData
    {
        public long Id;
        public WorldEventType Type;
        public WorldEventData Data;
        public double StartTimeStamp;
        public float2 Position;
    }

    public struct WorldEventFinishedReceived
    {
        public long Id;
        public bool Success;
    }
    
    public class ClientWorldEventNetworkManager : INetworkManager
    {
        public static OnEventReceived d_OnEventReceived;
        public static OnEventFinishedReceived d_OnEventFinishedReceived;
        
        public ClientWorldEventNetworkManager()
        {
            GameClient.NetConnector.Register(NetMessageType.WORLD_EVENT, OnWorldEventMessageReceived);
            GameClient.NetConnector.Register(NetMessageType.WORLD_EVENT_END, OnEndWorldEventMessageReceived);
        }

        private void OnEndWorldEventMessageReceived(ClientNetworkConnector net, NetIncomingMessage incMsg)
        {
            var uniqueId = incMsg.ReadInt64();
            var success = incMsg.ReadBoolean();

            var eventEnd = new WorldEventFinishedReceived
            {
                Id = uniqueId,
                Success = success
            };
            
            d_OnEventFinishedReceived?.Invoke(eventEnd);
        }
        
        private void OnWorldEventMessageReceived(ClientNetworkConnector net, NetIncomingMessage incMsg)
        {
            var eventDto = new SurvivalWorldEventDto();
            eventDto.NetDeserialize(incMsg);
            var idCode = eventDto.IdCode;
            var id = IdTranslator.Instance.GetId<WorldEventData>(idCode);
            var eventData = DataBank.Instance.GetData<WorldEventData>(id);
            
            var eventReceived = new WorldEventReceivedData
            {
                Id = eventDto.UniqueId,
                Data = eventData,
                Position = eventDto.Position,
                Type = eventData.EventType,
                StartTimeStamp = eventDto.StartTimeStamp
            };
            
            d_OnEventReceived?.Invoke(eventReceived);
        }
    }
}