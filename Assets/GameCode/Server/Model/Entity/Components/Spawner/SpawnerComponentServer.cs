using FNZ.Shared.Model.Entity.Components.Spawner;
using FNZ.Server.Controller;
using System.Collections.Generic;
using FNZ.Shared.Net.Dto.Hordes;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;

namespace FNZ.Server.Model.Entity.Components.Spawner
{
	public class SpawnerComponentServer : SpawnerComponentShared, ITickable
	{
		public static bool runSpawn = false;

		private float timer = 0;
		List<HordeEntitySpawnData> hordeEntitySpawnDatas;

		public override void Init()
		{
			base.Init();

			hordeEntitySpawnDatas = new List<HordeEntitySpawnData>(Data.spawnAmount);
		}

		public void Tick(float dt)
		{
			// if (!runSpawn) return;
			//
			// timer += dt;
			//
			// if(timer >= Data.spawnInterval)
			// {
			// 	timer = 0;
			//
			// 	for(int i = 0; i < Data.spawnAmount; i++)
			// 	{
			// 		var e = GameServer.EntityAPI.SpawnEntityImmediate(Data.enemyRef, ParentEntity.Position);
			//
			// 		GameServer.NetConnector.SyncEntity(e);
			//
			// 		hordeEntitySpawnDatas.Add(new HordeEntitySpawnData
			// 		{
			// 			NetId = e.NetId,
			// 			EntityIdCode = IdTranslator.Instance.GetIdCode<FNEEntityData>(e.EntityId),
			// 			Position = e.Position,
			// 			Rotation = e.RotationDegrees,
			// 		});
			// 	}
			//
			// 	GameServer.NetAPI.Entity_SpawnHordeEntity_Batched_BAR(hordeEntitySpawnDatas);
			// 	hordeEntitySpawnDatas.Clear();
			// }
		}
	}
}
