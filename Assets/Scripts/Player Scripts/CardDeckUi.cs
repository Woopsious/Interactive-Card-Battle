using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.GPUSort;

public class CardDeckManagerUi : MonoBehaviour
{
	public static CardDeckManagerUi instance;

	public Transform movingCardsTransform;

	public List<CardUi> cards;

	private RectTransform cardDeckRectTransform;
	public RectTransform[] cardSlotRectTransforms = new RectTransform[5];

	void Awake()
	{
		instance = this;
		cards = new List<CardUi>();
		cardDeckRectTransform = GetComponent<RectTransform>();
	}

	void OnEnable()
	{
		SpawnManager.OnPlayerSpawned += SpawnInitialCards;
		TurnOrderManager.OnNewTurnEvent += OnNewTurnStart;
	}
	void OnDisable()
	{
		SpawnManager.OnPlayerSpawned -= SpawnInitialCards;
		TurnOrderManager.OnNewTurnEvent -= OnNewTurnStart;
	}

	void SpawnInitialCards(PlayerEntity player)
	{
		Debug.LogError("player spawned");

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
		cardDeckRectTransform.anchoredPosition = Vector2.zero;

		for (int i = 0; i < cardSlotRectTransforms.Length; i++)
		{
			cards[i].selectable = true;

			switch (i)
			{
				case 0:
				cardSlotRectTransforms[i].anchoredPosition = new Vector2(-300, 40);
				cardSlotRectTransforms[i].transform.localRotation = Quaternion.Euler(0, 0, 15);
				break;
				case 1:
				cardSlotRectTransforms[i].anchoredPosition = new Vector2(-150, 70);
				cardSlotRectTransforms[i].transform.localRotation = Quaternion.Euler(0, 0, 5);
				break;
				case 2:
				cardSlotRectTransforms[i].anchoredPosition = new Vector2(0, 75);
				cardSlotRectTransforms[i].transform.localRotation = Quaternion.Euler(0, 0, 0);
				break;
				case 3:
				cardSlotRectTransforms[i].anchoredPosition = new Vector2(150, 70);
				cardSlotRectTransforms[i].transform.localRotation = Quaternion.Euler(0, 0, -5);
				break;
				case 4:
				cardSlotRectTransforms[i].anchoredPosition = new Vector2(300, 40);
				cardSlotRectTransforms[i].transform.localRotation = Quaternion.Euler(0, 0, -15);
				break;
			}
		}
	}
	void HideAndDisableCards()
	{
		cardDeckRectTransform.anchoredPosition = new Vector2(0, -50);

		for (int i = 0; i < cardSlotRectTransforms.Length; i++)
		{
			cards[i].selectable = false;
			cardSlotRectTransforms[i].transform.localRotation = Quaternion.Euler(0, 0, 0);

			switch (i)
			{
				case 0:
				cardSlotRectTransforms[i].anchoredPosition = new Vector2(-300, -50);
				break;
				case 1:
				cardSlotRectTransforms[i].anchoredPosition = new Vector2(-150, -50);
				break;
				case 2:
				cardSlotRectTransforms[i].anchoredPosition = new Vector2(0, -50);
				break;
				case 3:
				cardSlotRectTransforms[i].anchoredPosition = new Vector2(150, -50);
				break;
				case 4:
				cardSlotRectTransforms[i].anchoredPosition = new Vector2(300, -50);
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
		if (!cards.Contains(card))
			cards.Add(card);

		foreach (RectTransform cardSlotRectTransform in cardSlotRectTransforms)
		{
			if (cardSlotRectTransform.transform.childCount == 1)
				continue;
			else if (cardSlotRectTransform.transform.childCount == 0)
			{
				card.transform.SetParent(cardSlotRectTransform.transform, false);
				card.transform.localPosition = Vector3.zero;
				break;
			}
			else
				Debug.LogError("child count of card slots incorrect: " + cardSlotRectTransform.transform.childCount);
		}
	}
}
