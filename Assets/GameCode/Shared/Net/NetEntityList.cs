using FNZ.Shared.Model.Entity;
using Unity.Entities;
using UnityEngine;

namespace FNZ.Shared.Net
{
	public struct NetEntityWrapper
    {
        public FNEEntity fneEntity;
        public Entity ecsEntity;

        public bool IsDefault()
        {
            return fneEntity == null && ecsEntity == default;
        }
    }

    public class NetEntityList
    {
        public NetEntityWrapper[] list;

        private int firstFreeIndex = 1;

        public NetEntityList(int num)
        {
            list = new NetEntityWrapper[num];
        }

        public void Add(FNEEntity e)
        {
            list[firstFreeIndex] = new NetEntityWrapper { fneEntity = e };
            e.NetId = firstFreeIndex;

            FindNewFirstFreeIndexOnAdd();

            if (e.NetId == 0)
            {
                Debug.LogError("WAT");
            }
        }

        public int Add(Entity e)
        {
            return Add(new NetEntityWrapper { ecsEntity = e });
        }

        private int Add(NetEntityWrapper wrapper)
        {
            var indexAddedOn = firstFreeIndex;
            list[firstFreeIndex] = wrapper;

            FindNewFirstFreeIndexOnAdd();

            return indexAddedOn;
        }

        public void Add(FNEEntity entity, int NetEntityId)
        {
            Add(new NetEntityWrapper { fneEntity = entity }, NetEntityId);
        }

        public void Add(Entity entity, int NetEntityId)
        {
            Add(new NetEntityWrapper { ecsEntity = entity }, NetEntityId);
        }

        private void Add(NetEntityWrapper wrapper, int NetEntityId)
        {
            if (NetEntityId == 0) Debug.LogError("The NetAPI entityId given to Add was 0 which is not allowed");
            if (NetEntityId == -1) Debug.LogError("The NetAPI entityId given to Add was -1 which is not allowed");
            if (list.Length <= NetEntityId)
            {
                int doubleTimes = NetEntityId / list.Length;
                NetEntityWrapper[] newList = new NetEntityWrapper[list.Length * doubleTimes * 2];

                for (int i = 0; i < list.Length; i++)
                {
                    newList[i] = list[i];
                }

                newList[NetEntityId] = wrapper;

                if(wrapper.fneEntity != null)
                {
                    wrapper.fneEntity.NetId = NetEntityId;
                }
                list = newList;
            }
            else
            {
                list[NetEntityId] = wrapper;
                if (wrapper.fneEntity != null)
                {
                    wrapper.fneEntity.NetId = NetEntityId;
                }
            }

            FindNewFirstFreeIndexOnAdd();
        }

        private void FindNewFirstFreeIndexOnAdd()
        {
            bool foundEmptySlot = false;

            for (int i = firstFreeIndex; i < list.Length; i++)
            {
                if (list[i].IsDefault())
                {
                    firstFreeIndex = i;
                    foundEmptySlot = true;
                    break;
                }
            }

            if (!foundEmptySlot)
            {
                NetEntityWrapper[] newList = new NetEntityWrapper[list.Length * 2];

                for (int i = 1; i < list.Length; i++)
                {
                    newList[i] = list[i];
                }

                firstFreeIndex = list.Length;
                list = newList;
            }
        }

        public void Remove(int index)
        {
            if (index == 0) Debug.LogError("The NetAPI entityId given to Remove was 0 which is not allowed");
            if (index < 0) Debug.LogError($"The NetId given to Remove was {index} which is not allowed");
            if (index >= list.Length) Debug.LogError($"The NetId given to Remove was {index} which is not allowed");
            list[index] = default;

            if (index < firstFreeIndex)
            {
                firstFreeIndex = index;
            }
        }

        public FNEEntity GetFneEntity(int index)
        {
            if (index == 0) Debug.LogWarning("The NetAPI entityId given to GetEntity was 0 which is not allowed");
            if (index < 0) Debug.LogWarning($"The NetId given to Remove was {index} which is not allowed");
            if (index >= list.Length)
            {
                Debug.LogWarning($"The NetId {index} is larger than netentitylist which is not allowed");
                return null;
            }

            return list[index].fneEntity;
        }

        public Entity GetEcsEntity(int index)
        {
            if (index == 0) Debug.LogWarning("The NetAPI entityId given to GetEntity was 0 which is not allowed");
            if (index < 0) Debug.LogWarning($"The NetId given to Remove was {index} which is not allowed");
            if (index >= list.Length)
            {
                Debug.LogWarning($"The NetId {index} is larger than netentitylist which is not allowed");
                return default;
            }

            return list[index].ecsEntity;
        }
    }
}

