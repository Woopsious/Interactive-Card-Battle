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

		//ui elements
		public TMP_Text entityNameText;
		public TMP_Text energyText;
		public TMP_Text entityHealthText;
		public TMP_Text entityblockText;
		public GameObject turnIndicator;

		//background image highlight
		protected Image imageHighlight;
		Color _ColourDarkRed = new(0.3921569f, 0, 0, 1);
		protected Color _ColourIceBlue = new(0, 1, 1, 1);
		protected Color _ColourYellow = new(0.7843137f, 0.6862745f, 0, 1);

		[Header("Runtime Stats")]
		public int health;
		public int block;

		[Header("Modifiers %")]
		public Stat damageDealtModifier;
		public Stat damageRecievedModifier;

		[Header("Modifiers")]
		public Stat damageBonus;
		public Stat blockBonus;

		//events + rnd number
		public static event Action<Entity> OnTurnEndEvent;
		public static event Action<Entity> OnEntityDeath;

		private readonly System.Random systemRandom = new();

		protected virtual void Awake()
		{
			entityMoves = GetComponent<EntityMoves>();
			StatusEffectsHandler = GetComponent<StatusEffectsHandler>();
			imageHighlight = GetComponent<Image>();
			imageHighlight.color = _ColourDarkRed;
			audioHandler = GetComponent<AudioHandler>();
		}
		protected virtual void Start()
		{
			if (!debugInitilizeEntity) return;

			if (EntityData == null)
				Debug.LogError("Entity data null");
            else
				InitilizeEntity(EntityData);
		}

		void OnEnable()
		{
			TurnOrderManager.OnNewTurnEvent += StartTurn;
			CardHandler.OnPlayerPickedUpCard += OnCardPicked;
		}
		void OnDisable()
		{
			TurnOrderManager.OnNewTurnEvent -= StartTurn;
			CardHandler.OnPlayerPickedUpCard -= OnCardPicked;
		}

		void OnTriggerEnter2D(Collider2D other)
		{
			if (other.GetComponent<CardUi>() != null)
				CardEnter(other.GetComponent<CardUi>());
		}
		void OnTriggerExit2D(Collider2D other)
		{
			if (other.GetComponent<CardUi>() != null)
				CardExit(other.GetComponent<CardUi>());
		}

		public void InitilizeEntity(EntityData entityData)
		{
			if (entityData == null)
			return;
			else
				EntityData = entityData;

			string entityName; //specialize name

			if (entityData.playerClass == EntityData.PlayerClass.Mage)
				entityName = "Player (Mage)";
			else if (entityData.playerClass == EntityData.PlayerClass.Ranger)
				entityName = "Player (Ranger)";
			else if(entityData.playerClass == EntityData.PlayerClass.Rogue)
				entityName = "Player (Rogue)";
			else if(entityData.playerClass == EntityData.PlayerClass.Warrior)
				entityName = "Player (Warrior)";
			else
				entityName = entityData.entityName;

			gameObject.name = entityName;
			entityNameText.text = entityName;

			InitilizeStats();
			UpdateUi();
			turnIndicator.SetActive(false);

			if (entityData.isPlayer) return;

			imageHighlight.color = _ColourDarkRed;
			entityMoves.InitilizeMoveSet(this);
			StatusEffectsHandler.ClearStatusEffects();
		}
		protected virtual void InitilizeStats()
		{
			health = EntityData.maxHealth;
			block = EntityData.baseBlock;

			damageDealtModifier.InitilizeStat(1);
			damageRecievedModifier.InitilizeStat(1);

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

			turnIndicator.SetActive(true);
			entityMoves.PickNextMove();
		}
		public virtual void EndTurn()
		{
			turnIndicator.SetActive(false);
			OnTurnEndEvent?.Invoke(this);
		}

		//entity hits via cards
		public virtual void RecieveDamage(DamageData damageData)
		{
			if (damageData.EntityDamageSource.EntityData.playerClass == EntityData.PlayerClass.Mage)
				damageData.DamageValue = GetMageClassSpecialDamageModifier(damageData);

			damageData.DamageValue = GetDamageRecievedModifier(damageData);
			damageData.DamageValue = GetBlockedDamage(damageData);

			health -= damageData.DamageValue;

			audioHandler.PlayAudio(EntityData.hitSfx, true);
			UpdateUi();
			OnDeath();
		}
		public virtual void RecieveBlock(DamageData damageData)
		{
			block += damageData.BlockValue;
			UpdateUi();
		}
		public virtual void RecieveHealing(DamageData damageData)
		{
			health += damageData.HealValue;
			if (health > EntityData.maxHealth)
				health = EntityData.maxHealth;

			UpdateUi();
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
		int GetDamageRecievedModifier(DamageData damageData)
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
			RecieveDamage(new DamageData(this, false, true, (int)damage));
		}

		//applying/removing stat modifiers
		public virtual void AddStatModifier(StatType statType, float value)
		{
			int blockToKeep = (int)(block - blockBonus.value);

			damageDealtModifier.AddModifier(statType, value);
			damageRecievedModifier.AddModifier(statType, value);

			damageBonus.AddModifier(statType, value);
			blockBonus.AddModifier(statType, value);

			UpdateStatsAndUi(false, blockToKeep);
		}
		public virtual void RemoveStatModifier(StatType statType, float value)
		{
			int blockToKeep = (int)(block - blockBonus.value);

			damageDealtModifier.RemoveModifier(statType, value);
			damageRecievedModifier.RemoveModifier(statType, value);

			damageBonus.RemoveModifier(statType, value);
			blockBonus.RemoveModifier(statType, value);

			UpdateStatsAndUi(false, blockToKeep);
		}
		void UpdateStatsAndUi(bool newTurn, int blockToKeep)
		{
			if (newTurn)
			{
				block = (int)(EntityData.baseBlock + blockBonus.value);
			}
			else
			{
				block = (int)(blockToKeep + blockBonus.value);
			}
			UpdateUi();
		}

		//debugs
		public void DebugKill()
		{
			health = 0;
			OnDeath();
		}

		//update image border highlight
		protected virtual void OnCardPicked(CardUi card)
		{
			if (card == null)
				imageHighlight.color = _ColourDarkRed;
			else
			{
				if (card.Offensive)
					imageHighlight.color = _ColourIceBlue;
				else
					imageHighlight.color = _ColourDarkRed;
			}
		}
		protected virtual void CardEnter(CardUi card)
		{
			if (card == null)
				imageHighlight.color = _ColourDarkRed;
			else
			{
				if (card.Offensive)
					imageHighlight.color = _ColourYellow;
				else
					imageHighlight.color = _ColourDarkRed;
			}
		}
		protected virtual void CardExit(CardUi card)
		{
			if (card == null)
				imageHighlight.color = _ColourDarkRed;
			else
			{
				if (card.Offensive && card.cardHandler.isBeingDragged)
					imageHighlight.color = _ColourIceBlue;
				else
					imageHighlight.color = _ColourDarkRed;
			}
		}

		protected void UpdateUi()
		{
			entityHealthText.text = "HP:" + health + "/" + EntityData.maxHealth;
			entityblockText.text = "BLOCK: " + block;
		}
	}
}
