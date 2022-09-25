using UnityEngine;

namespace FNZ.Shared.Utils
{
	public class FNEList<T>
	{
		private T[] list;

		public FNEList(int num)
		{
			list = new T[num];
		}

		public int GetSize()
		{
			return list.Length;
		}

		public void Add(T e, int NetEntityId)
		{
			if (NetEntityId == 0) throw new System.Exception("The NetAPI entityId given to Add was 0 which is not allowed");
			if (list.Length <= NetEntityId)
			{
				int doubleTimes = NetEntityId / list.Length;
				T[] newList = new T[list.Length * doubleTimes * 2];

				for (int i = 0; i < list.Length; i++)
				{
					newList[i] = list[i];
				}

				newList[NetEntityId] = e;

				list = newList;
			}
			else
			{
				if (NetEntityId >= list.Length || NetEntityId < 0)
				{
					Debug.LogWarning("NET ID FAILED " + NetEntityId);
				}
				list[NetEntityId] = e;
			}
		}

		public void Remove(int index)
		{
			if (index == 0) throw new System.Exception("The NetAPI entityId given to Remove was 0 which is not allowed");
			list[index] = default;
		}

		public T GetEntity(int index)
		{
			if (index == 0) throw new System.Exception("The NetAPI entityId given to GetEntity was 0 which is not allowed");
			if (index >= list.Length) return default;
			return list[index];
		}
	}
}