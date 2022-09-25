using FNZ.Client.Utils;
using FNZ.Shared.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;

namespace FNZ.Client.View.Audio
{
	public class AudioManager : FNESingleton<AudioManager>
	{
		private GameObject[] m_3dAudioSources = new GameObject[20];
		private Vector3 offset;
		[SerializeField] private GameObject m_AudioSourcePrefab;
		[SerializeField] private AudioSource m_MusicPlayer1, m_MusicPlayer2, m_UiSfxPlayer, m_AmbiencePlayer1, m_AmbiencePlayer2;
		[SerializeField] private AudioMixer m_MasterMixer, m_MusicMixer, m_SfxMixer, m_AmbienceMixer;
		[SerializeField] private AudioMixerSnapshot m_MasterMuted, m_MasterUnmuted, m_MusicMuted, m_MusicPlayer1Plays, m_MusicPlayer2Plays, m_AmbienceMuted, m_AmbiencePlayer1Plays, m_AmbiencePlayer2Plays, m_SfxMuted, m_SfxUnmuted;

		private AnimationCurve m_LowPassCurve;
		private string m_CurrentMusicId, m_CurrentAmbienceId;
		private float m_CurrentMasterVolume, m_CurrentMusicVolume, m_CurrentSFXVolume, m_CurrentAmbienceVolume, m_MusicTrackLength, m_MaxDistance;
		private const short AUDIO_CUTOFF_LIGHT = 6000, AUDIO_CUTOFF_HEAVY = 3000;
		private const sbyte MINIMUM_VOLUME = -80;
		private const byte DEFAULT_FADE_TIMER = 3, AUDIO_OCCLUSION_LIGHT = 2, AUDIO_OCCLUSION_HEAVY = 4;
		private bool m_MusicPlayer1Playing, m_AmbiencePlayer1Playing, m_IsMusicMidFade, m_MusicCrossfadeEnabled, m_IsAmbienceMidFade, m_MasterVolumeToggle, m_AmbienceVolumeToggle, m_MusicVolumeToggle, m_SfxVolumeToggle;

		private List<GameObject> m_PlayedThisframe = new List<GameObject>();
		private Dictionary<string, AudioClip> m_LoadedSFX = new Dictionary<string, AudioClip>();
		private Dictionary<string, float> m_RecentPlayedClips = new Dictionary<string, float>();
		
		private void Awake()
		{
			if (Instance != this) Destroy(gameObject);
		}

		private void Start()
		{
			for (int i = 0; i < m_3dAudioSources.Length; i++)
			{
				m_3dAudioSources[i] = Instantiate(m_AudioSourcePrefab, transform);
			}

			m_LowPassCurve = m_3dAudioSources[0].GetComponent<AudioLowPassFilter>().customCutoffCurve;
			m_MaxDistance = DefaultValues.DEFAULT_AUDIOSOURCE_MAXDISTANCE;
			m_MusicPlayer1Playing = true;
			m_AmbiencePlayer1Playing = true;
			m_IsMusicMidFade = false;
			m_MusicCrossfadeEnabled = true;
			m_MasterVolumeToggle = true;
			m_AmbienceVolumeToggle = true;
			m_MusicVolumeToggle = true;
			m_SfxVolumeToggle = true;
			m_CurrentMasterVolume = 1;
			m_CurrentMusicVolume = 1;
			m_CurrentSFXVolume = 1;
			m_CurrentAmbienceVolume = 1;
			m_MusicTrackLength = DEFAULT_FADE_TIMER + 1; //<-- This needs to be more than DEFAULT_FADE_TIMER.
			offset = new Vector3(0, 0.5f, 0);
			DontDestroyOnLoad(this);
			PlayMusic("intro_music");
			PlayAmbience("night");
		}

		private void Update()
		{
			if (m_MusicTrackLength > 0)
				m_MusicTrackLength -= Time.deltaTime;

			//Music loop
			if (m_MusicCrossfadeEnabled && m_MusicTrackLength < DEFAULT_FADE_TIMER)
			{
				PlayMusic(SoundBank.STRING_RANDOM);
				m_MusicCrossfadeEnabled = false;
			}

			var keysToRemove = new List<string>();
			List<string> keys = m_RecentPlayedClips.Keys.ToList();
			foreach (var key in keys)
			{
				var tmp = m_RecentPlayedClips[key];
				tmp -= Time.deltaTime;
				m_RecentPlayedClips[key] = tmp;
				if (m_RecentPlayedClips[key] < 0)
				{
					keysToRemove.Add(key);
				}
			}

			foreach (var key in keysToRemove)
				m_RecentPlayedClips.Remove(key);
		}

