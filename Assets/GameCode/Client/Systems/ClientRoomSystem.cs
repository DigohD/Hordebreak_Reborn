using FNZ.Client.Model.World;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace FNZ.Client.Systems
{
    public struct RoomData
    {
        public int2 Position;
        public long RoomId;
    }
    
    public class ClientRoomSystem : SystemBase
    {
        private NativeList<RoomData> m_Queue;
        private NativeList<int2> m_RemoveQueue;

        protected override void OnCreate()
        {
            m_Queue = new NativeList<RoomData>(Allocator.Persistent);
            m_RemoveQueue = new NativeList<int2>(Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            m_Queue.Dispose();
            m_RemoveQueue.Dispose();
        }

        public void AddRoomDataToQueue(RoomData roomData)
        {
            m_Queue.Add(roomData);
        }
        
        public void AddRoomToBeRemvoedQueue(int2 pos)
        {
            m_RemoveQueue.Add(pos);
        }

        protected override void OnUpdate()
        {
            if (m_RemoveQueue.Length > 0)
            {
                for (var i = m_RemoveQueue.Length - 1; i >= 0; i--)
                {
                    var roomData = m_RemoveQueue[i];
                    var chunk = GameClient.World.GetWorldChunk<ClientWorldChunk>();
                    if (chunk == null) continue;
                    GameClient.World.RemoveTileRoom(roomData);
                    m_RemoveQueue.RemoveAt(i);
                }
            }
            
            if (m_Queue.Length > 0)
            {
                for (var i = m_Queue.Length - 1; i >= 0; i--)
                {
                    var roomData = m_Queue[i];
                    var chunk = GameClient.World.GetWorldChunk<ClientWorldChunk>();
                    if (chunk == null) continue;
                    GameClient.World.AddTileRoom(roomData.Position, roomData.RoomId);
                    m_Queue.RemoveAt(i);
                }
            }
        }
    }
}