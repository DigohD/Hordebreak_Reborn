using FNZ.Shared.Model.Entity;
using UnityEngine;

namespace FNZ.Server.FarNorthZMigrationStuff
{
	public class FF_Node : IHeapItem<FF_Node>
	{
		public byte gridCost = 1;
		public float pathDistanceCost = 65535;
		public int gridX;
		public int gridY;
		public int heapIndex;
		public Vector2 goalDirection;
		public FF_Node parent;
		public FNEEntity entityToAttackToNextNode = null;

		public FF_Node(int gridX, int gridY)
		{
			this.gridX = gridX;
			this.gridY = gridY;

			goalDirection = Vector2.zero;
		}

		public float totalCost
		{
			get
			{
				return pathDistanceCost + gridCost;
			}
		}

		public int HeapIndex
		{
			get
			{
				return heapIndex;
			}

			set
			{
				heapIndex = value;
			}
		}

		public int CompareTo(FF_Node other)
		{
			int compare = totalCost.CompareTo(other.totalCost);

			if (compare == 0)
			{
				compare = pathDistanceCost.CompareTo(other.pathDistanceCost);
			}

			return -compare;
		}

		public override bool Equals(System.Object other)
		{
			if (!(other is FF_Node))
			{
				return false;
			}
			FF_Node otherN = (FF_Node)other;
			return (this.gridX == otherN.gridX && this.gridY == otherN.gridY);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}