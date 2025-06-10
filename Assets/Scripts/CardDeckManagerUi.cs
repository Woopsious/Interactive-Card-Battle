using System.Collections.Generic;
using UnityEngine;

public class CardDeckManagerUi : MonoBehaviour
{
	public static CardDeckManagerUi instance;

	public Transform movingCardsTransform;

	public List<CardUi> cards;

	public GameObject cardPrefab;
	public GameObject[] cardSlots = new GameObject[5];

	public CardData[] cardDataList;

	private void Awake()
	{
		instance = this;
		cards = new List<CardUi>();
		SpawnNewPlayerCard();
	}

	//card creation
	void SpawnNewPlayerCard()
	{
		CardUi card = Instantiate(cardPrefab).GetComponent<CardUi>();
		AddCardToPlayerDeck(card);
		card.SetupCard(SetRandomCardData(), true);

		if (ShouldSpawnNewPlayerCard())
			SpawnNewPlayerCard();
	}
	bool ShouldSpawnNewPlayerCard()
	{
		if (cards.Count < 5)
			return true;
		else
			return false;
	}

	CardData SetRandomCardData()
	{
		CardData cardData = cardDataList[Random.Range(0, cardDataList.Length)];
		return cardData;
	}

	//add/remove cards from player deck
	public void RemoveCardFromPlayerDeck(CardUi card)
	{
		cards.Remove(card);
		SpawnNewPlayerCard();
	}
	public void AddCardToPlayerDeck(CardUi card)
	{
		cards.Add(card);

		foreach (GameObject cardSlot in cardSlots)
		{
			if (cardSlot.transform.childCount == 1)
				continue;
			else if (cardSlot.transform.childCount == 0)
			{
				card.transform.SetParent(cardSlot.transform, false);
				card.transform.localPosition = Vector3.zero;
				break;
			}
			else
				Debug.LogError("child count of card slots incorrect: " + cardSlot.transform.childCount);
		}
	}
}
