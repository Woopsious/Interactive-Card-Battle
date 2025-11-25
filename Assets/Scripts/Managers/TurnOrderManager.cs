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

		protected PlayerEntity player;

		public List<Entity> enemyEntities = new();

		//round/turn tracking
		public Entity currentEntityTurn;
		public Entity entityToStartNewRoundOn;
		public int currentRound;

		//events
		public static event Action<int> OnNewRoundStartEvent;
		public static event Action<Entity> OnNewTurnEvent;

		private void Awake()
		{
			Instance = this;
		}

		void OnEnable()
		{
			GameManager.OnShowMapEvent += ResetListsAndEntities;
			GameManager.OnStartCardCombatEvent += CreateTurnOrder;
			GameManager.OnDebugStartCardCombatEvent += DebugCreateTurnOrder;
			SpawnManager.OnPlayerSpawned += AddPlayerOnSpawnComplete;
			SpawnManager.OnEnemySpawned += AddEnemyOnSpawnComplete;
			Entity.OnTurnEndEvent += StartNewTurn;
			Entity.OnEntityDeath += RemoveEntityFromTurnOrder;
		}
		void OnDisable()
		{
			GameManager.OnShowMapEvent -= ResetListsAndEntities;
			GameManager.OnStartCardCombatEvent -= CreateTurnOrder;
			GameManager.OnDebugStartCardCombatEvent += DebugCreateTurnOrder;
			SpawnManager.OnPlayerSpawned -= AddPlayerOnSpawnComplete;
			SpawnManager.OnEnemySpawned -= AddEnemyOnSpawnComplete;
			Entity.OnTurnEndEvent -= StartNewTurn;
			Entity.OnEntityDeath -= RemoveEntityFromTurnOrder;
		}

		//create and start initial turn order
		async void CreateTurnOrder(MapNode mapNode)
		{
			await SpawnManager.SpawnEntitiesForCardBattle(mapNode);

			turnOrder.Clear();
			turnOrder.Add(player);

			foreach (var entity in enemyEntities)
				turnOrder.Add(entity);

			StartInitialTurn(turnOrder[0]);
		}
		async void DebugCreateTurnOrder(List<EntityData> entitiesToSpawn)
		{
			await SpawnManager.DebugSpawnEntities(entitiesToSpawn);

			turnOrder.Clear();
			turnOrder.Add(player);

			foreach (var entity in enemyEntities)
				turnOrder.Add(entity);

			StartInitialTurn(turnOrder[0]);
		}
		void StartInitialTurn(Entity entity)
		{
			currentEntityTurn = entity;
			entityToStartNewRoundOn = entity;
			currentRound = 1;

			OnNewRoundStartEvent?.Invoke(currentRound);
			OnNewTurnEvent?.Invoke(currentEntityTurn);
		}
		void ResetListsAndEntities()
		{
			if (player != null)
				Destroy(player.gameObject);

			if (enemyEntities.Count > 0)
			{
				for (int i = enemyEntities.Count - 1; i >= 0; i--)
					enemyEntities[i].DebugKill();

				enemyEntities.Clear();
			}
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
		}
		void ShouldStartNewRound()
		{
			if (currentEntityTurn == entityToStartNewRoundOn)
			{
				currentRound++;
				OnNewRoundStartEvent?.Invoke(currentRound);
			}
		}

		//set refs for turn order creation
		void AddPlayerOnSpawnComplete(PlayerEntity player)
		{
			this.player = player;
		}
		void AddEnemyOnSpawnComplete(Entity entity)
		{
			enemyEntities.Add(entity);
		}

		//get instanced refs
		public static PlayerEntity Player()
		{
			return Instance.player;
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
