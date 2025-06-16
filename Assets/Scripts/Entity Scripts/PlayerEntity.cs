using System;
using UnityEngine;
using static DamageData;

public class PlayerEntity : Entity
{
	RectTransform rectTransform;

	int cardsUsedThisTurn;
	int damageCardsUsedThisTurn;
	int nonDamageCardsUsedThisTurn;

	public static event Action<bool> HideCardsWithDamageType;

	void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
	}

	void OnEnable()
	{
		TurnOrderManager.OnNewTurnEvent += StartTurn;
	}

	void OnDisable()
	{
		TurnOrderManager.OnNewTurnEvent -= StartTurn;
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
	}

	public void UpdateCardsUsed(DamageType damageType)
	{
		cardsUsedThisTurn++;

		if (damageType == DamageType.block || damageType == DamageType.heal)
			nonDamageCardsUsedThisTurn++;
		else if (damageType == DamageType.physical)
			damageCardsUsedThisTurn++;
		else
		{
			Debug.LogError("card damage type has no matches");
			return;
		}

		if (cardsUsedThisTurn == entityData.maxCardsUsedPerTurn)
		{
			EndTurn();
			return;
		}

		if (damageCardsUsedThisTurn == entityData.maxDamageCardsUsedPerTurn)
		{
			HideCardsWithDamageType?.Invoke(true);
		}
		if (nonDamageCardsUsedThisTurn == entityData.maxNonDamageCardsUsedPerTurn)
		{
			HideCardsWithDamageType?.Invoke(false);
		}
	}

}
