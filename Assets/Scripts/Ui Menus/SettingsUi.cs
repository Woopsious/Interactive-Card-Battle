using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Woopsious;

namespace Woopsious
{
	public class SettingsUi : MonoBehaviour
	{
		public GameObject settingsPanel;

		public TMP_Text audioText;
		public Slider audioSlider;

		void OnEnable()
		{
			MainMenuUi.ShowMainMenuUi += HideSettingsUi;
			MainMenuUi.ShowNewGameUi += HideSettingsUi;
			MainMenuUi.ShowSaveSlotsUi += HideSettingsUi;
			MainMenuUi.ShowSettingsUi += ShowSettingsUi;
		}
		void OnDestroy()
		{
			MainMenuUi.ShowMainMenuUi -= HideSettingsUi;
			MainMenuUi.ShowNewGameUi -= HideSettingsUi;
			MainMenuUi.ShowSaveSlotsUi -= HideSettingsUi;
			MainMenuUi.ShowSettingsUi -= ShowSettingsUi;
		}

		private void Awake()
		{
			SetUpAudio();
		}

		void SetUpAudio()
		{
			audioSlider.onValueChanged.AddListener(delegate { UpdateAudioVolume(audioSlider.value); });
			SetAudioSettings(0.75f); //set initial value
		}
		void SetAudioSettings(float audioVolume) //will eventually sub to load player data event to restore player set audio volume
		{
			audioSlider.value = audioVolume;
			audioText.text = "Audio Volume: " + audioVolume * 100;
		}

		void UpdateAudioVolume(float audioVolume)
		{
			audioText.text = "Audio Volume: " + audioVolume * 100;
			AudioManager.SetAudioVolume(audioVolume);
		}

		void ShowSettingsUi()
		{
			settingsPanel.SetActive(true);
			MainMenuUi.Instance.backButton.gameObject.transform.SetParent(settingsPanel.transform);
		}

		void HideSettingsUi()
		{
			settingsPanel.SetActive(false);
		}
	}
}