using System;
using UnityEngine;
using Woopsious.AbilitySystem;
using Woopsious.ComplexStats;
using Woopsious.AudioSystem;

namespace Woopsious
{
	public class Entity : MonoBehaviour, IDamagable, IStatusEffectsHandler
	{
		public bool debugInitilizeEntity;
		public EntityData EntityData { get; private set; }
		EntityMoves entityMoves;
		public StatusEffectsHandler StatusEffectsHandler { get; private set; }
		AudioHandler audioHandler;

		[Header("Runtime Stats")]
		public Stat healthMax;
		public Stat health;
		public Stat block;

		[Header("Modifiers")]
		public Stat damageBonus;

		[Header("Modifiers %")]
		public Stat damageDealtModifier;
		public Stat damageRecievedModifier;
		public Stat healingRecievedModifier;

		//hihglight colurs
		public enum HighlightState
		{
			Neutral, Unhighlighted, Highlighted
		}
		protected Color _ColourDarkRed = new(0.3921569f, 0, 0, 1);
		protected Color _ColourIceBlue = new(0, 1, 1, 1);
		protected Color _ColourYellow = new(0.7843137f, 0.6862745f, 0, 1);

		//events + rnd number
		public static event Action<Entity> OnEntityDeath;

		public event Action<string> OnEntityInitialize;
		public event Action<int, int> OnHealthChange;
		public event Action<int> OnBlockChange;
		public event Action<HighlightState> OnUpdateHighlightColour;

		private readonly System.Random systemRandom = new();

		protected virtual void Awake()
		{
			entityMoves = GetComponent<EntityMoves>();
			StatusEffectsHandler = GetComponent<StatusEffectsHandler>();
			audioHandler = GetComponent<AudioHandler>();
		}
		protected virtual void Start()
		{
			if (!debugInitilizeEntity) return;

			if (EntityData == null)
				Debug.LogError("Entity data null");
            else
				InitializeEntity(EntityData);
		}

		protected virtual void OnEnable()
		{
			TurnOrderManager.OnStartTurn += StartTurn;
			CardHandler.OnPlayerPickedUpCard += OnCardPicked;
			StatusEffectsHandler.OnStatusEffectChanges += StatusEffectChange;
		}
		protected virtual void OnDisable()
		{
			TurnOrderManager.OnStartTurn -= StartTurn;
			CardHandler.OnPlayerPickedUpCard -= OnCardPicked;
			StatusEffectsHandler.OnStatusEffectChanges -= StatusEffectChange;
		}

		void OnTriggerEnter2D(Collider2D other)
		{
			if (other.GetComponent<CardHandler>() != null)
				CardEnter(other.GetComponent<CardHandler>());
		}
		void OnTriggerExit2D(Collider2D other)
		{
			if (other.GetComponent<CardHandler>() != null)
				CardExit(other.GetComponent<CardHandler>());
		}

		public void InitializeEntity(EntityData entityData)
		{
			if (entityData == null)
				return;
			else
				EntityData = entityData;

			InitializeStats();

			OnEntityInitialize?.Invoke(EntityData.CreateEntityName(EntityData));
			OnHealthChange?.Invoke((int)health.Value, (int)healthMax.Value);
			OnBlockChange?.Invoke((int)block.Value);
			ImageHighlightChangeEvent(HighlightState.Neutral);

			if (entityData.isPlayer) return;

			entityMoves.InitilizeMoveSet(this);
			StatusEffectsHandler.ClearStatusEffects();
		}
		protected virtual void InitializeStats()
		{
			healthMax.InitializeStat(EntityData.maxHealth);
			healthMax.AddModifier(healthMax.StatType, 
				MapController.Instance.globalModifiers.GetModifierStat(healthMax.StatType));

			health.InitializeStat(healthMax.Value);
			block.InitializeStat(EntityData.baseBlock);

			damageBonus.InitializeStat(0);

			damageDealtModifier.InitializeStat(1);
			damageDealtModifier.AddModifier(damageDealtModifier.StatType, 
				MapController.Instance.globalModifiers.GetModifierStat(damageDealtModifier.StatType));

			damageRecievedModifier.InitializeStat(1);
			damageRecievedModifier.AddModifier(damageRecievedModifier.StatType, 
				MapController.Instance.globalModifiers.GetModifierStat(damageRecievedModifier.StatType));

			healingRecievedModifier.InitializeStat(1);
			healingRecievedModifier.AddModifier(healingRecievedModifier.StatType,
				MapController.Instance.globalModifiers.GetModifierStat(healingRecievedModifier.StatType));
		}

