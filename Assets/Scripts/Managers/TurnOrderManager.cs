using System;
using System.Collections.Generic;
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
		public int currentRound;
		public int currentTurn;
		public Entity currentEntityTurn;
		public Entity entityToStartNewRoundOn;

		//events
		public static event Action<int> OnNewRoundStartEvent;
		public static event Action<Entity> OnStartTurn;
		public static event Action<Entity> OnEndTurn;

		private void Awake()
		{
			Instance = this;
		}

		private void OnEnable()
		{
			GameManager.OnGameStateChange += OnGameStateChanges;
			SpawnManager.OnPlayerSpawned += AddPlayerOnSpawnComplete;
			SpawnManager.OnEnemySpawned += AddEnemyOnSpawnComplete;
			Entity.OnEntityDeath += HandleEntityDeaths;
		}
		private void OnDisable()
		{
			GameManager.OnGameStateChange -= OnGameStateChanges;
			SpawnManager.OnPlayerSpawned -= AddPlayerOnSpawnComplete;
			SpawnManager.OnEnemySpawned -= AddEnemyOnSpawnComplete;
			Entity.OnEntityDeath -= HandleEntityDeaths;
		}

		private void OnGameStateChanges(GameManager.GameState gameState)
		{
			if (GameManager.GameState.MapView == gameState)
			{
				ResetListsAndEntities();
			}
			else if (GameManager.GameState.CardCombat == gameState)
			{
				CreateTurnOrder(GameManager.CurrentlyVisitedMapNode);
			}
			else if (GameManager.GameState.debugCombat == gameState)
			{
				DebugCreateTurnOrder(GameManager.instance.debugEnemiesToFight);
			}	
		}

		//create and start initial turn order
		private async void CreateTurnOrder(MapNodeController mapNode)
		{
			await SpawnManager.SpawnEntitiesForCardBattle(mapNode);

			turnOrder.Clear();
			turnOrder.Add(player);

			foreach (var entity in enemyEntities)
				turnOrder.Add(entity);

			StartInitialTurn(turnOrder[0]);
		}
		private async void DebugCreateTurnOrder(List<EntityData> entitiesToSpawn)
		{
			await SpawnManager.DebugSpawnEntities(entitiesToSpawn);

			turnOrder.Clear();
			turnOrder.Add(player);

			foreach (var entity in enemyEntities)
				turnOrder.Add(entity);

			StartInitialTurn(turnOrder[0]);
		}
		private void StartInitialTurn(Entity entity)
		{
			PlayerCardDeckHandler.UpdatePlayerCardDrawPile(true);
			currentEntityTurn = entity;
			entityToStartNewRoundOn = entity;
			currentRound = 1;
			currentTurn = 1;

			HashSet<(Entity, Entity)> checkedPairs = new HashSet<(Entity, Entity)>();

			foreach (Entity conditionalEntity in turnOrder)
			{
				foreach (Entity outcomeEntity in turnOrder)
				{
					var pair = (conditionalEntity, outcomeEntity);

					if (checkedPairs.Contains(pair)) continue;

					RulesManager.CheckRules(RuleDefinition.RuleTrigger.cardBattleStart, new(conditionalEntity, outcomeEntity));
					checkedPairs.Add(pair);
				}
			}

			OnNewRoundStartEvent?.Invoke(currentRound);
			OnStartTurn?.Invoke(entity);
		}
		private void ResetListsAndEntities()
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

		//set refs for turn order creation
		private void AddPlayerOnSpawnComplete(PlayerEntity player)
		{
			this.player = player;
		}
		private void AddEnemyOnSpawnComplete(Entity entity)
		{
			enemyEntities.Add(entity);
		}

		//start new turns/rounds
		private void StartNewTurn(Entity entity)
		{
			RemoveEntityFromTurnOrder(entity); //remove from front of queue then add at back
			AddEntityToTurnOrder(entity);

			currentEntityTurn = turnOrder[0];
			currentTurn++;

			ShouldStartNewRound();
			OnStartTurn?.Invoke(currentEntityTurn);
		}
		private void ShouldStartNewRound()
		{
			if (currentEntityTurn == entityToStartNewRoundOn)
			{
				currentRound++;
				currentTurn = 1;
				OnNewRoundStartEvent?.Invoke(currentRound);
			}
		}

		//start/end turn event
		public static void EndCurrentTurn(Entity entity)
		{
			OnEndTurn?.Invoke(entity);
			Instance.StartNewTurn(entity);
		}

		//track entities alive
		private void HandleEntityDeaths(Entity entity)
		{
			if (GameManager.CurrentGameState != GameManager.GameState.CardCombat) return;

			RemoveEntityFromTurnOrder(entity);

			if (Player() == entity) //return early on player death
			{
				GameManager.EnterCardCombatLoss();
				return;
			}

			int enemiesDead = 0;

			foreach (Entity enemy in EnemyEntities())
			{
				if (enemy.health.Value <= 0)
					enemiesDead++;
			}

			if (enemiesDead < EnemyEntities().Count) return; //win with all enemies dead
			GameManager.EnterCardCombatWin();
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
			EndCurrentTurn(Instance.currentEntityTurn);
		}
	}
}
