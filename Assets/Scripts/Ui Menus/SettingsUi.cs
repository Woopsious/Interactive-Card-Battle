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

		public Button saveSettingsButton;

		void OnEnable()
		{
			SaveManager.ReloadPlayerData += ReloadPlayerData;
			MainMenuUi.ShowMainMenuUi += HideSettingsUi;
			MainMenuUi.ShowNewGameUi += HideSettingsUi;
			MainMenuUi.ShowSaveSlotsUi += HideSettingsUi;
			MainMenuUi.ShowSettingsUi += ShowSettingsUi;
			MainMenuUi.ShowDatabaseUi += HideSettingsUi;
		}
		void OnDestroy()
		{
			SaveManager.ReloadPlayerData -= ReloadPlayerData;
			MainMenuUi.ShowMainMenuUi -= HideSettingsUi;
			MainMenuUi.ShowNewGameUi -= HideSettingsUi;
			MainMenuUi.ShowSaveSlotsUi -= HideSettingsUi;
			MainMenuUi.ShowSettingsUi -= ShowSettingsUi;
			MainMenuUi.ShowDatabaseUi -= HideSettingsUi;
		}

		private void Awake()
		{
			SetUpAudio();
			saveSettingsButton.onClick.AddListener(() => SaveManager.SavePlayerData());
		}

		void SetUpAudio()
		{
			audioSlider.onValueChanged.AddListener(delegate { UpdateAudioManagerVolume(audioSlider.value); });
			SetAudioSettings(75); //set initial value
		}

		//reload player data event
		void ReloadPlayerData(PlayerData playerData)
		{
			SetAudioSettings((int)(playerData.audioVolume * 100));
		}

		//set audio levels
		void SetAudioSettings(int audioVolume)
		{
			audioSlider.value = audioVolume;
			audioText.text = "Audio Volume: " + audioVolume;
		}
		void UpdateAudioManagerVolume(float audioVolume)
		{
			audioText.text = "Audio Volume: " + audioVolume;
			AudioManager.SetAudioVolume(audioVolume / 100);
		}

		//update ui panel
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