		//start/end turn events
		protected virtual void StartTurn(Entity entity)
		{
			if (entity != this) return; //not this entities turn

			UpdateBlock(true);
			StatusEffectsHandler.OnNewTurn(entity.gameObject);

			if (EntityData.isPlayer) return; //if is player shouldnt need to do anything else as other scripts handle it

			entityMoves.PickNextMove();
		}

		//entity hits via cards
		public virtual void ReceiveDamage(DamageData damageData)
		{
			if (damageData.EntityDamageSource.EntityData.playerClass == EntityData.PlayerClass.Mage)
				damageData.DamageValue = GetMageClassSpecialDamageModifier(damageData);

			damageData.DamageValue = GetDamageReceivedModifier(damageData);
			damageData.DamageValue = GetBlockedDamage(damageData);

			health.ModifyValue(damageData.DamageValue, false);

			audioHandler.PlayAudio(EntityData.hitSfx, true);
			OnBlockChange?.Invoke((int)block.Value);
			OnHealthChange?.Invoke((int)health.Value, (int)healthMax.Value);
			ImageHighlightChangeEvent(HighlightState.Neutral);
			OnDeath();

			if (damageData.FromEffect) return;
			CombatLogUi.CreateLog(new(CombatLogContext.CombatLogEntry.damage, damageData.EntityDamageSource, this, damageData));
		}
		public virtual void ReceiveBlock(DamageData damageData)
		{
			block.ModifyValue(damageData.BlockValue, true);
			OnBlockChange?.Invoke((int)block.Value);
			CombatLogUi.CreateLog(new(CombatLogContext.CombatLogEntry.block, damageData.EntityDamageSource, this, damageData));
		}
		public virtual void ReceiveHealing(DamageData damageData)
		{
			damageData.HealValue = Mathf.RoundToInt(damageData.HealValue * healingRecievedModifier.Value);
			damageData.HealValue = (int)Mathf.Min(damageData.HealValue, healthMax.Value - health.Value);
			health.ModifyValue(damageData.HealValue, true);
			OnHealthChange?.Invoke((int)health.Value, (int)healthMax.Value);
			CombatLogUi.CreateLog(new(CombatLogContext.CombatLogEntry.heal, damageData.EntityDamageSource, this, damageData));
		}

		//helpers
		int GetMageClassSpecialDamageModifier(DamageData damageData)
		{
			float roll = (float)(systemRandom.NextDouble() * 100);
			if (roll < damageData.EntityDamageSource.EntityData.chanceOfDoubleDamage)
				return damageData.DamageValue *= 2;
			else
				return damageData.DamageValue;
		}
		int GetDamageReceivedModifier(DamageData damageData)
		{
			int damage = (int)(damageData.DamageValue * damageRecievedModifier.Value);
			return damage;
		}
		int GetBlockedDamage(DamageData damageData)
		{
			if (damageData.DamageIgnoresBlock)
				return damageData.DamageValue;

			if (block.Value > damageData.DamageValue)
			{
				block.ModifyValue(damageData.DamageValue, false);
				damageData.DamageValue = 0;
			}
			else if (block.Value >= 0)
			{
				damageData.DamageValue -= (int)block.Value;

				if (block.Value - damageData.DamageValue < 0) //stop negative block and set to 0
					block.SetValue(0);
			}
			return damageData.DamageValue;
		}

		//entity death
		void OnDeath()
		{
			if (health.Value <= 0)
			{
				OnEntityDeath?.Invoke(this);
				gameObject.SetActive(false);
			}
		}

		//Status Effect Changes
		protected virtual void StatusEffectChange(StatusEffectsData effect, bool wasAdded)
		{
			if (wasAdded)
				CombatLogUi.CreateLog(new(CombatLogContext.CombatLogEntry.statusEffectGained, this, effect));
			else
				CombatLogUi.CreateLog(new(CombatLogContext.CombatLogEntry.statusEffectLost, this, effect));
		}

