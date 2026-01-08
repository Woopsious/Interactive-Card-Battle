using UnityEngine;
using Woopsious;

public class MapUiHandler : MonoBehaviour
{
	[Header("Interactive Map Ui Panel")]
	public GameObject mapUi;

	private void OnEnable()
	{
		GameManager.OnGameStateChange += ShowHideMapUi;
	}
	private void OnDisable()
	{
		GameManager.OnGameStateChange -= ShowHideMapUi;
	}

	private void ShowHideMapUi(GameManager.GameState gameState)
	{
		if (GameManager.GameState.MapView == gameState)
			mapUi.SetActive(true);
		else
			mapUi.SetActive(false);
	}
}
