using UnityEngine;
using Woopsious;

namespace Woopsious
{
	public class SettingsUi : MonoBehaviour
	{
		public GameObject settingsPanel;

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