		//applying damage from DoT effects
		public void ApplyDoTFromEffects(StatusEffect statusEffect)
		{
			DamageData damageData = new(this, true, false, true, (int)statusEffect.effectModifier.value);
			ReceiveDamage(damageData);

			if (!damageData.FromEffect) return;
			CombatLogUi.CreateLog(new(CombatLogContext.CombatLogEntry.effectDamage, this, damageData, statusEffect.StatusEffectsData));
		}

		//applying/removing stat modifiers
		public virtual void AddStatModifier(StatType statType, StatModifier modifier)
		{
			IncreaseHealthBasedOnMaxHealthModifiers(statType, modifier);

			block.AddModifier(statType, modifier);
			damageBonus.AddModifier(statType, modifier);

			damageDealtModifier.AddModifier(statType, modifier);
			damageRecievedModifier.AddModifier(statType, modifier);
			healingRecievedModifier.AddModifier(statType , modifier);

			UpdateBlock(false);
		}
		public virtual void RemoveStatModifier(StatType statType, UnityEngine.Object modifierSource)
		{
			DecreaseHealthBasedOnMaxHealthModifiers(statType, modifierSource);

			block.RemoveModifier(statType, modifierSource);
			damageBonus.RemoveModifier(statType, modifierSource);

			damageDealtModifier.RemoveModifier(statType, modifierSource);
			damageRecievedModifier.RemoveModifier(statType, modifierSource);
			healingRecievedModifier.RemoveModifier(statType, modifierSource);

			UpdateBlock(false);
		}
		private void IncreaseHealthBasedOnMaxHealthModifiers(StatType statType, StatModifier modifier)
		{
			float percentageOfCurrentHealth = health.Value / healthMax.Value;
			healthMax.AddModifier(statType, modifier);

			float newHealth = Mathf.RoundToInt(healthMax.Value * percentageOfCurrentHealth);
			health.SetValue(newHealth);
			OnHealthChange?.Invoke((int)health.Value, (int)healthMax.Value);
		}
		private void DecreaseHealthBasedOnMaxHealthModifiers(StatType statType, UnityEngine.Object modifierSource)
		{
			healthMax.RemoveModifier(statType, modifierSource);

			if (health.Value > healthMax.Value)
				health.SetValue(healthMax.Value);
			OnHealthChange?.Invoke((int)health.Value, (int)healthMax.Value);
		}
		private void UpdateBlock(bool newTurn)
		{
			if (newTurn)
				block.ResetModifiedValue();

			OnBlockChange?.Invoke((int)block.Value);
		}

		//event calls
		protected void ImageHighlightChangeEvent(HighlightState highlightState)
		{
			OnUpdateHighlightColour?.Invoke(highlightState);
		}

		//debugs
		public void DebugKill()
		{
			ReceiveDamage(new DamageData(this, false, false, true, (int)health.Value + 1));
			OnDeath();
		}

		//update image border highlight
		protected virtual void OnCardPicked(CardHandler card)
		{
			if (card == null)
				ImageHighlightChangeEvent(HighlightState.Neutral);
			else
			{
				if (card.Offensive)
					ImageHighlightChangeEvent(HighlightState.Unhighlighted);
				else
					ImageHighlightChangeEvent(HighlightState.Neutral);
			}
		}
		protected virtual void CardEnter(CardHandler card)
		{
			if (card == null)
				ImageHighlightChangeEvent(HighlightState.Neutral);
			else
			{
				if (card.Offensive)
					ImageHighlightChangeEvent(HighlightState.Highlighted);
				else
					ImageHighlightChangeEvent(HighlightState.Neutral);
			}
		}
		protected virtual void CardExit(CardHandler card)
		{
			if (card == null)
				ImageHighlightChangeEvent(HighlightState.Neutral);
			else
			{
				if (card.Offensive && card.isBeingDragged)
					ImageHighlightChangeEvent(HighlightState.Unhighlighted);
				else
					ImageHighlightChangeEvent(HighlightState.Neutral);
			}
		}
	}
}
