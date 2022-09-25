using FNZ.Server.FarNorthZMigrationStuff;
using FNZ.Server.Model.Entity.Components.EdgeObject;
using FNZ.Server.Model.World;
using FNZ.Server.Utils;
using FNZ.Shared.Model.Entity.Components.Door;
using Lidgren.Network;

namespace FNZ.Server.Model.Entity.Components.Door
{
	public class DoorComponentServer : DoorComponentShared
	{
		private void NE_Receive_DoorInteract(NetIncomingMessage incMsg)
		{
			// TODO: handle agent simulation obstacles here
			if (IsOpen)
			{
				CloseDoor();
			}else if (!IsOpen)
			{
				OpenDoor();
			}
		}

		public override void OnNetEvent(NetIncomingMessage incMsg)
		{
			base.OnNetEvent(incMsg);

			switch ((DoorNetEvent)incMsg.ReadByte())
			{
				case DoorNetEvent.DOOR_INTERACT:
					NE_Receive_DoorInteract(incMsg);
					break;
			}
		}

		public void OpenDoor()
		{
			IsOpen = true;
			var eoComp = ParentEntity.GetComponent<EdgeObjectComponentServer>();
			
			eoComp.IsSeethrough = true;
			eoComp.IsPassable = true;
			eoComp.IsHittable = false;
			
			GameServer.World.QueueObstacleForUpdate(new UpdateObstacleData
			{
				Entity = ParentEntity,
				ShouldRemove = true
			});
			
			GameServer.NetAPI.Entity_UpdateComponent_BAR(this);
		}
		
		public void CloseDoor()
		{
			IsOpen = false;
			var eoComp = ParentEntity.GetComponent<EdgeObjectComponentServer>();
			
			eoComp.IsSeethrough = false;
			eoComp.IsPassable = false;
			eoComp.IsHittable = true;
			
			GameServer.World.QueueObstacleForUpdate(new UpdateObstacleData
			{
				Entity = ParentEntity,
				ShouldRemove = false
			});
			
			GameServer.NetAPI.Entity_UpdateComponent_BAR(this);
		}
	}
}
