using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.GPUSort;

public class CardDeckManagerUi : MonoBehaviour
{
	public static CardDeckManagerUi instance;

	public Transform movingCardsTransform;

	public List<CardUi> cards;
	public GameObject[] cardSlots = new GameObject[5];

	void Awake()
	{
		instance = this;
		cards = new List<CardUi>();
	}
	void Start()
	{
		SpawnInitialCards();
	}

	void OnEnable()
	{
		TurnOrderManager.OnNewTurnEvent += OnNewTurnStart;
	}
	void OnDisable()
	{
		TurnOrderManager.OnNewTurnEvent -= OnNewTurnStart;
	}

	void SpawnInitialCards()
	{
		while (cards.Count < 5)
			SpawnNewPlayerCard(true);
	}

	//spawn card in player deck
	void SpawnNewPlayerCard(bool makeSelectable)
	{
		CardUi card = GameManager.instance.SpawnCard();
		card.SetupCard(GameManager.instance.GetRandomCard(TurnOrderManager.Instance.playerEntity.entityData.cards), true);
		AddCardToPlayerDeck(card);
		card.selectable = makeSelectable;
	}

	//show/hide cards on turn start
	void OnNewTurnStart(Entity entity)
	{
		if (entity.entityData.isPlayer)
			ShowAndEnableCards();
		else
			HideAndDisableCards();
	}
	void ShowAndEnableCards()
	{
		for (int i = 0; i < cardSlots.Length; i++)
		{
			cards[i].selectable = true;

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
	void HideAndDisableCards()
	{
		for (int i = 0; i < cardSlots.Length; i++)
		{
			cards[i].selectable = false;
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

	//add/remove cards from player deck
	public void RemoveCardFromPlayerDeck(CardUi card)
	{
		cards.Remove(card);
		SpawnNewPlayerCard(false);
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
