using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Woopsious
{
	public class TurnOrderManager : MonoBehaviour
	{
		public static TurnOrderManager Instance;

		public List<Entity> turnOrder = new();

		protected PlayerEntity playerEntity;

		public List<Entity> enemyEntities = new();

		//round/turn tracking
		public Entity currentEntityTurn;
		private Entity entityToStartNewRoundOn;
		public int currentRound;

		//events
		public static event Action OnNewRoundStartEvent;
		public static event Action<Entity> OnNewTurnEvent;
		public static event Action<Entity> OnPlayerTurnStartEvent;

		private void Awake()
		{
			Instance = this;
		}

		void OnEnable()
		{
			GameManager.OnStartCardCombatEvent += CreateTurnOrder;
			SpawnManager.OnPlayerSpawned += AddPlayerOnSpawnComplete;
			SpawnManager.OnEnemySpawned += AddEnemyOnSpawnComplete;
			Entity.OnTurnEndEvent += StartNewTurn;
			Entity.OnEntityDeath += RemoveEntityFromTurnOrder;
		}
		void OnDisable()
		{
			GameManager.OnStartCardCombatEvent -= CreateTurnOrder;
			SpawnManager.OnPlayerSpawned -= AddPlayerOnSpawnComplete;
			SpawnManager.OnEnemySpawned -= AddEnemyOnSpawnComplete;
			Entity.OnTurnEndEvent -= StartNewTurn;
			Entity.OnEntityDeath -= RemoveEntityFromTurnOrder;
		}

		//create and start initial turn order
		async void CreateTurnOrder()
		{
			RemoveAllEntities();
			await SpawnManager.SpawnEntitiesForCardBattle();

			turnOrder.Clear();
			turnOrder.Add(playerEntity);

			foreach (var entity in enemyEntities)
				turnOrder.Add(entity);

			StartInitialTurn(turnOrder[0]);
		}
		void RemoveAllEntities()
		{
			if (playerEntity != null)
				Destroy(playerEntity.gameObject);

			if (enemyEntities.Count > 0)
			{
				for (int i = enemyEntities.Count - 1; i >= 0; i--)
					Destroy(enemyEntities[i].gameObject);

				enemyEntities.Clear();
			}
		}
		void StartInitialTurn(Entity entity)
		{
			currentEntityTurn = entity;
			entityToStartNewRoundOn = entity;
			currentRound = 1;

			OnNewTurnEvent?.Invoke(currentEntityTurn);
			OnNewRoundStartEvent?.Invoke();

			if (currentEntityTurn.entityData.isPlayer)
				OnPlayerTurnStartEvent?.Invoke(currentEntityTurn);
		}

		//add/remove entities as turns happen
		public void AddEntityToTurnOrder(Entity entity)
		{
			turnOrder.Insert(turnOrder.Count, entity);
		}
		public void RemoveEntityFromTurnOrder(Entity entity)
		{
			turnOrder.Remove(entity);
		}

		//start new turns/rounds
		void StartNewTurn(Entity entity)
		{
			RemoveEntityFromTurnOrder(entity); //remove from front of queue then add at back
			AddEntityToTurnOrder(entity);

			currentEntityTurn = turnOrder[0];

			ShouldStartNewRound();
			OnNewTurnEvent?.Invoke(currentEntityTurn);

			if (currentEntityTurn.entityData.isPlayer)
				OnPlayerTurnStartEvent?.Invoke(currentEntityTurn);
		}
		void ShouldStartNewRound()
		{
			if (currentEntityTurn == entityToStartNewRoundOn)
			{
				currentRound += 1;
				OnNewRoundStartEvent?.Invoke();
			}
		}

		//set refs for turn order creation
		void AddPlayerOnSpawnComplete(PlayerEntity playerEntity)
		{
			this.playerEntity = playerEntity;
		}
		void AddEnemyOnSpawnComplete(Entity entity)
		{
			enemyEntities.Add(entity);
		}

		//get instanced refs
		public static Entity Player()
		{
			return Instance.playerEntity;
		}
		public static Entity CurrentEntitiesTurn()
		{
			return Instance.currentEntityTurn;
		}
		public static List<Entity> EnemyEntities()
		{
			return Instance.enemyEntities;
		}

		//end instanced enemies turn
		public static void SkipCurrentEntitiesTurn()
		{
			Instance.currentEntityTurn.EndTurn();
		}
	}
}
