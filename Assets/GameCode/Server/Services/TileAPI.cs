using FNZ.Shared.Model;
using FNZ.Shared.Model.World.Tile;
using Unity.Mathematics;

namespace FNZ.Server.Services
{

	public class TileAPI
	{
		public bool TryNetChangeTile(string id, float2 position)
		{
			var tileChanged = TryChangeTile(id, position);

			if (tileChanged)
			{
				GameServer.NetAPI.World_ChangeTile_BAR((int)position.x, (int)position.y, id);
			}

			return tileChanged;
		}

		public bool TryChangeTile(string id, float2 position)
		{
			var world = GameServer.World;
			var data = DataBank.Instance.GetData<TileData>(id);
			var oldTileDef = world.GetTileId((int)position.x, (int)position.y);

			if (oldTileDef == id)
				return false;

			world.ChangeTile((int)position.x, (int)position.y, data.Id);

			return true;
		}
	}
}