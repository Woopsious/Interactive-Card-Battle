using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Woopsious.DamageData;

namespace Woopsious
{
	public class Entity : MonoBehaviour, IDamagable
	{
		public bool debugInitilizeEntity;
		public EntityData EntityData { get; private set; }
		EntityMoves entityMoves;
		private AudioHandler audioHandler;

		//ui elements
		public TMP_Text entityNameText;
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
		[SerializeReference] public Stat damageDealtModifier;
		[SerializeReference] public Stat damageRecievedModifier;

		//events + rnd number
		public static event Action<Entity> OnTurnEndEvent;
		public static event Action<Entity> OnEntityDeath;

		private readonly System.Random systemRandom = new();

		protected virtual void Awake()
		{
			entityMoves = GetComponent<EntityMoves>();
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
			{
				Debug.LogError("Entity data null");
				return;
			}
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

			health = entityData.maxHealth;
			block = 0;
			damageDealtModifier = new Stat(1);
			damageRecievedModifier = new Stat(1);
			damageDealtModifier.AddModifier(-0.5f);

			UpdateUi();
			turnIndicator.SetActive(false);

			if (entityData.isPlayer) return;

			imageHighlight.color = _ColourDarkRed;
			entityMoves.InitilizeMoveSet(this);
		}

		//start/end turn events
		protected virtual void StartTurn(Entity entity)
		{
			if (entity != this) return; //not this entities turn

			block = 0;
			UpdateUi();

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
			int damage = 0; //get true damage from multihit enum
			switch (damageData.multiHitSettings)
			{
				case MultiHitAttack.No:
				damage = damageData.DamageValue;
				break;
				case MultiHitAttack.TwoHits:
				damage = damageData.DamageValue * 2;
				break;
				case MultiHitAttack.ThreeHits:
				damage = damageData.DamageValue * 3;
				break;
				case MultiHitAttack.FourHits:
				damage = damageData.DamageValue * 4;
				break;
			}

			damage = (int)(damage * damageRecievedModifier.Value); //apply damage recieved modifier

			if (damageData.EntityDamageSource.EntityData.playerClass == EntityData.PlayerClass.Mage)
			{
				float roll = (float)(systemRandom.NextDouble() * 100);
				if (roll < damageData.EntityDamageSource.EntityData.chanceOfDoubleDamage)
					damage *= 2;
			}

			if (damageData.DamageIgnoresBlock)
				health -= damage;
			else
			{
				damage = GetBlockedDamage(damage);
				health -= damage;
			}

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

		int GetBlockedDamage(int damage)
		{
			if (block > damage)
			{
				block -= damage;
				damage = 0;
			}
			else
			{
				damage -= block;
				block = 0;
			}

			return damage;
		}
		void OnDeath()
		{
			if (health <= 0)
			{
				OnEntityDeath?.Invoke(this);
				gameObject.SetActive(false);
			}
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

		void UpdateUi()
		{
			entityHealthText.text = "HEALTH:\n" + health + "/" + EntityData.maxHealth;
			entityblockText.text = "Block: " + block;
		}
	}
}
