using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static Woopsious.DamageData;

namespace Woopsious
{
	public class Entity : MonoBehaviour, IDamagable
	{
		public EntityData entityData;

		public TMP_Text entityNameText;
		public TMP_Text entityHealthText;
		public TMP_Text entityblockText;
		public GameObject turnIndicator;

		public int health;
		public int block;

		public List<EnemyMoveSet> enemyMovesList = new();
		public AttackData previousUsedMove;
		public int previousUsedMoveIndex;

		public static event Action<AttackData> OnEnemyMoveFound;
		public static event Action OnEnemyAttack;
		public static event Action OnEnemyAttackCancel;

		public static event Action<Entity> OnTurnEndEvent;
		public static event Action<Entity> OnEntityDeath;

		CancellationTokenSource cancellationToken = new();
		private readonly System.Random systemRandom = new();

		void Start()
		{
			SetupEntity();
		}

		void OnEnable()
		{
			TurnOrderManager.OnNewRoundStartEvent += NewRoundStart;
			TurnOrderManager.OnNewTurnEvent += StartTurn;
		}
		void OnDisable()
		{
			TurnOrderManager.OnNewRoundStartEvent -= NewRoundStart;
			TurnOrderManager.OnNewTurnEvent -= StartTurn;
		}

		void SetupEntity()
		{
			if (entityData == null)
			{
				Debug.LogError("Entity data null");
				return;
			}

			string cardName = entityData.entityName;
			gameObject.name = cardName;
			entityNameText.text = cardName;
			health = entityData.maxHealth;
			block = 0;
			UpdateUi();

			if (entityData.isPlayer) return;

			SetupEnemyAttacks();
		}
		void SetupEnemyAttacks()
		{
			if (entityData.moveSetOrder.Count == 0)
			{
				Debug.LogError("Entity move set order at 0");
				return;
			}

			previousUsedMove = null;
			previousUsedMoveIndex = -1;

			enemyMovesList.Clear();

			foreach (MoveSetData moveSet in entityData.moveSetOrder)
				enemyMovesList.Add(new EnemyMoveSet(this, moveSet));
		}

		//start new round event
		void NewRoundStart(int currentRound)
		{
			foreach (EnemyMoveSet move in enemyMovesList)
				move.NewRound();
		}

		//start/end turn events
		protected virtual void StartTurn(Entity entity)
		{
			if (entity != this) return; //not this entities turn

			cancellationToken = new CancellationTokenSource();
			block = 0;
			UpdateUi();

			if (entityData.isPlayer) return; //if is player shouldnt need to do anything else as other scripts handle it

			turnIndicator.SetActive(true);
			PickNextMove();
		}
		public virtual void EndTurn()
		{
			turnIndicator.SetActive(false);
			OnTurnEndEvent?.Invoke(this);
			cancellationToken.Cancel();
		}

		//new enemy entity attacks
		async void PickNextMove()
		{
			int nextMoveIndex = previousUsedMoveIndex + 1;

			//Debug.LogError("next attack index: " + nextMoveIndex + " | attacks count: " + enemyMovesList.Count);

			if (nextMoveIndex >= enemyMovesList.Count) //end of attack queue, loop back
			{
				//Debug.LogError("end of attack queue");

				previousUsedMoveIndex = -1;
				PickNextMove();
				return;
			}

			if (enemyMovesList[nextMoveIndex].CanUseMoveFromMoveSet())
			{
				//Debug.LogError("using attack");

				await UseMove(cancellationToken.Token, nextMoveIndex);
				return;
			}
			else if (HasMoveAvailable()) //try next move set
			{
				//Debug.LogError("has attack trying next");

				previousUsedMoveIndex++;
				PickNextMove();
				return;
			}
			else //end turn if no moves available in move sets to avoid endless loop
			{
				//Debug.LogError("end turn");

				EndTurn();
				return;
			}
		}
		bool HasMoveAvailable()
		{
			foreach (EnemyMoveSet moveSet in enemyMovesList)
			{
				if (moveSet.CanUseMoveFromMoveSet())
					return true;
			}

			return false;
		}
		async Task UseMove(CancellationToken token, int nextMoveIndex)
		{
			await Task.Delay(100);
			AttackData attackData = enemyMovesList[nextMoveIndex].UseMove().attackData;

			previousUsedMoveIndex = nextMoveIndex;
			previousUsedMove = attackData;
			OnEnemyMoveFound?.Invoke(attackData);

			await Task.Delay(3000);
			if (token.IsCancellationRequested)
			{
				OnEnemyAttackCancel?.Invoke();
				return;
			}

			if (TurnOrderManager.Player() == null) return;

			CardUi card = SpawnManager.SpawnCard();
			card.SetupCard(attackData);

			if (card.Offensive)
				card.throwableCard.EnemyThrowCard(this, TurnOrderManager.Player().transform.localPosition);
			else
				card.throwableCard.EnemyUseCard(this);

			OnEnemyAttack?.Invoke();
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
			if (health > entityData.maxHealth)
				health = entityData.maxHealth;

			UpdateUi();
		}
		protected void RecieveDamage(DamageData damageData)
		{
			int damage = damageData.damage;

			if (damageData.entityDamageSource.entityData.playerClass == EntityData.PlayerClass.Mage)
			{
				float roll = (float)(systemRandom.NextDouble() * 100);
				if (roll < damageData.entityDamageSource.entityData.chanceOfDoubleDamage)
					damage *= 2;
			}

			if (damageData.damageIgnoresBlock)
				health -= damage;
			else
			{
				damage = GetBlockedDamage(damage);
				health -= damage;
			}

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
				Destroy(gameObject);
			}
		}

		void UpdateUi()
		{
			entityHealthText.text = "HEALTH\n" + health + "/" + entityData.maxHealth;
			entityblockText.text = "Block\n" + block;
		}
	}
}