		private void LateUpdate()
		{
			m_PlayedThisframe.Clear();
		}

		private GameObject GetAvailableSoundSource()
		{
			for (int i = 0; i < m_3dAudioSources.Length; i++)
			{
				if (m_3dAudioSources[i] != null)
				{
					if (m_3dAudioSources[i].GetComponent<AudioSource>().isPlaying)
						continue;
					else
					{
						if (m_PlayedThisframe.Contains(m_3dAudioSources[i]))
							continue;

						m_PlayedThisframe.Add(m_3dAudioSources[i]);
						return m_3dAudioSources[i];
					}

				}
				else
				{
					m_3dAudioSources[i] = Instantiate(m_AudioSourcePrefab, transform);
					break;
				}
			}

			return null;
		}

		private IEnumerator LoadAndPlay3dSound(string filePath, AudioSource source, AudioLowPassFilter lowPassFilter)
		{
			if (filePath != null)
			{
				if (m_LoadedSFX.ContainsKey(filePath))
					source.clip = m_LoadedSFX[filePath];
				else
				{
					using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.WAV);
					yield return www.SendWebRequest();

					if (www.result == UnityWebRequest.Result.ConnectionError)
					{
						source.clip = null;
					}
					else
					{
						if(!m_LoadedSFX.ContainsKey(filePath))
						{
							var audioClip = DownloadHandlerAudioClip.GetContent(www);
							source.clip = audioClip;
							m_LoadedSFX.Add(filePath, audioClip);
						} else
						{
							source.clip = m_LoadedSFX[filePath];
						}
					}
				}

				source.Play();
			
				yield return new WaitForSeconds(source.clip.length);
				lowPassFilter.customCutoffCurve = m_LowPassCurve;
			}
		}

