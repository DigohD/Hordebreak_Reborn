namespace FNZ.Server.FarNorthZMigrationStuff
{
	public class FNEHeap<T> where T : IHeapItem<T>
	{
		T[] items;
		int currentItemCount;

		public FNEHeap(int maxHeapSize)
		{
			items = new T[maxHeapSize];
		}

		public void Add(T item)
		{
			item.HeapIndex = currentItemCount;
			items[currentItemCount] = item;
			SortUp(item);
			currentItemCount++;
		}

		public T RemoveFirst()
		{
			T firstItem = items[0];
			currentItemCount--;
			items[0] = items[currentItemCount];
			items[0].HeapIndex = 0;
			SortDown(items[0]);
			return firstItem;
		}

		public void UpdateItem(T item)
		{
			SortUp(item);
		}

		public int Count
		{
			get
			{
				return currentItemCount;
			}
		}

		public bool Contains(T item)
		{
			for (int i = 0; i < currentItemCount; i++)
			{
				if (items[i].Equals(item))
					return true;
			}

			return false;
			//return Equals(items[item.HeapIndex], item);
		}

		private void SortDown(T item)
		{
			while (true)
			{
				int childIndexLeft = item.HeapIndex * 2 + 1;
				int childIndexRight = item.HeapIndex * 2 + 2;
				int swapIndex = 0;

				if (childIndexLeft < currentItemCount)
				{
					swapIndex = childIndexLeft;

					if (childIndexRight < currentItemCount)
					{
						if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
						{
							swapIndex = childIndexRight;
						}
					}

					if (item.CompareTo(items[swapIndex]) < 0)
					{
						Swap(item, items[swapIndex]);
					}
					else
					{
						return;
					}
				}
				else
				{
					return;
				}
			}
		}

		private void SortUp(T item)
		{
			int parentIndex = (item.HeapIndex - 1) / 2;

			while (true)
			{
				T parentItem = items[parentIndex];

				if (item.CompareTo(parentItem) > 0)
				{
					Swap(item, parentItem);
				}
				else
				{
					break;
				}

				parentIndex = (item.HeapIndex - 1) / 2;
			}
		}

		private void Swap(T item1, T item2)
		{
			items[item1.HeapIndex] = item2;
			items[item2.HeapIndex] = item1;

			int item1Index = item1.HeapIndex;
			item1.HeapIndex = item2.HeapIndex;
			item2.HeapIndex = item1Index;
		}

	}

	public interface IHeapItem<T> : System.IComparable<T>
	{
		int HeapIndex
		{
			get;
			set;
		}
	}
}