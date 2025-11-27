using System;
using System.Collections.Generic;
using UnityEngine;
using Woopsious.AbilitySystem;
using Woopsious.ComplexStats;
using static Woopsious.DamageData;

namespace Woopsious
{
	public class PlayerEntity : Entity
	{
		Color _ColourDarkGreen = new(0, 0.3921569f, 0, 1);

		[Header("Player Unique Stats")]
		public Stat cardDrawAmount;
		public Stat energy;

		[Header("Runtime Debug Inspector")]
		public List<StatusEffectsData> statusEffectsToDebugAdd;

		public static event Action OnStatChanges;
		public static event Action<int> OnEnergyChange;

		protected override void OnEnable()
		{
			base.OnEnable();
			CardHandler.OnCardCleanUp += UpdateCardsUsed;
			OnEntityDeath += RangerHealOnKill;
		}
		protected override void OnDisable()
		{
			base.OnDisable();
			CardHandler.OnCardCleanUp -= UpdateCardsUsed;
			OnEntityDeath -= RangerHealOnKill;
		}

		protected override void InitializeStats()
		{
			base.InitializeStats();

			cardDrawAmount.InitilizeStat(EntityData.initialCardDrawAmount);
			energy.InitilizeStat(EntityData.baseEnergy);
		}

		protected override void StartTurn(Entity entity)
		{
			base.StartTurn(entity);

			ImageHighlightChangeEvent(_ColourDarkGreen);

			if (entity != this) return;

			energy.ResetStat();
			cardDrawAmount.ResetStat();
			OnEnergyChange?.Invoke((int)energy.value);
		}
		public void UpdateCardsUsed(CardHandler card)
		{
			if (!card.PlayerCard) return;

			energy.AddModifier(energy.statType, new(false, card.AttackData.energyCost));
			cardDrawAmount.AddModifier(cardDrawAmount.statType, new(false, card.AttackData.extraCardsToDraw));
			OnEnergyChange?.Invoke((int)energy.value);

			if (energy.value <= 0)
			{
				TurnOrderManager.EndCurrentTurn(this);
				return;
			}
		}

		//entity hits via cards
		public override void ReceiveDamage(DamageData damageData)
		{
			base.ReceiveDamage(damageData);
			RogueReflectDamageReceived(damageData);
		}

		//applying/removing stat modifiers
		public override void AddStatModifier(StatType statType, StatData statData)
		{
			base.AddStatModifier(statType, statData);
			OnStatChanges?.Invoke();
		}
		public override void RemoveStatModifier(StatType statType, StatData statData)
		{
			base.RemoveStatModifier(statType, statData);
			OnStatChanges?.Invoke();
		}

		//special class effects for player
		void RangerHealOnKill(Entity entity)
		{
			if (entity == this) return;
			if (EntityData.playerClass != EntityData.PlayerClass.Ranger) return;

			int healOnKill = (int)(healthMax.value / EntityData.healOnKillPercentage);
			ReceiveHealing(new(ValueTypes.heals, healOnKill));
		}
		void RogueReflectDamageReceived(DamageData damageData)
		{
			if (EntityData.playerClass != EntityData.PlayerClass.Rogue) return;
			if (!damageData.DamageReflectable) return;

			int damageReflected = Mathf.RoundToInt(damageData.DamageValue / EntityData.damageReflectedPercentage);
			if (damageReflected == 0)
				damageReflected++;

			damageData.EntityDamageSource.ReceiveDamage(new(this, false, true, damageReflected));
		}

		//update image border highlight
		protected override void OnCardPicked(CardHandler card)
		{
			if (card == null)
				ImageHighlightChangeEvent(_ColourDarkGreen);
			else
			{
				if (!card.Offensive)
					ImageHighlightChangeEvent(_ColourIceBlue);
				else
					ImageHighlightChangeEvent(_ColourDarkGreen);
			}
		}
		protected override void CardEnter(CardHandler card)
		{
			if (card == null)
				ImageHighlightChangeEvent(_ColourDarkGreen);
			else
			{
				if (!card.Offensive)
					ImageHighlightChangeEvent(_ColourYellow);
				else
					ImageHighlightChangeEvent(_ColourDarkGreen);
			}
		}
		protected override void CardExit(CardHandler card)
		{
			if (card == null)
				ImageHighlightChangeEvent(_ColourDarkGreen);
			else
			{
				if (!card.Offensive && card.isBeingDragged)
					ImageHighlightChangeEvent(_ColourIceBlue);
				else
					ImageHighlightChangeEvent(_ColourDarkGreen);
			}
		}

		//debug
		public void DebugAddStatusEffect()
		{
			StatusEffectsHandler.AddStatusEffects(statusEffectsToDebugAdd);
		}
	}
}
