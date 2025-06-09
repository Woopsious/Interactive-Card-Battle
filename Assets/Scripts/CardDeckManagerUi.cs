using System.Collections.Generic;
using UnityEngine;

public class CardDeckManagerUi : MonoBehaviour
{
	public static CardDeckManagerUi instance;

	public Transform movingCardsTransform;

	public List<CardUi> cards;

	public GameObject cardPrefab;
	public GameObject[] cardSlots = new GameObject[5];

	private void Awake()
	{
		instance = this;
		cards = new List<CardUi>();
		SpawnNewCard();
	}

	//card creation
	bool ShouldSpawnNewCard()
	{
		if (cards.Count < 5)
			return true;
		else
			return false;
	}
	void SpawnNewCard()
	{
		CardUi card = Instantiate(cardPrefab).GetComponent<CardUi>();
		cards.Add(card);
		AddCardToDeck(card);

		if (ShouldSpawnNewCard())
			SpawnNewCard();
	}

	public void AddCardToDeck(CardUi card)
	{
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
