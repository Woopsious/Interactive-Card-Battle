using System;
using UnityEngine;
using UnityEngine.UI;
using static Woopsious.DamageData;

namespace Woopsious
{
	public class PlayerEntity : Entity
	{
		RectTransform rectTransform;

		Color _ColourDarkGreen = new(0, 0.3921569f, 0, 1);

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
			CardHandler.OnPlayerUseCard += UpdateCardsUsed;
			CardUi.OnCardReplace += OnReplaceCard;
			OnEntityDeath += RangerHealOnKill;
		}
		void OnDisable()
		{
			TurnOrderManager.OnNewTurnEvent -= StartTurn;
			CardHandler.OnPlayerUseCard -= UpdateCardsUsed;
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
				AddBlock(EntityData.extraBlockPerTurn);

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
		public override void OnHit(DamageData damageData)
		{
			base.OnHit(damageData);

			if (damageData.damageType is DamageType.block or DamageType.heal) return;
			RogueReflectDamageRecieved(damageData);
		}

		//special class effects for player
		void RangerHealOnKill(Entity entity)
		{
			if (entity == this) return;
			if (EntityData.playerClass != EntityData.PlayerClass.Ranger) return;

			int healOnKill = (int)(EntityData.maxHealth / EntityData.healOnKillPercentage);
			RecieveHealing(healOnKill);
		}
		void RogueReflectDamageRecieved(DamageData damageData)
		{
			if (EntityData.playerClass != EntityData.PlayerClass.Rogue) return;

			int damageReflected = Mathf.RoundToInt(damageData.damage / EntityData.damageReflectedPercentage);

			if (damageReflected == 0)
				damageReflected++;
			damageData.entityDamageSource.OnHit(new(this, true, damageReflected, DamageType.physical));
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
