using FNZ.Client.Model.World;
using FNZ.Client.View.Audio;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.World.Atmosphere;
using FNZ.Shared.Utils;
using UnityEngine;

namespace FNZ.Client.View.Player.Atmosphere
{

	public class PlayerAtmosphereSFX : MonoBehaviour
	{
		private static FNEEntity m_Player;

		public void Init(FNEEntity player)
		{
			m_Player = player;
		}


		void Update()
		{
			if (Random.Range(0, 150) == 0)
			{
				int randomX = (int)m_Player.Position.x + Random.Range(-15, 15);
				int randomY = (int)m_Player.Position.y + Random.Range(-15, 15);

				randomX = randomX > m_Player.Position.x ? randomX + 15 : randomX - 15;
				randomY = randomY > m_Player.Position.y ? randomY + 15 : randomY - 15;

				if (GameClient.World.GetWorldChunk<ClientWorldChunk>() == null)
					return;

				var atmosphereRef = GameClient.World.GetTileData(randomX, randomY).atmosphereRef;
				if (string.IsNullOrEmpty(atmosphereRef)) return;
				var atmosphereData = DataBank.Instance.GetData<AtmosphereData>(atmosphereRef);

				var sfxList = atmosphereData.sfxList;

				var gameHour = GameClient.Environment.Hour;

				var activeThisTime = sfxList.FindAll(e =>
				{
					var endSpawnTime = e.centerTime + e.timeOffset;
					var startSpawnTime = e.centerTime - e.timeOffset;

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
							return true;
						}
					}
					// Special case #2: If starttime is less than endtime for example 12 - 18 
					else
					{
						if (gameHour >= startSpawnTime && gameHour <= endSpawnTime)
						{
							return true;
						}
					}

					return false;
				});

				int totalWeight = 0;

				foreach (var entry in activeThisTime)
				{
					totalWeight += entry.weight;
				}

				var randomWeight = FNERandom.GetRandomIntInRange(1, totalWeight + 1);

				foreach (var atmosData in activeThisTime)
				{
					if (randomWeight <= atmosData.weight)
					{
						AudioManager.Instance.PlaySfx3dClip(
							atmosData.sfxRefs[Random.Range(0, atmosData.sfxRefs.Count)],
							new Unity.Mathematics.float2(randomX, randomY)
						);
					}
					else
					{
						randomWeight -= atmosData.weight;
					}
				}
			}
		}

		public static void PlayAmbienceBasedOnTimeOfDay(byte timeOfDay)
		{
			var atmosphereRef = GameClient.World.GetTileData((int)m_Player.Position.x, (int)m_Player.Position.y).atmosphereRef;
			if (string.IsNullOrEmpty(atmosphereRef))
				return;
			var sfxList = DataBank.Instance.GetData<AtmosphereData>(atmosphereRef).sfxList;

			int timeDifference = 1000;
			int listIndexToPick = 1000;
			int absTimeDiff, timeOfInterest;

			for (int i = 0; i < sfxList.Count; i++)
			{
				timeOfInterest = sfxList[i].centerTime - sfxList[i].timeOffset < 0 ?
					24 + (sfxList[i].centerTime - sfxList[i].timeOffset) :
					sfxList[i].centerTime - sfxList[i].timeOffset;

				if (timeOfDay >= timeOfInterest)
				{
					absTimeDiff = Mathf.Abs(timeOfInterest - timeOfDay);

					if (absTimeDiff < timeDifference)
					{
						timeDifference = absTimeDiff;
						listIndexToPick = i;
					}
				}
			}

			if (listIndexToPick != 1000 && AudioManager.Instance.GetCurrentAmbienceId() != sfxList[listIndexToPick].ambience)
				AudioManager.Instance.PlayAmbience(sfxList[listIndexToPick].ambience);
		}
	}
}