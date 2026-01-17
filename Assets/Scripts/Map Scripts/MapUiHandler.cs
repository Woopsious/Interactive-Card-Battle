using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Woopsious.MapGlobalModifiers;

namespace Woopsious
{
	public class MapUiHandler : MonoBehaviour
	{
		[Header("Map Ui")]
		public GameObject mapDifficultyUi;
		public TMP_Text mapDifficultyText;
		public GameObject mapModifiersUi;
		public TMP_Text mapModifiersText;

		[Header("Map Controller Ui Panel")]
		public GameObject mapControllerUi;

		private void OnEnable()
		{
			GameManager.OnGameStateChange += OnGameStateChange;
			MapController.OnMapDifficultyChange += UpdateWorldDifficultyText;
			MapController.OnMapModifiersChange += UpdateWorldModifiersText;
		}
		private void OnDisable()
		{
			GameManager.OnGameStateChange -= OnGameStateChange;
			MapController.OnMapDifficultyChange -= UpdateWorldDifficultyText;
			MapController.OnMapModifiersChange -= UpdateWorldModifiersText;
		}

		private void OnGameStateChange(GameManager.GameState gameState)
		{
			if (GameManager.GameState.MapView == gameState)
				ToggleMapUi(true);
			else
				ToggleMapUi(false);
		}
		private void ToggleMapUi(bool toggle)
		{
			mapControllerUi.SetActive(toggle);
			mapDifficultyUi.SetActive(toggle);
			mapModifiersUi.SetActive(toggle);
		}

		private void UpdateWorldDifficultyText(int difficulty)
		{
			mapDifficultyText.text = $"World Difficulty: {difficulty}";
		}
		private void UpdateWorldModifiersText(List<WorldModifer> worldModifers)
		{
			string modiferTextInfo = "Modifiers\n";

			foreach (WorldModifer worldModifer in worldModifers)
			{
				if (!worldModifer.ModifierActive) continue;

				if (worldModifer.modiferValue >= 0)
				{
					modiferTextInfo += $"+{worldModifer.modiferValue * 100}% {worldModifer.statToEffect.nameDescriptor}\n";
				}
				else
				{
					modiferTextInfo += $"{worldModifer.modiferValue * 100}% {worldModifer.statToEffect.nameDescriptor}\n";
				}
			}
			mapModifiersText.text = modiferTextInfo;
		}
	}
}
