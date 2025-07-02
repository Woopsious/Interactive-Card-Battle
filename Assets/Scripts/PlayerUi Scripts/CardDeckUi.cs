using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDeckUi : MonoBehaviour
{
	public static CardDeckUi instance;

	Image imageHighlight;
	Color colorGrey = new(0.3921569f, 0.3921569f, 0.3921569f, 1);

	private RectTransform cardDeckRectTransform;
	public RectTransform[] cardSlotRectTransforms = new RectTransform[5];

	public Transform movingCardsTransform;

	public List<CardUi> cards;

	bool damageCardsHidden;
	bool nonDamageCardsHidden;
	bool playerPickedUpCard;

	void Awake()
	{
		instance = this;
		cards = new List<CardUi>();
		cardDeckRectTransform = GetComponent<RectTransform>();
		imageHighlight = GetComponent<Image>();
	}

	void OnEnable()
	{
		SpawnManager.OnPlayerSpawned += SpawnInitialCards;
		TurnOrderManager.OnNewTurnEvent += OnNewTurnStart;
		PlayerEntity.HideOffensiveCards += HideMatchingCards;
		PlayerEntity.HideReplaceCardsButton += HideReplaceCardsButton;
		CardUi.OnCardReplace += ReplaceCardInDeck;
		ThrowableCard.OnCardPickUp += OnCardPicked;
	}
	void OnDisable()
	{
		SpawnManager.OnPlayerSpawned -= SpawnInitialCards;
		TurnOrderManager.OnNewTurnEvent -= OnNewTurnStart;
		PlayerEntity.HideOffensiveCards -= HideMatchingCards;
		PlayerEntity.HideReplaceCardsButton -= HideReplaceCardsButton;
		CardUi.OnCardReplace -= ReplaceCardInDeck;
		ThrowableCard.OnCardPickUp -= OnCardPicked;
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.GetComponent<CardUi>() != null)
			ThrowableCardEnter();
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if (other.GetComponent<CardUi>() != null)
			ThrowableCardExit();
	}

	void SpawnInitialCards(PlayerEntity player)
	{
		while (cards.Count < 5)
			SpawnNewPlayerCard();
	}

	//spawn card in player deck
	void SpawnNewPlayerCard()
	{
		CardUi card = SpawnManager.SpawnCard();
		card.SetupCard(SpawnManager.GetRandomCard(TurnOrderManager.Player().entityData.cards));
		AddCardToPlayerDeck(card);
		card.selectable = true;
	}

	//replace card
	public void ReplaceCardInDeck(CardUi cardToReplace)
	{
		cardToReplace.SetupCard(SpawnManager.GetRandomCard(TurnOrderManager.Player().entityData.cards));

        for (int i = 0; i < cards.Count; i++)
        {
			if (cards[i] != cardToReplace) continue;

			if (damageCardsHidden && cardToReplace.Offensive)
				HideCard(i);
			else if (nonDamageCardsHidden && !cardToReplace.Offensive)
				HideCard(i);
			else
				ShowCard(i);
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
		card.replaceCardButton.gameObject.SetActive(true);

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

	//UI
	//show/hide deck + cards event listeners
	void OnNewTurnStart(Entity entity)
	{
		if (entity.entityData.isPlayer)
		{
			damageCardsHidden = false;
			nonDamageCardsHidden = false;

			RemoveNullCardsFromPlayerDeck();
			while (cards.Count < 5)
				SpawnNewPlayerCard();
			ShowCardDeck();
		}
		else
			HideCardDeck();
	}
	void HideMatchingCards(bool hideDamageCards)
	{
		if (hideDamageCards)
		{
			Debug.LogError("hiding Damage Cards");
			damageCardsHidden = true;

			for (int i = 0; i < cards.Count; i++)
			{
				if (cards[i].Offensive)
					HideCard(i);
			}
		}
		else
		{
			Debug.LogError("hiding Non Damage Cards");
			nonDamageCardsHidden = true;

			for (int i = 0; i < cards.Count; i++)
			{
				if (!cards[i].Offensive)
					HideCard(i);
			}
		}
	}
	void HideReplaceCardsButton()
	{
		for (int i = 0; i < cards.Count; i++)
		{
			if (cards[i] == null) continue;
			cards[i].replaceCardButton.gameObject.SetActive(false);
		}
	}

	//update image border highlight
	void OnCardPicked(CardUi card)
	{
		if (card != null)
		{
			playerPickedUpCard = true;
			imageHighlight.color = Color.red;
		}
		else
		{
			playerPickedUpCard = false;
			imageHighlight.color = colorGrey;
		}
	}
	void ThrowableCardEnter()
	{
		if (!playerPickedUpCard) return;
		imageHighlight.color = Color.cyan;
	}
	void ThrowableCardExit()
	{
		if (!playerPickedUpCard) return;
		imageHighlight.color = Color.red;
	}

	//show/hide deck + all cards
	void ShowCardDeck()
	{
		cardDeckRectTransform.anchoredPosition = Vector2.zero;
		imageHighlight.color = colorGrey;

		for (int i = 0; i < cardSlotRectTransforms.Length; i++)
			ShowCard(i);
	}
	void HideCardDeck()
	{
		cardDeckRectTransform.anchoredPosition = new Vector2(0, -50);
		imageHighlight.color = colorGrey;

		for (int i = 0; i < cardSlotRectTransforms.Length; i++)
			HideCard(i);
	}

	//show/hide individual cards
	void ShowCard(int i)
	{
		if (cards[i] != null)
		{
			cards[i].selectable = true;
			cards[i].replaceCardButton.anchoredPosition = new Vector2(0, 5);
			cards[i].replaceCardButton.gameObject.SetActive(true);
		}

		switch (i)
		{
			case 0:
			cardSlotRectTransforms[i].anchoredPosition = new Vector2(-300, 31);
			cardSlotRectTransforms[i].transform.localRotation = Quaternion.Euler(0, 0, 20);
			break;
			case 1:
			cardSlotRectTransforms[i].anchoredPosition = new Vector2(-150, 66);
			cardSlotRectTransforms[i].transform.localRotation = Quaternion.Euler(0, 0, 8);
			break;
			case 2:
			cardSlotRectTransforms[i].anchoredPosition = new Vector2(0, 75);
			cardSlotRectTransforms[i].transform.localRotation = Quaternion.Euler(0, 0, 0);
			break;
			case 3:
			cardSlotRectTransforms[i].anchoredPosition = new Vector2(150, 66);
			cardSlotRectTransforms[i].transform.localRotation = Quaternion.Euler(0, 0, -8);
			break;
			case 4:
			cardSlotRectTransforms[i].anchoredPosition = new Vector2(300, 31);
			cardSlotRectTransforms[i].transform.localRotation = Quaternion.Euler(0, 0, -20);
			break;
		}
	}
	void HideCard(int i)
	{
		if (cards[i] != null)
		{
			cards[i].selectable = false;
			cards[i].replaceCardButton.anchoredPosition = new Vector2(0, 230);
		}

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
