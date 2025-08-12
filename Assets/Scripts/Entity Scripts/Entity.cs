using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

		public TMP_Text entityNameText;
		public TMP_Text entityHealthText;
		public TMP_Text entityblockText;
		public GameObject turnIndicator;

		public int health;
		public int block;

		public static event Action<Entity> OnTurnEndEvent;
		public static event Action<Entity> OnEntityDeath;

		private readonly System.Random systemRandom = new();

		//background image highlight
		protected Image imageHighlight;
		Color _ColourDarkRed = new(0.3921569f, 0, 0, 1);
		protected Color _ColourIceBlue = new(0, 1, 1, 1);
		protected Color _ColourYellow = new(0.7843137f, 0.6862745f, 0, 1);

		AudioHandler audioHandler;

		void Awake()
		{
			entityMoves = GetComponent<EntityMoves>();
			imageHighlight = GetComponent<Image>();
			imageHighlight.color = _ColourDarkRed;
			audioHandler = GetComponent<AudioHandler>();
		}
		void Start()
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
		public virtual void OnHit(DamageData damageData)
		{
			if (damageData.damageType == DamageType.block)
				AddBlock(damageData.damage);
			else if (damageData.damageType == DamageType.heal)
				RecieveHealing(damageData.damage);
			else if (damageData.damageType == DamageType.physical)
				RecieveDamage(damageData);
			else
				Debug.LogError("no hit type set up");
		}
		protected void AddBlock(int block)
		{
			this.block += block;
			UpdateUi();
		}
		protected void RecieveHealing(int healing)
		{
			health += healing;
			if (health > EntityData.maxHealth)
				health = EntityData.maxHealth;

			UpdateUi();
		}
		protected void RecieveDamage(DamageData damageData)
		{
			int damage = damageData.damage;

			if (damageData.entityDamageSource.EntityData.playerClass == EntityData.PlayerClass.Mage)
			{
				float roll = (float)(systemRandom.NextDouble() * 100);
				if (roll < damageData.entityDamageSource.EntityData.chanceOfDoubleDamage)
					damage *= 2;
			}

			if (damageData.damageIgnoresBlock)
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
