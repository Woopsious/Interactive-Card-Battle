using System;
using UnityEngine;

namespace Woopsious
{
	public class AudioManager : MonoBehaviour
	{
		public static AudioManager instance;

		public static float AudioVolume;

		public static Action<float> OnAudioVolumeChange;

		public static void SetAudioVolume(float audioVolume)
		{
			AudioVolume = audioVolume;
			OnAudioVolumeChange?.Invoke(audioVolume);
		}
	}
}