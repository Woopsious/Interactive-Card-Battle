using System;
using UnityEngine;
using UnityEngine.UI;

namespace Woopsious
{
	public class PlayerEntity : Entity
{
	RectTransform rectTransform;
	Image imageHighlight;

	Color colorDarkGreen = new(0, 0.3921569f, 0, 1);

	int cardsUsedThisTurn;
	int damageCardsUsedThisTurn;
	int nonDamageCardsUsedThisTurn;
	int cardsReplacedThisTurn;

	public static event Action<bool> HideOffensiveCards;
	public static event Action HideReplaceCardsButton;

	void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
		imageHighlight = GetComponent<Image>();
	}

	void OnEnable()
	{
		TurnOrderManager.OnNewTurnEvent += StartTurn;
		CardUi.OnCardReplace += OnReplaceCard;
		ThrowableCard.OnCardPickUp += OnCardPicked;
	}
	void OnDisable()
	{
		TurnOrderManager.OnNewTurnEvent -= StartTurn;
		CardUi.OnCardReplace -= OnReplaceCard;
		ThrowableCard.OnCardPickUp -= OnCardPicked;
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.GetComponent<CardUi>() != null)
			ThrowableCardEnter(other.GetComponent<CardUi>());
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if (other.GetComponent<CardUi>() != null)
			ThrowableCardExit(other.GetComponent<CardUi>());
	}

	protected override void StartTurn(Entity entity)
	{
		if (entity == this)
			rectTransform.anchoredPosition = new Vector2(0, -190);
		else
			rectTransform.anchoredPosition = new Vector2(0, -325);

		base.StartTurn(entity);
		imageHighlight.color = colorDarkGreen;

		cardsUsedThisTurn = 0;
		damageCardsUsedThisTurn = 0;
		nonDamageCardsUsedThisTurn = 0;
		cardsReplacedThisTurn = 0;
	}

	public void UpdateCardsUsed(bool offensiveCard)
	{
		cardsUsedThisTurn++;

		if (offensiveCard)
			damageCardsUsedThisTurn++;
		else
			nonDamageCardsUsedThisTurn++;

		if (cardsUsedThisTurn == entityData.maxCardsUsedPerTurn)
		{
			EndTurn();
			return;
		}

		if (damageCardsUsedThisTurn == entityData.maxDamageCardsUsedPerTurn)
		{
			HideOffensiveCards?.Invoke(true);
		}
		if (nonDamageCardsUsedThisTurn == entityData.maxNonDamageCardsUsedPerTurn)
		{
			HideOffensiveCards?.Invoke(false);
		}
	}

	//update image border highlight
	void OnCardPicked(CardUi card)
	{
		if (card != null)
		{
			if (!card.Offensive)
				imageHighlight.color = Color.red;
			else
				imageHighlight.color = colorDarkGreen;
		}
		else
			imageHighlight.color = colorDarkGreen;
	}
	void ThrowableCardEnter(CardUi card)
	{
		if (!card.Offensive)
			imageHighlight.color = Color.cyan;
		else
			imageHighlight.color = colorDarkGreen;
	}
	void ThrowableCardExit(CardUi card)
	{
		if (card.Offensive)
		{
			if (card.throwableCard.PlayerRef != null)
				imageHighlight.color = Color.red;
			else
				imageHighlight.color = colorDarkGreen;
		}
		else
			imageHighlight.color = colorDarkGreen;
	}

	void OnReplaceCard(CardUi card)
	{
		cardsReplacedThisTurn++;

		if (HasReachedReplaceCardsLimit())
			HideReplaceCardsButton?.Invoke();
	}
	public bool HasReachedReplaceCardsLimit()
	{
		if (cardsReplacedThisTurn >= entityData.maxReplaceableCardsPerTurn)
			return true;
		else
			return false;
	}
}
}
