using FNZ.Shared.Model.Entity;
using UnityEngine;

namespace FNZ.Shared.Net
{
	public class NetEntityList
	{
		public FNEEntity[] list;

		private readonly object m_Lock = new object();

		public NetEntityList(int num)
		{
			list = new FNEEntity[num];
		}

		public int GetNumOfSyncedEntities()
		{
			int count = 0;

			for (int i = 0; i < list.Length; i++)
			{
				if (list[i] != null) count++;
			}

			return count;
		}

		public int GetSize()
		{
			return list.Length;
		}

		public void Add(FNEEntity e)
		{
			lock (m_Lock)
			{
				bool foundEmptySlot = false;

				for (int i = 1; i < list.Length; i++)
				{
					if (list[i] == null)
					{
						list[i] = e;
						e.NetId = i;
						foundEmptySlot = true;
						break;
					}
				}

				if (!foundEmptySlot)
				{
					FNEEntity[] newList = new FNEEntity[list.Length * 2];

					for (int i = 1; i < list.Length; i++)
					{
						newList[i] = list[i];
					}

					newList[list.Length] = e;
					e.NetId = list.Length;
					list = newList;
				}

				if (e.NetId == 0)
				{
					Debug.LogError("WAT");
				}
			}
		}

		public void Add(FNEEntity entity, int NetEntityId)
		{
			lock (m_Lock)
			{
				if (NetEntityId == 0) Debug.LogError("The NetAPI entityId given to Add was 0 which is not allowed");
				if (list.Length <= NetEntityId)
				{
					int doubleTimes = NetEntityId / list.Length;
					FNEEntity[] newList = new FNEEntity[list.Length * doubleTimes * 2];

					for (int i = 0; i < list.Length; i++)
					{
						newList[i] = list[i];
					}

					newList[NetEntityId] = entity;
					entity.NetId = NetEntityId;
					list = newList;
				}
				else
				{
					list[NetEntityId] = entity;
					entity.NetId = NetEntityId;
				}

				if (entity.NetId == 0)
				{
					Debug.LogError("WAT AGAIN");
				}
			}
		}

		public void Remove(int index)
		{
			lock (m_Lock)
			{
				if (index == 0) Debug.LogError("The NetAPI entityId given to Remove was 0 which is not allowed");
				if (index < 0) Debug.LogError($"The NetId given to Remove was {index} which is not allowed");
				if (index >= list.Length) Debug.LogError($"The NetId given to Remove was {index} which is not allowed");
				list[index] = null;
			}
		}

		public FNEEntity GetEntity(int index)
		{
			lock (m_Lock)
			{
				if (index == 0) Debug.LogWarning("The NetAPI entityId given to GetEntity was 0 which is not allowed");
				if (index < 0) Debug.LogWarning($"The NetId given to Remove was {index} which is not allowed");
				if (index >= list.Length) 
				{ 
					Debug.LogWarning($"The NetId {index} is larger than netentitylist which is not allowed");
					return null; 
				}
				return list[index];
			}
		}
	}
}

