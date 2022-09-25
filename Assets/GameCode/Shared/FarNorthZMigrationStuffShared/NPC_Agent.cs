using FNZ.Shared.Model.Entity;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Shared.FarNorthZMigrationStuff
{
	public class NPC_Agent
	{
		public bool active = true;
		public bool isAttacking = false;
		public FNEEntity entity;
		public FNEEntity target;
		public int agentID;
		public float baseSpeed;
		public float speed;
		public Vector2 position;
		public Vector2 lastUpdatedPos;
		public Vector2 desiredVelocity = Vector2.zero;
		public Vector2 velocity;
		public long attackStartTimeStamp;
		public int2 currentTile = new int2(4080, 4080);
		//public Vector2 target;

	}
}