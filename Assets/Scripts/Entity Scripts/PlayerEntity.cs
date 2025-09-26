using System;
using System.Collections.Generic;
using UnityEngine;
using static Woopsious.DamageData;
using static Woopsious.Stat;

namespace Woopsious
{
	public class PlayerEntity : Entity
	{
		RectTransform rectTransform;

		Color _ColourDarkGreen = new(0, 0.3921569f, 0, 1);

		[Header("Player Unique Stats")]
		public Stat cardDrawAmount;
		public int energy;

		public static event Action<int> OnPlayerEnergyChanges;
		public static event Action OnPlayerStatChanges;

		protected override void Awake()
		{
			base.Awake();
			rectTransform = GetComponent<RectTransform>();
			imageHighlight.color = _ColourDarkGreen;
		}

		void OnEnable()
		{
			TurnOrderManager.OnNewTurnEvent += StartTurn;
			CardHandler.OnCardCleanUp += UpdateCardsUsed;
			OnEntityDeath += RangerHealOnKill;
		}
		void OnDisable()
		{
			TurnOrderManager.OnNewTurnEvent -= StartTurn;
			CardHandler.OnCardCleanUp -= UpdateCardsUsed;
			OnEntityDeath += RangerHealOnKill;
		}

		protected override void InitilizeStats()
		{
			base.InitilizeStats();

			cardDrawAmount = new Stat(EntityData.initialCardDrawAmount, StatType.cardDrawAmount);
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

			energy = EntityData.baseEnergy;

			UpdateEnergyUi();
		}
		public void UpdateCardsUsed(CardUi card)
		{
			if (!card.PlayerCard) return;

			energy -= card.AttackData.energyCost;
			UpdateEnergyUi();
			OnPlayerEnergyChanges?.Invoke(energy);

			if (energy <= 0)
			{
				EndTurn();
				return;
			}
		}

		//entity hits via cards
		public override void RecieveDamage(DamageData damageData)
		{
			base.RecieveDamage(damageData);
			RogueReflectDamageRecieved(damageData);
		}

		//applying/removing stat modifiers
		public override void AddStatModifier(float value, StatType statType)
		{
			base.AddStatModifier(value, statType);
			OnPlayerStatChanges?.Invoke();
		}
		public override void RemoveStatModifier(float value, StatType statType)
		{
			base.RemoveStatModifier(value, statType);
			OnPlayerStatChanges?.Invoke();
		}

		//special class effects for player
		void RangerHealOnKill(Entity entity)
		{
			if (entity == this) return;
			if (EntityData.playerClass != EntityData.PlayerClass.Ranger) return;

			int healOnKill = (int)(EntityData.maxHealth / EntityData.healOnKillPercentage);
			RecieveHealing(new(ValueTypes.heals, healOnKill));
		}
		void RogueReflectDamageRecieved(DamageData damageData)
		{
			if (EntityData.playerClass != EntityData.PlayerClass.Rogue) return;
			if (!damageData.DamageReflectable) return;

			int damageReflected = Mathf.RoundToInt(damageData.DamageValue / EntityData.damageReflectedPercentage);
			if (damageReflected == 0)
				damageReflected++;

			damageData.EntityDamageSource.RecieveDamage(new(this, false, true, damageReflected));
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

		protected void UpdateEnergyUi()
		{
			energyText.text = $"{energy}";
		}
	}
}
