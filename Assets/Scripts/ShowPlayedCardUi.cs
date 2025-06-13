using System;
using System.Threading.Tasks;
using UnityEngine;

public class ShowPlayedCardUi : MonoBehaviour
{
	public static ShowPlayedCardUi instance;

	public GameObject PlayedCardUi;
	public CardUi cardUi;

	public int TimeToDisplayCardMiliseconds {  get; private set; }

	public static event Action<CardData> OnThrowCardAfterShown;

	void Awake()
	{
		instance = this;
		TimeToDisplayCardMiliseconds = 3000;
	}

	void OnEnable()
	{
		Entity.OnCardChosen += ShowPlayedCard;
	}

	void OnDisable()
	{
		Entity.OnCardChosen -= ShowPlayedCard;
	}

	async void ShowPlayedCard(CardData data)
	{
		cardUi.SetupCard(data, false);
		PlayedCardUi.SetActive(true);

		await DisplayCard();

		OnThrowCardAfterShown?.Invoke(data);
		HidePlayedCard();
	}

	async Task DisplayCard()
	{
		await Task.Delay(TimeToDisplayCardMiliseconds);
	}


	void HidePlayedCard()
	{
		PlayedCardUi.SetActive(false);
	}
}
