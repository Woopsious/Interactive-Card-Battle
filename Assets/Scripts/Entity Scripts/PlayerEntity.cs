using System;
using UnityEngine;
using UnityEngine.UI;
using static Woopsious.DamageData;

namespace Woopsious
{
	public class PlayerEntity : Entity
	{
		RectTransform rectTransform;
		Image imageHighlight;

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
			CardUi.OnCardReplace += OnReplaceCard;
			ThrowableCard.OnCardPickUp += OnCardPicked;
			OnEntityDeath += RangerHealOnKill;
		}
		void OnDisable()
		{
			TurnOrderManager.OnNewTurnEvent -= StartTurn;
			CardUi.OnCardReplace -= OnReplaceCard;
			ThrowableCard.OnCardPickUp -= OnCardPicked;
			OnEntityDeath += RangerHealOnKill;
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
			imageHighlight.color = _ColourDarkGreen;

			if (entity == this)
				rectTransform.anchoredPosition = new Vector2(0, -190);
			else
				rectTransform.anchoredPosition = new Vector2(0, -325);

			base.StartTurn(entity);

			if (entity != this) return;

			if (entityData.playerClass == EntityData.PlayerClass.Warrior)
				AddBlock(entityData.extraBlockPerTurn);

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

		public override void OnHit(DamageData damageData)
		{
			base.OnHit(damageData);
			RogueReflectDamageRecieved(damageData);
		}

		void RangerHealOnKill(Entity entity)
		{
			if (entity == this) return;
			if (entityData.playerClass != EntityData.PlayerClass.Ranger) return;

			int healOnKill = (int)(entityData.maxHealth / entityData.healOnKillPercentage);
			RecieveHealing(healOnKill);
		}
		void RogueReflectDamageRecieved(DamageData damageData)
		{
			if (entityData.playerClass != EntityData.PlayerClass.Rogue) return;

			int damageReflected = Mathf.RoundToInt(damageData.damage / entityData.damageReflectedPercentage);

			if (damageReflected == 0)
				damageReflected++;
			damageData.entityDamageSource.OnHit(new(this, true, damageReflected, DamageType.physical));
		}

		//update image border highlight
		void OnCardPicked(CardUi card)
		{
			if (card == null)
				imageHighlight.color = _ColourDarkGreen;
			else
			{
				if (card.Offensive)
					imageHighlight.color = _ColourDarkGreen;
				else
					imageHighlight.color = Color.red;
			}
		}
		void ThrowableCardEnter(CardUi card)
		{
			if (card == null)
				imageHighlight.color = _ColourDarkGreen;
			else
			{
				if (card.Offensive)
					imageHighlight.color = _ColourDarkGreen;
				else
					imageHighlight.color = Color.cyan;
			}
		}
		void ThrowableCardExit(CardUi card)
		{
			if (card == null)
				imageHighlight.color = _ColourDarkGreen;
			else
			{
				if (!card.Offensive && card.throwableCard.isBeingDragged)
					imageHighlight.color = Color.red;
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
			if (cardsReplacedThisTurn >= entityData.maxReplaceableCardsPerTurn)
				return true;
			else
				return false;
		}
	}
}
