using FNZ.Shared.FarNorthZMigrationStuff;
using FNZ.Shared.Model.Entity.Components;
using FNZ.Shared.Utils;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Shared.Model.Entity
{
    public static class EntityType
    {
        public const string PLAYER = "Player";
        public const string TILE = "Tile";
        public const string EDGE_OBJECT = "EdgeObject";
        public const string FLOATING_POINT_OBJECT = "FloatpointObject";
        public const string TILE_OBJECT = "TileObject";
        public const string PROJECTILE = "Projectile";
        public const string ENEMY = "Enemy";
        public const string ECS_ENEMY = "ECSEnemy";
        public const string GO_ENEMY = "GOEnemy";
        public const string VEHICLE = "Vehicle";
    }

    public class FNEEntity
    {
        public FNEEntityData Data;

        public NPC_Agent Agent;

        public string EntityId;
        public string EntityType;

        public float2 Position;
        public float3 Scale;
        public float RotationDegrees;

        public int NetId = -1;

        public bool Enabled = true;
        public bool IsDead = false;
        public bool IsTickable = false;

        public List<FNEComponent> Components = new List<FNEComponent>();

        public void Init(string entityId, float2 position, float rotation = 0, bool enabled = true)
        {
            EntityId = entityId;
            Position = position;
            RotationDegrees = rotation;

            Data = DataBank.Instance.GetData<FNEEntityData>(entityId);
            EntityType = Data.entityType;
            Enabled = true;
            IsDead = false;
        }

        public void Reset()
        {
            Agent = null;
            Data = null;
            Position = new float2(0, 0);
            Scale = new float3(1, 1, 1);
            RotationDegrees = 0;
            NetId = -1;
            Enabled = false;
            IsDead = true;
            IsTickable = false;
            Components.Clear();
        }

        public void SendComponentMessage(FNEComponentMessage message)
        {
            Components.ForEach(comp => comp.Receive(message));
        }

        public T AddComponent<T>() where T : FNEComponent, new()
        {
            T newComp = new T();

            foreach (var comp in Components)
            {
                if (comp.GetType() == typeof(T))
                {
                    Debug.LogError("COMPONENT ADDED TWICE TO ENTITY!");
                    return comp as T;
                }
            }

            Components.Add(newComp);
            newComp.ParentEntity = this;

            newComp.Init();

            return newComp;
        }

        public FNEComponent AddComponent(Type newCompType, DataComponent data = null)
        {
            foreach (var comp in Components)
            {
                if (comp.GetType() == newCompType)
                {
                    Debug.LogError("COMPONENT ADDED TWICE TO ENTITY!");
                    return comp;
                }
            }

            var newComp = (FNEComponent)Activator.CreateInstance(newCompType);

            Components.Add(newComp);
            newComp.ParentEntity = this;

            if (data == null)
            {
                Debug.LogError("WARNING: " + newCompType + " Was added without any data!");
            }

            newComp.SetData(data);
            newComp.Init();

            return newComp;
        }

        public void RemoveComponent<T>()
        {
            Components.RemoveAll(c => c is T);
        }

        public T GetComponent<T>() where T : FNEComponent
        {
            return Components
                .Where(comp => comp is T)
                .FirstOrDefault() as T;
        }

        public bool HasComponent<T>()
        {
            return Components.FindAll(c => c is T).Count > 0;
        }

        public List<FNEComponent> GetInteractableComponents()
        {
            return Components.FindAll(c => c is IInteractableComponent && ((IInteractableComponent)c).IsInteractable());
        }

        public int TotalBitsNetBuffer()
        {
            int compBits = (GetComponentDataSizeInBytes() + 6 + 2) * 8;
            int netIdBits = 32;
            return compBits + netIdBits;
        }

        public int TotalBitsFileBuffer()
        {
            int compBits = (GetComponentDataSizeInBytes() + 6 + 2) * 8;
            return compBits;
        }

        public int GetComponentDataSizeInBytes()
        {
            int componentDataSizeInBytes = 0;

            foreach (var component in Components)
                componentDataSizeInBytes += component.GetSizeInBytes();

            return componentDataSizeInBytes;
        }

        public void SerializePosition(NetBuffer writer)
        {
            byte chunkX = (byte)(Position.x / 32);
            byte chunkY = (byte)(Position.y / 32);

            ushort localX = FNEUtil.PackFloatAsShort(Position.x % 32);
            ushort localY = FNEUtil.PackFloatAsShort(Position.y % 32);

            writer.Write(chunkX);
            writer.Write(chunkY);

            writer.Write(localX);
            writer.Write(localY);
        }

        public void SerializeRotation(NetBuffer writer)
        {
            writer.Write(FNEUtil.PackFloatAsShort(
                RotationDegrees < 0 ? RotationDegrees + 360 : RotationDegrees
            ));
        }

        public void NetSerialize(NetBuffer writer)
        {
            writer.Write(NetId);
            SerializePosition(writer);
            SerializeRotation(writer);
            Serialize(writer);
        }

        public void NetDeserialize(NetBuffer reader)
        {
            NetId = reader.ReadInt32();
            DeserializePosition(reader);
            DeserializeRotation(reader);
            Deserialize(reader);
        }

        public void DeserializePosition(NetBuffer reader)
        {
            byte chunkX = reader.ReadByte();
            byte chunkY = reader.ReadByte();

            float x = FNEUtil.UnpackShortToFloat(reader.ReadUInt16());
            float y = FNEUtil.UnpackShortToFloat(reader.ReadUInt16());

            Position = new float2(x + chunkX * 32, y + chunkY * 32);
        }

        public void DeserializeRotation(NetBuffer reader)
        {
            RotationDegrees = FNEUtil.UnpackShortToFloat(reader.ReadUInt16());
        }

        public void FileSerialize(NetBuffer writer)
        {
            SerializePosition(writer);
            SerializeRotation(writer);
            Components.ForEach(comp => comp.FileSerialize(writer));
        }

        public void FileDeserialize(NetBuffer reader)
        {
            DeserializePosition(reader);
            DeserializeRotation(reader);
            Components.ForEach(comp => comp.FileDeserialize(reader));
        }

        private void Serialize(NetBuffer writer)
        {
            Components.ForEach(comp => comp.Serialize(writer));
        }

        private void Deserialize(NetBuffer reader)
        {
            Components.ForEach(comp => comp.Deserialize(reader));
        }

        public static string GetEntityViewVariationId(FNEEntityData data, float2 position)
        {
            var seed = (position.x * 2.33f) + position.y;
            FNERandom.InitSeed((int)seed);
            int index = FNERandom.GetRandomIntInRange(0, data.entityViewVariations.Count);
            return data.entityViewVariations[index];
        }
    }
}

