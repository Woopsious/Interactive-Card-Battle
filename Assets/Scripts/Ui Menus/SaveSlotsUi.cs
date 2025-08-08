using System;
using UnityEngine;

namespace Woopsious
{
	public class SaveSlotsUi : MonoBehaviour
	{
		void OnEnable()
		{
			MainMenuUi.ShowMainMenuUi += HideSaveSlotsUi;
			MainMenuUi.ShowSaveSlotsUi += ShowSaveSlotsUi;
			MainMenuUi.ShowSettingsUi += HideSaveSlotsUi;
		}
		void OnDestroy()
		{
			MainMenuUi.ShowMainMenuUi -= HideSaveSlotsUi;
			MainMenuUi.ShowSaveSlotsUi -= ShowSaveSlotsUi;
			MainMenuUi.ShowSettingsUi -= HideSaveSlotsUi;
		}

		void ShowSaveSlotsUi()
		{

		}

		void HideSaveSlotsUi()
		{

		}
	}
}