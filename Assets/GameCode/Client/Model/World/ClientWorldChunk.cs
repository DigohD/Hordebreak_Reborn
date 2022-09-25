using FNZ.Client.Systems.Hordes;
using FNZ.Client.View.Prefab;
using FNZ.Client.View.World;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.World;
using FNZ.Shared.Net.Dto.Hordes;
using Lidgren.Network;
using System.Collections.Generic;
using FNZ.Client.Model.Entity.Components.EdgeObject;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace FNZ.Client.Model.World
{
    public delegate void OnChunkUnloaded();
    public delegate void OnGenerateUVs();

    public class ClientWorldChunk : WorldChunk
    {
        public OnChunkUnloaded d_OnChunkUnloadedEvent;
        public OnGenerateUVs d_OnGenerateUVsEvent;

        public ClientWorldChunkView view;

        private HordeSpawnerSystem m_HordeSpawnerSystem;

        public List<FNEEntity> EntitiesToSync = new List<FNEEntity>();

        public ClientWorldChunk(byte chunkX, byte chunkY, byte size)
            : base(chunkX, chunkY, size)
        {
            m_HordeSpawnerSystem = GameClient.ECS_ClientWorld.GetExistingSystem<HordeSpawnerSystem>();
        }

        public override void ClearChunk()
        {
            foreach (var edgeObj in SouthEdgeObjects)
            {
                if (edgeObj == null) continue;
                GameClient.NetConnector.UnsyncEntity(edgeObj);
                GameClient.ViewConnector.RemoveView(edgeObj.NetId);
            }

            foreach (var edgeObj in WestEdgeObjects)
            {
                if (edgeObj == null) continue;
                GameClient.NetConnector.UnsyncEntity(edgeObj);
                GameClient.ViewConnector.RemoveView(edgeObj.NetId);
            }

            foreach (var tileObj in TileObjects)
            {
                if (tileObj == null) continue;
                GameClient.NetConnector.UnsyncEntity(tileObj);
                GameClient.ViewConnector.RemoveView(tileObj.NetId);
            }

            d_OnChunkUnloadedEvent?.Invoke();
        }

        public override void NetDeserialize(NetBuffer nb)
        {
            Profiler.BeginSample("NetDeserialize tiles and floatpointobjects");
            base.NetDeserialize(nb);
            Profiler.EndSample();

            Profiler.BeginSample("NetDeserializeEdgeObjects");
            NetDeserializeEdgeObjects(nb);
            Profiler.EndSample();

            Profiler.BeginSample("NetDeserializeTileObjects");
            NetDeserializeTileObjects(nb);
            Profiler.EndSample();

            // Profiler.BeginSample("NetDeserializeEnemies");
            // NetDeserializeEnemies(nb);
            // Profiler.EndSample();

            Profiler.BeginSample("SetTileRooms");
            SetTileRooms();
            Profiler.EndSample();
        }

        private void SetTileRooms()
        {
            var worldX = ChunkX * Size;
            var worldY = ChunkY * Size;
            for (var i = 0; i < TileRooms.Length; i++)
            {
                var worldEquivalent = new int2(worldX + (i % Size), worldY + (i / Size));
                var tileRoom = GameClient.RoomManager.GetTileRoomWithoutWorldData(worldEquivalent);
                if (tileRoom != null)
                    TileRooms[i] = tileRoom.Id;
            }
        }

        private void NetDeserializeEdgeObjects(NetBuffer nb)
        {
            var count = nb.ReadInt32();

            for (var i = 0; i < count; i++)
            {
                var entityId = IdTranslator.Instance.GetId<FNEEntityData>(nb.ReadUInt16());
                var edgeObject = GameClient.EntityFactory.CreateEdgeObject(entityId);
                edgeObject.NetDeserialize(nb);
                EntitiesToSync.Add(edgeObject);
            }

            count = nb.ReadInt32();

            for (var i = 0; i < count; i++)
            {
                var entityId = IdTranslator.Instance.GetId<FNEEntityData>(nb.ReadUInt16());
                var edgeObject = GameClient.EntityFactory.CreateEdgeObject(entityId);
                edgeObject.NetDeserialize(nb);
                EntitiesToSync.Add(edgeObject);
            }
        }

        private void NetDeserializeTileObjects(NetBuffer nb)
        {
            var count = nb.ReadInt32();

            for (var i = 0; i < count; i++)
            {
                var idCode = nb.ReadUInt16();

                var entityId = IdTranslator.Instance.GetId<FNEEntityData>(idCode);

                var tileObject = GameClient.EntityFactory.CreateTileObject(entityId);
                tileObject.NetDeserialize(nb);

                EntitiesToSync.Add(tileObject);
            }
        }

        private void NetDeserializeEnemies(NetBuffer nb)
        {
            var amount = nb.ReadInt32();

            for (var i = 0; i < amount; i++)
            {
                var data = new HordeEntitySpawnData();
                data.NetDeserialize(nb);

                var entityId = IdTranslator.Instance.GetId<FNEEntityData>(data.EntityIdCode);

                var entityModel = GameClient.EntityFactory.CreateEnemy(entityId, data.NetId, data.Position, data.Rotation);
                if (entityModel.EntityType == EntityType.GO_ENEMY)
                    entityModel.NetDeserialize(nb);

                EntitiesToSync.Add(entityModel);
            }
        }

        public void DelegateInvokeRerender()
        {
            d_OnGenerateUVsEvent?.Invoke();
        }
    }
}