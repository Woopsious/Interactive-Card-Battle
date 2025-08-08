using UnityEngine;
using Woopsious;

namespace Woopsious
{
	public class SettingsUi : MonoBehaviour
	{
		void OnEnable()
		{
			MainMenuUi.ShowMainMenuUi += HideSettingsUi;
			MainMenuUi.ShowSaveSlotsUi += HideSettingsUi;
			MainMenuUi.ShowSettingsUi += ShowSettingsUi;
		}
		void OnDestroy()
		{
			MainMenuUi.ShowMainMenuUi -= HideSettingsUi;
			MainMenuUi.ShowSaveSlotsUi -= HideSettingsUi;
			MainMenuUi.ShowSettingsUi -= ShowSettingsUi;
		}

		void ShowSettingsUi()
		{

		}

		void HideSettingsUi()
		{

		}
	}
}