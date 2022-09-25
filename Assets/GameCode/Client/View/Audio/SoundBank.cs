using Assets.GameCode.Shared.Model.GameStateMusicAmbience;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Ambience;
using FNZ.Shared.Model.Music;
using FNZ.Shared.Model.SFX;
using FNZ.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FNZ.Client.View.Audio
{
	public class SoundBank
	{
		private static Dictionary<string, List<string>> m_GameStateDict = new Dictionary<string, List<string>>();
		private static Dictionary<string, string> m_AllSFXIdToResourcePaths = new Dictionary<string, string>();
		private static Dictionary<string, string> m_AllMusicIdToResourcePaths = new Dictionary<string, string>();
		private static Dictionary<string, string> m_AllAmbienceIdToResourcePaths = new Dictionary<string, string>();
		private static Dictionary<string, float> m_MaxDistanceValues = new Dictionary<string, float>();

		private static List<string> m_CurrentMusicTracks = new List<string>();

		public const string STRING_RANDOM = "random";
		public const string STRING_MUSIC = "music";
		public const string STRING_AMBIENCE = "ambience";
		public const string STRING_DEFAULT = "default";
		private static string s_StreamingAssetsPath = Application.streamingAssetsPath + "/";

		static SoundBank()
		{
			//Sound effects
			List<SFXData> sfxList = DataBank.Instance.GetAllDataIdsOfType<SFXData>();
			foreach (var data in sfxList)
			{
				m_AllSFXIdToResourcePaths.Add(data.Id, data.filePath);
				m_MaxDistanceValues.Add(data.Id, data.distance);
			}

			//Music
			var musicDataList = DataBank.Instance.GetAllDataIdsOfType<MusicData>();
			foreach (var musicData in musicDataList)
			{
				m_AllMusicIdToResourcePaths.Add(musicData.Id, musicData.filePath);
			}

			//Ambience
			var ambDataList = DataBank.Instance.GetAllDataIdsOfType<AmbienceData>();
			foreach (var ambData in ambDataList)
			{
				m_AllAmbienceIdToResourcePaths.Add(ambData.Id, ambData.filePath);
			}

			//GameStates Music
			var gameStateMusicData = DataBank.Instance.GetAllDataIdsOfType<GameStateMusicData>();
			foreach (var gameState in gameStateMusicData)
			{
				if (!m_GameStateDict.ContainsKey(gameState.Id))
					m_GameStateDict.Add(gameState.Id, new List<string>());

				if (gameState.musicTracks != null && gameState.musicTracks.Count > 0)
				{
					foreach (var track in gameState.musicTracks)
					{
						m_GameStateDict[gameState.Id].Add(track);
					}
				}
			}

			//set baseline music
			SetCurrentMusicTracks(m_GameStateDict[STRING_DEFAULT]);
		}

		public static string GetAudioClipPath(string Id, bool isAmbience, string currentlyPlayingTrackId = "")
		{
			if (Id == STRING_RANDOM)
			{
				KeyValuePair<string, string> randomElement;

				if (currentlyPlayingTrackId != "" && !isAmbience)//If it is music
				{
					int safeguard = 0;
					do
					{
						var trackId = m_CurrentMusicTracks[FNERandom.GetRandomIntInRange(0, m_CurrentMusicTracks.Count)];
						var path = m_AllMusicIdToResourcePaths[trackId];
						randomElement = new KeyValuePair<string, string>(trackId, path);

						safeguard++;
						if (safeguard >= 5)
							break;
					} while (randomElement.Key == currentlyPlayingTrackId);

					return s_StreamingAssetsPath + m_AllMusicIdToResourcePaths[randomElement.Key];
				}
				else //It was not music
				{
					randomElement = m_AllSFXIdToResourcePaths.ElementAt(FNERandom.GetRandomIntInRange(0, m_AllSFXIdToResourcePaths.Count));
					return s_StreamingAssetsPath + m_AllSFXIdToResourcePaths[randomElement.Key];
				}
			}
			else
			{
				if (currentlyPlayingTrackId != "") //If it is music or ambience
				{
					if (isAmbience && m_AllAmbienceIdToResourcePaths.ContainsKey(Id))
						return s_StreamingAssetsPath + m_AllAmbienceIdToResourcePaths[Id];

					if (!isAmbience && m_AllMusicIdToResourcePaths.ContainsKey(Id))
						return s_StreamingAssetsPath + m_AllMusicIdToResourcePaths[Id];
				}
				else if (m_AllSFXIdToResourcePaths.ContainsKey(Id)) //It was not music or ambience
					return s_StreamingAssetsPath + m_AllSFXIdToResourcePaths[Id];

				return null;
			}
		}

		public static float GetMaxDistanceValue(string sfxId)
		{
			if (m_MaxDistanceValues.ContainsKey(sfxId))
				return m_MaxDistanceValues[sfxId];
			else
				return 0;
		}

		public static void SetCurrentGameStateMusic(string gameState)
		{
			if (m_GameStateDict.TryGetValue(gameState, out List<string> tracks))
				SetCurrentMusicTracks(tracks);
		}

		public static void SetCurrentMusicTracks(List<string> tracks)
		{
			m_CurrentMusicTracks = tracks;
		}

	}
}