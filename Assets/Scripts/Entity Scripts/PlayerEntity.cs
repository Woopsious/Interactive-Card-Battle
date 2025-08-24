using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Woopsious.DamageData;

namespace Woopsious
{
	public class PlayerEntity : Entity
	{
		RectTransform rectTransform;

		Color _ColourDarkGreen = new(0, 0.3921569f, 0, 1);

		public int cardsUsedThisTurn;
		public int damageCardsUsedThisTurn;
		public int nonDamageCardsUsedThisTurn;
		public int cardsReplacedThisTurn;

		public static event Action<bool> HideOffensiveCards;
		public static event Action HideReplaceCardsButton;

		protected override void Awake()
		{
			base.Awake();
			rectTransform = GetComponent<RectTransform>();
			imageHighlight.color = _ColourDarkGreen;
		}

		void OnEnable()
		{
			TurnOrderManager.OnNewTurnEvent += StartTurn;
			CardHandler.OnCardUsed += UpdateCardsUsed;
			CardUi.OnCardReplace += OnReplaceCard;
			OnEntityDeath += RangerHealOnKill;
		}
		void OnDisable()
		{
			TurnOrderManager.OnNewTurnEvent -= StartTurn;
			CardHandler.OnCardUsed -= UpdateCardsUsed;
			CardUi.OnCardReplace -= OnReplaceCard;
			OnEntityDeath += RangerHealOnKill;
		}

		protected override void StartTurn(Entity entity)
		{
			imageHighlight.color = _ColourDarkGreen;

			if (entity == this)
				rectTransform.anchoredPosition = new Vector2(0, -190);
			else
				rectTransform.anchoredPosition = new Vector2(0, -325);

			base.StartTurn(entity);

			if (entity != this) return;

			if (EntityData.playerClass == EntityData.PlayerClass.Warrior)
				RecieveBlock(new(true, EntityData.extraBlockPerTurn));

			cardsUsedThisTurn = 0;
			damageCardsUsedThisTurn = 0;
			nonDamageCardsUsedThisTurn = 0;
			cardsReplacedThisTurn = 0;
		}
		public void UpdateCardsUsed(CardUi card)
		{
			if (!card.PlayerCard) return;

			cardsUsedThisTurn++;

			if (card.Offensive)
				damageCardsUsedThisTurn++;
			else
				nonDamageCardsUsedThisTurn++;

			if (cardsUsedThisTurn == EntityData.maxCardsUsedPerTurn)
			{
				EndTurn();
				return;
			}

			if (damageCardsUsedThisTurn == EntityData.maxDamageCardsUsedPerTurn)
			{
				HideOffensiveCards?.Invoke(true);
			}
			if (nonDamageCardsUsedThisTurn == EntityData.maxNonDamageCardsUsedPerTurn)
			{
				HideOffensiveCards?.Invoke(false);
			}
		}

		//entity hits via cards
		public override void RecieveDamage(DamageData damageData)
		{
			base.RecieveDamage(damageData);
			RogueReflectDamageRecieved(damageData);
		}

		//special class effects for player
		void RangerHealOnKill(Entity entity)
		{
			if (entity == this) return;
			if (EntityData.playerClass != EntityData.PlayerClass.Ranger) return;

			int healOnKill = (int)(EntityData.maxHealth / EntityData.healOnKillPercentage);
			RecieveHealing(new(false, healOnKill));
		}
		void RogueReflectDamageRecieved(DamageData damageData)
		{
			if (EntityData.playerClass != EntityData.PlayerClass.Rogue) return;
			if (!damageData.DamageReflectable) return;

			int damageReflected = Mathf.RoundToInt(damageData.DamageValue / EntityData.damageReflectedPercentage);
			if (damageReflected == 0)
				damageReflected++;

			damageData.EntityDamageSource.RecieveDamage(new(this, true, damageReflected));
		}

		//update image border highlight
		protected override void OnCardPicked(CardUi card)
		{
			if (card == null)
				imageHighlight.color = _ColourDarkGreen;
			else
			{
				if (!card.Offensive)
					imageHighlight.color = _ColourIceBlue;
				else
					imageHighlight.color = _ColourDarkGreen;
			}
		}
		protected override void CardEnter(CardUi card)
		{
			if (card == null)
				imageHighlight.color = _ColourDarkGreen;
			else
			{
				if (!card.Offensive)
					imageHighlight.color = _ColourYellow;
				else
					imageHighlight.color = _ColourDarkGreen;
			}
		}
		protected override void CardExit(CardUi card)
		{
			if (card == null)
				imageHighlight.color = _ColourDarkGreen;
			else
			{
				if (!card.Offensive && card.cardHandler.isBeingDragged)
					imageHighlight.color = _ColourIceBlue;
				else
					imageHighlight.color = _ColourDarkGreen;
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
			if (cardsReplacedThisTurn >= EntityData.maxReplaceableCardsPerTurn)
				return true;
			else
				return false;
		}
	}
}
