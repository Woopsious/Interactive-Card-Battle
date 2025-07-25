using UnityEngine;
using Woopsious;

public class MapUiHandler : MonoBehaviour
{
	[Header("Interactive Map Ui Panel")]
	public GameObject mapUi;

	void OnEnable()
	{
		GameManager.OnShowMapEvent += ShowMap;
		GameManager.OnStartCardCombatUiEvent += HideMap;
	}
	void OnDisable()
	{
		GameManager.OnShowMapEvent -= ShowMap;
		GameManager.OnStartCardCombatUiEvent -= HideMap;
	}

	void ShowMap()
	{
		mapUi.SetActive(true);
	}

	void HideMap()
	{
		mapUi.SetActive(false);
	}
}
