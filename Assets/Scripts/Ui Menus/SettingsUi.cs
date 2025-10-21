using UnityEngine;
using UnityEngine.UI;

namespace Woopsious
{
	public class SettingsUi : MonoBehaviour
	{
		public GameObject settingsPanel;

		public Button saveSettingsButton;

		void OnEnable()
		{
			MainMenuUi.ShowMainMenuUi += HideSettingsUi;
			MainMenuUi.ShowNewGameUi += HideSettingsUi;
			MainMenuUi.ShowSaveSlotsUi += HideSettingsUi;
			MainMenuUi.ShowSettingsUi += ShowSettingsUi;
			MainMenuUi.ShowDatabaseUi += HideSettingsUi;
		}
		void OnDestroy()
		{
			MainMenuUi.ShowMainMenuUi -= HideSettingsUi;
			MainMenuUi.ShowNewGameUi -= HideSettingsUi;
			MainMenuUi.ShowSaveSlotsUi -= HideSettingsUi;
			MainMenuUi.ShowSettingsUi -= ShowSettingsUi;
			MainMenuUi.ShowDatabaseUi -= HideSettingsUi;
		}

		private void Awake()
		{
			saveSettingsButton.onClick.AddListener(() => SaveManager.SavePlayerData());
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