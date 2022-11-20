using FNZ.Server.Model.World;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace FNZ.Server.Controller.Systems
{
    public struct RoomData
    {
        public int2 Position;
        public long RoomId;
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ServerMainSystem))]
    public class ServerRoomSystem : SystemBase
    {
        private NativeList<RoomData> m_Queue;

        protected override void OnCreate()
        {
            m_Queue = new NativeList<RoomData>(Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            m_Queue.Dispose();
        }

        public void AddRoomDataToQueue(RoomData roomData)
        {
            m_Queue.Add(roomData);
        }

        protected override void OnUpdate()
        {
            if (m_Queue.Length <= 0) return;

            for (var i = m_Queue.Length - 1; i >= 0; i--)
            {
                var roomData = m_Queue[i];
                var chunk = GameServer.MainWorld.GetWorldChunk<ServerWorldChunk>(roomData.Position);
                if (chunk == null) continue;
                chunk.AddTileRoom(roomData.Position, roomData.RoomId);
                m_Queue.RemoveAt(i);
            }
        }
    }
}