using System;
using UnityEngine;
using static DamageData;

public class PlayerEntity : Entity
{
	RectTransform rectTransform;

	int cardsUsedThisTurn;
	int damageCardsUsedThisTurn;
	int nonDamageCardsUsedThisTurn;
	int cardsReplacedThisTurn;

	public static event Action<bool> HideOffensiveCards;
	public static event Action HideReplaceCardsButton;

	void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
	}

	void OnEnable()
	{
		TurnOrderManager.OnNewTurnEvent += StartTurn;
		CardUi.OnCardReplace += OnReplaceCard;
	}

	void OnDisable()
	{
		TurnOrderManager.OnNewTurnEvent -= StartTurn;
		CardUi.OnCardReplace -= OnReplaceCard;
	}

	protected override void StartTurn(Entity entity)
	{
		if (entity == this)
			rectTransform.anchoredPosition = new Vector2(0, -200);
		else
			rectTransform.anchoredPosition = new Vector2(0, -325);

		base.StartTurn(entity);

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
