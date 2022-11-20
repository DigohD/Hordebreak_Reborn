using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.World.Tile;
using FNZ.Shared.Utils;
using Unity.Mathematics;

namespace FNZ.Server.Utils
{

	public class TileTimeEffectGen
	{
		public static void GenerateTileTimeEffects(FNEEntity player)
		{
			float2 playerPos = player.Position;

			var randomTileX = FNERandom.GetRandomIntInRange(-40, 40);
			var randomTileY = FNERandom.GetRandomIntInRange(-40, 40);
			var tilePos = new int2((int)playerPos.x + randomTileX, (int)playerPos.y + randomTileY);
			var tileDataDef = GameServer.MainWorld.GetTileId(tilePos.x, tilePos.y);

			if (tileDataDef == string.Empty)
				return;

			var tileData = DataBank.Instance.GetData<TileData>(tileDataDef);
			var randomFrequency = FNERandom.GetRandomFloatInRange(0f, 100f);

			if (randomFrequency >= tileData.timeEffectFrequency)
			{
				return;
			}

			var timeTileEffects = tileData.timeEffects;

			float rotation = 0;
			int totalWeight = 0;

			foreach (var entry in timeTileEffects)
			{
				totalWeight += entry.weight;
			}

			var randomWeight = FNERandom.GetRandomIntInRange(1, totalWeight + 1);
			var gameHour = GameServer.MainWorld.Environment.Hour;

			foreach (var tile in timeTileEffects)
			{
				var endSpawnTime = tile.centerTime + tile.timeOffset;
				var startSpawnTime = tile.centerTime - tile.timeOffset;

				// Special case #1: If starttime is more than endtime for example 22 - 2
				if (startSpawnTime > endSpawnTime || endSpawnTime >= 24 || startSpawnTime < 0)
				{
					if (endSpawnTime >= 24)
					{
						endSpawnTime -= 24;
					}
					else if (startSpawnTime < 0)
					{
						startSpawnTime += 24;
					}

					if (gameHour >= startSpawnTime || gameHour <= endSpawnTime)
					{
						if (randomWeight <= tile.weight)
						{
							GameServer.NetAPI.Effect_SpawnEffect_BAR(tile.effectRef, tilePos, rotation);
						}
						else
						{
							randomWeight -= tile.weight;
						}
					}
				}
				// Special case #2: If starttime is less than endtime for example 12 - 18 
				else
				{
					if (gameHour >= startSpawnTime && gameHour <= endSpawnTime)
					{
						if (randomWeight <= tile.weight)
						{
							GameServer.NetAPI.Effect_SpawnEffect_BAR(tile.effectRef, tilePos, rotation);
						}
						else
						{
							randomWeight -= tile.weight;
						}
					}
				}

			}
		}
	}
}