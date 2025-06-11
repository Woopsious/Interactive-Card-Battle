using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

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

	void OnEnable()
	{
		TurnOrderManager.OnNewTurnEvent += OnNewTurnStart;
	}
	void OnDisable()
	{
		TurnOrderManager.OnNewTurnEvent -= OnNewTurnStart;
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

	//show/hide cards on turn start
	void OnNewTurnStart(Entity entity)
	{
		if (entity.entityData.isPlayer)
			ShowCards();
		else
			HideCards();
	}
	void ShowCards()
	{
		for (int i = 0; i < cardSlots.Length; i++)
		{
			switch (i)
			{
				case 0:
				cardSlots[i].transform.localPosition = new Vector3(-300, 40 + 90, 0);
				cardSlots[i].transform.localRotation = Quaternion.Euler(0, 0, 15);
				break;
				case 1:
				cardSlots[i].transform.localPosition = new Vector3(-150, 70 + 90, 0);
				cardSlots[i].transform.localRotation = Quaternion.Euler(0, 0, 5);
				break;
				case 2:
				cardSlots[i].transform.localPosition = new Vector3(0, 75 + 90, 0);
				cardSlots[i].transform.localRotation = Quaternion.Euler(0, 0, 0);
				break;
				case 3:
				cardSlots[i].transform.localPosition = new Vector3(150, 70 + 90, 0);
				cardSlots[i].transform.localRotation = Quaternion.Euler(0, 0, -5);
				break;
				case 4:
				cardSlots[i].transform.localPosition = new Vector3(300, 40 + 90, 0);
				cardSlots[i].transform.localRotation = Quaternion.Euler(0, 0, -15);
				break;
			}
		}
	}
	void HideCards()
	{
		for (int i = 0; i < cardSlots.Length; i++)
		{
			cardSlots[i].transform.localRotation = Quaternion.Euler(0, 0, 0);

			switch (i)
			{
				case 0:
				cardSlots[i].transform.localPosition = new Vector3(-300, 0, 0);
				break;
				case 1:
				cardSlots[i].transform.localPosition = new Vector3(-150, 0, 0);
				break;
				case 2:
				cardSlots[i].transform.localPosition = new Vector3(0, 0, 0);
				break;
				case 3:
				cardSlots[i].transform.localPosition = new Vector3(150,0, 0);
				break;
				case 4:
				cardSlots[i].transform.localPosition = new Vector3(300, 0, 0);
				break;
			}
		}
	}

	/// <summary>
	/// also need code to disable interacting with cards on the deck
	/// </summary>

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
