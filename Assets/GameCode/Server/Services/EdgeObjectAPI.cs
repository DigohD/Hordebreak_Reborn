using FNZ.Server.Controller;
using FNZ.Server.Model.Entity.Components.EdgeObject;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components.EdgeObject;
using FNZ.Shared.Model.Entity.MountedObject;
using FNZ.Shared.Utils;
using Unity.Mathematics;

namespace FNZ.Server.Services
{

	public class EdgeObjectAPI
	{
		//public FNEEntity TryNetSpawnEdgeObject(string id, float2 position, float rotation = 0)
		//{
		//	var edgeObject = TrySpawnEdgeObject(id, position, rotation);

		//	if (edgeObject != null)
		//	{
		//		GameServer.NetConnector.SyncEntity(edgeObject);
		//		GameServer.NetAPI.Entity_SpawnEntity_BAR(edgeObject);
		//	}

		//	return edgeObject;
		//}

		//public FNEEntity TrySpawnEdgeObject(string id, float2 position, float rotation = 0)
		//{
		//	if (GameServer.World.GetEdgeObjectAtPosition(position) != null)
		//	{
		//		return null;
		//	}

		//	var edgeObject = GameServer.EntityFactory.CreateEdgeObject(id, position, rotation);

		//	if (edgeObject != null)
		//	{
		//		GameServer.World.AddEdgeObject(edgeObject);

		//		foreach (var comp in edgeObject.Components)
		//		{
		//			if (comp is ITickable)
		//			{
		//				GameServer.World.AddTickableEntity(edgeObject);
		//				break;
		//			}
		//		}
		//	}

		//	// Return null if object could not be spawned
		//	return edgeObject;
		//}

		public void NetSpawnMountedObject(string id, FNEEntity edgeObject, bool oppositeDirection)
		{
			SpawnMountedObject(id, edgeObject, oppositeDirection);

			GameServer.NetAPI.Entity_UpdateComponent_BAR(edgeObject.GetComponent<EdgeObjectComponentServer>());
		}

		public void SpawnMountedObject(string id, FNEEntity edgeObject, bool oppositeDirection)
		{
			var edgeComp = edgeObject.GetComponent<EdgeObjectComponentServer>();

			edgeComp.MountedObjectData = DataBank.Instance.GetData<MountedObjectData>(id);
			edgeComp.OppositeMountedDirection = oppositeDirection;
		}

		public void NetRemoveMountedObject(FNEEntity edgeObject)
		{
			var edgeComp = edgeObject.GetComponent<EdgeObjectComponentServer>();
			edgeComp.MountedObjectData = null;
			edgeComp.OppositeMountedDirection = false;
			GameServer.NetAPI.Entity_UpdateComponent_BAR(edgeObject.GetComponent<EdgeObjectComponentServer>());
		}
	}
}