		private IEnumerator LoadAndPlayMusicClip(string filePath, float timer)
		{
			if (filePath != null)
			{
				using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.OGGVORBIS))
				{
					((DownloadHandlerAudioClip)www.downloadHandler).streamAudio = true;

					yield return www.SendWebRequest();

					if (www.result == UnityWebRequest.Result.ConnectionError)
					{
						if (m_MusicPlayer1Playing)
							m_MusicPlayer2.clip = null;
						else
							m_MusicPlayer1.clip = null;
					}
					else
					{
						if (m_MusicPlayer1Playing)
							m_MusicPlayer2.clip = DownloadHandlerAudioClip.GetContent(www);
						else
							m_MusicPlayer1.clip = DownloadHandlerAudioClip.GetContent(www);

						MusicCrossfade(timer);
					}
				}
			}
		}

		private IEnumerator LoadAndPlayAmbienceClip(string filePath, float timer)
		{
			if (filePath != null)
			{
				using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.OGGVORBIS);
				((DownloadHandlerAudioClip)www.downloadHandler).streamAudio = true;

				yield return www.SendWebRequest();

				if (www.result == UnityWebRequest.Result.ConnectionError)
				{
					if (m_AmbiencePlayer1Playing)
						m_AmbiencePlayer2.clip = null;
					else
						m_AmbiencePlayer1.clip = null;
				}
				else
				{
					if (m_AmbiencePlayer1Playing)
						m_AmbiencePlayer2.clip = DownloadHandlerAudioClip.GetContent(www);
					else
						m_AmbiencePlayer1.clip = DownloadHandlerAudioClip.GetContent(www);

					AmbienceCrossfade(timer);
				}
			}
		}

		private IEnumerator LoadAndPlayUISound(string filePath)
		{
			if (filePath != null)
			{
				if (m_LoadedSFX.ContainsKey(filePath))
					m_UiSfxPlayer.clip = m_LoadedSFX[filePath];
				else
				{
					using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.WAV);
					yield return www.SendWebRequest();

					if (www.result == UnityWebRequest.Result.ConnectionError)
					{
						m_UiSfxPlayer.clip = null;
					}
					else
					{
						var audioClip = DownloadHandlerAudioClip.GetContent(www);
						m_UiSfxPlayer.clip = audioClip;
						if (!m_LoadedSFX.ContainsKey(filePath))
							m_LoadedSFX.Add(filePath, audioClip);
					}
				}

				m_UiSfxPlayer.Play();
			}
		}

		private void MusicCrossfade(float timer)
		{
			m_MusicMixer.GetFloat("MusicMasterVol", out float volume);
			m_IsMusicMidFade = true;
			m_MusicCrossfadeEnabled = true;

			if (m_MusicPlayer1Playing)
			{
				m_MusicTrackLength = m_MusicPlayer2.clip.length;
				m_MusicPlayer2.Play();
				if (volume > MINIMUM_VOLUME)
					m_MusicPlayer2Plays.TransitionTo(timer);
				Invoke("MusicStopPlayer1", timer);
			}
			else
			{
				m_MusicTrackLength = m_MusicPlayer1.clip.length;
				m_MusicPlayer1.Play();
				if (volume > MINIMUM_VOLUME)
					m_MusicPlayer1Plays.TransitionTo(timer);
				Invoke("MusicStopPlayer2", timer);
			}

			m_MusicPlayer1Playing = !m_MusicPlayer1Playing;
		}

		private void MusicStopPlayer1()
		{
			m_MusicPlayer1.Stop();
			if (m_MusicPlayer1.clip != null)
				m_MusicPlayer1.clip.UnloadAudioData();
			m_MusicPlayer1.clip = null;
			m_IsMusicMidFade = false;
		}

		private void MusicStopPlayer2()
		{
			m_MusicPlayer2.Stop();
			if (m_MusicPlayer2.clip != null)
				m_MusicPlayer2.clip.UnloadAudioData();
			m_MusicPlayer2.clip = null;
			m_IsMusicMidFade = false;
		}

		public void PlayMusic(string musicId, float fadeDuration = DEFAULT_FADE_TIMER)
		{
			if (!m_IsMusicMidFade)
			{
				m_CurrentMusicId = musicId;
				IEnumerator playMusicFile = LoadAndPlayMusicClip(SoundBank.GetAudioClipPath(musicId, false, m_CurrentMusicId), fadeDuration);
				StartCoroutine(playMusicFile);
			}
		}

		public string GetCurrentMusicId()
		{
			return m_CurrentMusicId;
		}

		private void AmbienceCrossfade(float timer)
		{
			m_AmbienceMixer.GetFloat("AmbienceMasterVol", out float volume);
			m_IsAmbienceMidFade = true;

			if (m_AmbiencePlayer1Playing)
			{
				m_AmbiencePlayer2.Play();
				if (volume > MINIMUM_VOLUME)
					m_AmbiencePlayer2Plays.TransitionTo(timer);
				Invoke("AmbienceStopPlayer1", timer);
			}
			else
			{
				m_AmbiencePlayer1.Play();
				if (volume > MINIMUM_VOLUME)
					m_AmbiencePlayer1Plays.TransitionTo(timer);
				Invoke("AmbienceStopPlayer2", timer);
			}

			m_AmbiencePlayer1Playing = !m_AmbiencePlayer1Playing;
		}

		private void AmbienceStopPlayer1()
		{
			m_AmbiencePlayer1.Stop();
			m_AmbiencePlayer1.clip = null;
			m_IsAmbienceMidFade = false;
		}

		private void AmbienceStopPlayer2()
		{
			m_AmbiencePlayer2.Stop();
			m_AmbiencePlayer2.clip = null;
			m_IsAmbienceMidFade = false;
		}

		public void PlayAmbience(string ambienceId, float fadeDuration = DEFAULT_FADE_TIMER)
		{
			if (!m_IsAmbienceMidFade)
			{
				m_CurrentAmbienceId = ambienceId;
				IEnumerator playAmbienceFile = LoadAndPlayAmbienceClip(SoundBank.GetAudioClipPath(ambienceId, true, m_CurrentAmbienceId), fadeDuration);
				StartCoroutine(playAmbienceFile);
			}
		}

		public string GetCurrentAmbienceId()
		{
			return m_CurrentAmbienceId;
		}

		public AudioSource PlaySfx3dClip(string sfxId, float2 position, float maxDist = 0)
		{
			if (m_RecentPlayedClips.ContainsKey(sfxId))
			{
				return null;
			}
			
			if(maxDist == 0)
				maxDist = SoundBank.GetMaxDistanceValue(sfxId);

			m_RecentPlayedClips.Add(sfxId, 1f/60f);
			
			
			var source = GetAvailableSoundSource();

			if (source != null)
			{
				var audioSource = source.GetComponent<AudioSource>();
				var lowPassFilter = source.GetComponent<AudioLowPassFilter>();

				source.transform.position = new Vector3(position.x, 0, position.y) + offset;

				var playerPosition = GameClient.LocalPlayerView.transform.position + offset;

				var obstructingObjects = Physics.RaycastAll(source.transform.position, playerPosition - source.transform.position, Vector3.Distance(source.transform.position, playerPosition));

				if (obstructingObjects.Length > AUDIO_OCCLUSION_HEAVY)
					lowPassFilter.cutoffFrequency = AUDIO_CUTOFF_HEAVY;
				else if (obstructingObjects.Length > AUDIO_OCCLUSION_LIGHT)
					lowPassFilter.cutoffFrequency = AUDIO_CUTOFF_LIGHT;

				audioSource.maxDistance = maxDist != 0 ? maxDist : m_MaxDistance;

				IEnumerator play3dSound = LoadAndPlay3dSound(SoundBank.GetAudioClipPath(sfxId, false), audioSource, lowPassFilter);
				StartCoroutine(play3dSound);

				return audioSource;
			}

			return null;
		}

		public void PlaySfxUiClip(string sfxId)
		{
			IEnumerator playSFXFile = LoadAndPlayUISound(SoundBank.GetAudioClipPath(sfxId, false));
			StartCoroutine(playSFXFile);
		}

		public void SetVolumeMaster(float value)
		{
			m_MasterMixer.SetFloat("GameAudioMasterVol", Mathf.Log10(value) * 30);
			m_CurrentMasterVolume = value;
		}

		public float GetVolumeMaster()
		{
			return m_CurrentMasterVolume;
		}

		public void SetVolumeAmbience(float value)
		{
			m_AmbienceMixer.SetFloat("AmbienceMasterVol", Mathf.Log10(value) * 30);
			m_CurrentAmbienceVolume = value;
		}

		public float GetVolumeAmbience()
		{
			return m_CurrentAmbienceVolume;
		}

		public void SetVolumeMusic(float value)
		{
			m_MusicMixer.SetFloat("MusicMasterVol", Mathf.Log10(value) * 30);
			m_CurrentMusicVolume = value;
		}

		public float GetVolumeMusic()
		{
			return m_CurrentMusicVolume;
		}

		public void SetVolumeSFX(float value)
		{
			m_SfxMixer.SetFloat("SFXMasterVol", Mathf.Log10(value) * 30);
			m_CurrentSFXVolume = value;
		}

		public float GetVolumeSfx()
		{
			return m_CurrentSFXVolume;
		}

		public void ToggleMaster(bool value)
		{
			if (value)
			{
				m_MasterUnmuted.TransitionTo(0.1f);
				m_MasterMixer.SetFloat("GameAudioMasterVol", Mathf.Log10(m_CurrentMasterVolume) * 30);
			}
			else
			{
				m_MasterMixer.ClearFloat("GameAudioMasterVol");
				m_MasterMuted.TransitionTo(0.1f);
			}

			m_MasterVolumeToggle = value;
		}

		public bool GetToggleMaster()
		{
			return m_MasterVolumeToggle;
		}

		public void ToggleAmbience(bool value)
		{
			if (value)
			{
				m_AmbienceMixer.SetFloat("AmbienceMasterVol", Mathf.Log10(m_CurrentAmbienceVolume) * 30);

				if (m_AmbiencePlayer1Playing)
					m_AmbiencePlayer1Plays.TransitionTo(0.1f);
				else
					m_AmbiencePlayer2Plays.TransitionTo(0.1f);
			}
			else
			{
				m_AmbienceMixer.ClearFloat("AmbienceMasterVol");
				m_AmbienceMuted.TransitionTo(0.1f);
			}

			m_AmbienceVolumeToggle = value;
		}

		public bool GetToggleAmbience()
		{
			return m_AmbienceVolumeToggle;
		}

		public void ToggleMusic(bool value)
		{
			if (value)
			{
				m_MusicMixer.SetFloat("MusicMasterVol", Mathf.Log10(m_CurrentMusicVolume) * 30);

				if (m_MusicPlayer1Playing)
					m_MusicPlayer1Plays.TransitionTo(0.1f);
				else
					m_MusicPlayer2Plays.TransitionTo(0.1f);

			}
			else
			{
				m_MusicMixer.ClearFloat("MusicMasterVol");
				m_MusicMuted.TransitionTo(0.1f);
			}

			m_MusicVolumeToggle = value;
		}

		public bool GetToggleMusic()
		{
			return m_MusicVolumeToggle;
		}

		public void ToggleSFX(bool value)
		{
			if (value)
			{
				m_SfxUnmuted.TransitionTo(0.1f);
				m_SfxMixer.SetFloat("SFXMasterVol", Mathf.Log10(m_CurrentSFXVolume) * 30);
			}
			else
			{
				m_SfxMixer.ClearFloat("SFXMasterVol");
				m_SfxMuted.TransitionTo(0.1f);
			}

			m_SfxVolumeToggle = value;
		}

		public bool GetToggleSfx()
		{
			return m_SfxVolumeToggle;
		}

	}
}
