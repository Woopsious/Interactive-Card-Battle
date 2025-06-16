using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static DamageData;

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
		PlayerEntity.HideCardsWithDamageType += HideMatchingCardDamageTypes;
	}
	void OnDisable()
	{
		SpawnManager.OnPlayerSpawned -= SpawnInitialCards;
		TurnOrderManager.OnNewTurnEvent -= OnNewTurnStart;
		PlayerEntity.HideCardsWithDamageType -= HideMatchingCardDamageTypes;
	}

	void SpawnInitialCards(PlayerEntity player)
	{
		while (cards.Count < 5)
			SpawnNewPlayerCard();
	}

	//spawn card in player deck
	void SpawnNewPlayerCard()
	{
		CardUi card = GameManager.instance.SpawnCard();
		card.SetupCard(GameManager.instance.GetRandomCard(TurnOrderManager.Instance.playerEntity.entityData.cards));
		AddCardToPlayerDeck(card);
		card.selectable = true;
	}

	//show/hide deck + cards event listeners
	void OnNewTurnStart(Entity entity)
	{
		if (entity.entityData.isPlayer)
		{
			RemoveNullCardsFromPlayerDeck();
			while (cards.Count < 5)
				SpawnNewPlayerCard();
			ShowCardDeck();
		}
		else
			HideCardDeck();
	}
	void HideMatchingCardDamageTypes(bool hideDamageCards)
	{
		Debug.LogError("hide damage cards:" + hideDamageCards);

		for (int i = 0; i < cards.Count; i++)
		{
			if (hideDamageCards)
			{
				Debug.LogError("card damage type: " + cards[i].DamageType);

				if (cards[i].DamageType == DamageType.physical)
					HideCard(i);
			}
			else
			{
				Debug.LogError("card damage type: " + cards[i].DamageType);

				if (cards[i].DamageType == DamageType.block || cards[i].DamageType == DamageType.heal)
					HideCard(i);
			}
		}
	}

	//show/hide deck + all cards
	void ShowCardDeck()
	{
		cardDeckRectTransform.anchoredPosition = Vector2.zero;

		for (int i = 0; i < cardSlotRectTransforms.Length; i++)
		{
			ShowCard(i);
		}
	}
	void HideCardDeck()
	{
		cardDeckRectTransform.anchoredPosition = Vector2.zero;

		for (int i = 0; i < cardSlotRectTransforms.Length; i++)
			HideCard(i);
	}

	//show/hide individual cards
	void ShowCard(int i)
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
	void HideCard(int i)
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

	//add/remove cards from player deck
	public void RemoveNullCardsFromPlayerDeck()
	{
		for (int i = cards.Count - 1; i >= 0; i--)
		{
			if (cards[i] == null)
				cards.RemoveAt(i);
		}
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
