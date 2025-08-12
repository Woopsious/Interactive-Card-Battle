using UnityEngine;

namespace Woopsious
{
	public class AudioHandler : MonoBehaviour
	{
		public AudioType audioType;
		public enum AudioType
		{
			music, menuSfx, ambience, sfx
		}

		AudioSource audioSource;

		void Awake()
		{
			audioSource = GetComponent<AudioSource>();
		}

		void OnEnable()
		{
			AudioManager.OnAudioVolumeChange += UpdateAudioVolume;
			UpdateAudioVolume(AudioManager.AudioVolume);
		}
		void OnDisable()
		{
			AudioManager.OnAudioVolumeChange -= UpdateAudioVolume;
		}

		public void PlayAudio(AudioClip clip, bool overwriteAudio)
		{
			if (clip == null) return;
			if (!gameObject.activeInHierarchy) return;

			if (IsAudioPlaying())
			{
				if (overwriteAudio)
					audioSource.Stop();
				else return;
			}

			audioSource.clip = clip;
			audioSource.Play();
		}
		public void PlayAudio(AudioClip clip, AudioType audioType, bool overwriteAudio)
		{
			if (clip == null) return;
			if (!gameObject.activeInHierarchy) return;

			if (IsAudioPlaying())
			{
				if (overwriteAudio)
					audioSource.Stop();
				else return;
			}

			audioSource.clip = clip;
			audioSource.Play();
		}
		bool IsAudioPlaying()
		{
			if (audioSource.isPlaying)
				return true;
			else return false;
		}

		//audio event listener
		void UpdateAudioVolume(float audioVolume)
		{
			audioSource.volume = audioVolume;
		}
	}
}