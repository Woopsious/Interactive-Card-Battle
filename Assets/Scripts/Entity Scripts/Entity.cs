using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
		public int health;
		public int block;

		[Header("Modifiers %")]
		public Stat damageDealtModifier;
		public Stat damageRecievedModifier;

		[Header("Modifiers")]
		public Stat healthMax;
		public Stat damageBonus;
		public Stat blockBonus;

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
		}
		protected virtual void OnDisable()
		{
			TurnOrderManager.OnStartTurn -= StartTurn;
			CardHandler.OnPlayerPickedUpCard -= OnCardPicked;
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
			OnHealthChange?.Invoke(health, entityData.maxHealth);
			OnBlockChange?.Invoke(block);
			ImageHighlightChangeEvent(HighlightState.Neutral);

			if (entityData.isPlayer) return;

			entityMoves.InitilizeMoveSet(this);
			StatusEffectsHandler.ClearStatusEffects();
		}
		protected virtual void InitializeStats()
		{
			health = EntityData.maxHealth;
			block = EntityData.baseBlock;

			damageDealtModifier.InitilizeStat(1);
			damageRecievedModifier.InitilizeStat(1);

			healthMax.InitilizeStat(EntityData.maxHealth);
			damageBonus.InitilizeStat(0);
			blockBonus.InitilizeStat(0);
		}

		//start/end turn events
		protected virtual void StartTurn(Entity entity)
		{
			if (entity != this) return; //not this entities turn

			UpdateStatsAndUi(true, 0);
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

			health -= damageData.DamageValue;

			audioHandler.PlayAudio(EntityData.hitSfx, true);
			OnBlockChange?.Invoke(block);
			ImageHighlightChangeEvent(HighlightState.Neutral);
			OnDeath();
		}
		public virtual void ReceiveBlock(DamageData damageData)
		{
			block += damageData.BlockValue;
			OnBlockChange?.Invoke(block);
		}
		public virtual void ReceiveHealing(DamageData damageData)
		{
			health += damageData.HealValue;
			if (health > healthMax.value)
				health = (int)healthMax.value;

			OnHealthChange?.Invoke(health, EntityData.maxHealth);
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
			int damage = (int)(damageData.DamageValue * damageRecievedModifier.value);
			return damage;
		}
		int GetBlockedDamage(DamageData damageData)
		{
			if (damageData.DamageIgnoresBlock)
				return damageData.DamageValue;

			if (block > damageData.DamageValue)
			{
				block -= damageData.DamageValue;
				damageData.DamageValue = 0;
			}
			else
			{
				damageData.DamageValue -= block;
				block = 0;
			}
			return damageData.DamageValue;
		}

		//entity death
		void OnDeath()
		{
			if (health <= 0)
			{
				OnEntityDeath?.Invoke(this);
				gameObject.SetActive(false);
			}
		}

		//applying damage from DoT effects
		public void ApplyDoTFromEffects(float damage)
		{
			ReceiveDamage(new DamageData(this, false, true, (int)damage));
		}

		//applying/removing stat modifiers
		public virtual void AddStatModifier(StatType statType, StatData statData)
		{
			int blockToKeep = (int)(block - blockBonus.value);

			damageDealtModifier.AddModifier(statType, statData);
			damageRecievedModifier.AddModifier(statType, statData);

			damageBonus.AddModifier(statType, statData);
			blockBonus.AddModifier(statType, statData);

			UpdateStatsAndUi(false, blockToKeep);
		}
		public virtual void RemoveStatModifier(StatType statType, StatData statData)
		{
			int blockToKeep = (int)(block - blockBonus.value);

			damageDealtModifier.RemoveModifier(statType, statData);
			damageRecievedModifier.RemoveModifier(statType, statData);

			damageBonus.RemoveModifier(statType, statData);
			blockBonus.RemoveModifier(statType, statData);

			UpdateStatsAndUi(false, blockToKeep);
		}
		void UpdateStatsAndUi(bool newTurn, int blockToKeep)
		{
			if (newTurn)
				block = (int)(EntityData.baseBlock + blockBonus.value);
			else
				block = (int)(blockToKeep + blockBonus.value);

			OnBlockChange?.Invoke(block);
		}

		//event calls
		protected void ImageHighlightChangeEvent(HighlightState highlightState)
		{
			OnUpdateHighlightColour?.Invoke(highlightState);
		}

		//debugs
		public void DebugKill()
		{
			health = 0;
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
