using System;
using UnityEngine;

namespace Woopsious
{
	public class SaveSlotsUi : MonoBehaviour
	{
		public GameObject saveSlotsPanel;

		void OnEnable()
		{
			MainMenuUi.ShowMainMenuUi += HideSaveSlotsUi;
			MainMenuUi.ShowNewGameUi += HideSaveSlotsUi;
			MainMenuUi.ShowSaveSlotsUi += ShowSaveSlotsUi;
			MainMenuUi.ShowSettingsUi += HideSaveSlotsUi;
			MainMenuUi.ShowDatabaseUi += HideSaveSlotsUi;
		}
		void OnDestroy()
		{
			MainMenuUi.ShowMainMenuUi -= HideSaveSlotsUi;
			MainMenuUi.ShowNewGameUi -= HideSaveSlotsUi;
			MainMenuUi.ShowSaveSlotsUi -= ShowSaveSlotsUi;
			MainMenuUi.ShowSettingsUi -= HideSaveSlotsUi;
			MainMenuUi.ShowDatabaseUi -= HideSaveSlotsUi;
		}

		void ShowSaveSlotsUi()
		{
			saveSlotsPanel.SetActive(true);
			MainMenuUi.Instance.backButton.gameObject.transform.SetParent(saveSlotsPanel.transform);
		}

		void HideSaveSlotsUi()
		{
			saveSlotsPanel.SetActive(false);
		}
	}
}