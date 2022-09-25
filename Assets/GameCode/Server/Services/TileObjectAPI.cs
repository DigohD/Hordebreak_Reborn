using FNZ.Server.Controller;
using FNZ.Shared.Model.Entity;
using Unity.Mathematics;

namespace FNZ.Server.Services
{
	public class TileObjectAPI
	{
		//public FNEEntity TryNetSpawnTileObject(string id, float2 position, float rotation = 0)
		//{
		//	var tileObject = TrySpawnTileObject(id, position, rotation);

		//	if (tileObject != null)
		//	{
		//		GameServer.NetConnector.SyncEntity(tileObject);
		//		GameServer.NetAPI.Entity_SpawnEntity_BAR(tileObject);
		//	}

		//	return tileObject;
		//}

		//public FNEEntity TrySpawnTileObject(string id, float2 position, float rotation = 0)
		//{
		//	var tileObject = GameServer.EntityFactory.CreateTileObject(id, position, rotation);

		//	if (tileObject != null)
		//	{
		//		GameServer.World.AddTileObject(tileObject);

		//		foreach (var comp in tileObject.Components)
		//		{
		//			if (comp is ITickable)
		//			{
		//				GameServer.World.AddTickableEntity(tileObject);
		//				break;
		//			}
		//		}
		//	}

		//	// Return null if object could not be spawned
		//	return tileObject;
		//}

		public FNEEntity TryDestroyTileObject(int2 pos)
		{
			// Code for destroying a tile object in the game

			// Return null if object could not be destroyed
			return null;
		}

		public FNEEntity TryMoveTileObject(int2 originPos, int2 targetPos)
		{
			// Code for spawning a tile object in the game

			// Return null if object could not be moved
			return null;
		}

		public void DestroyTileObjectsInRadius(int2 centerPos, int radius)
		{
			// Code for destroying tile objects in a radius from center point
		}
	}
}