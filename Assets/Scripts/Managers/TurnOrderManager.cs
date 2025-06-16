using System;
using System.Collections.Generic;
using UnityEngine;

public class TurnOrderManager : MonoBehaviour
{
	public static TurnOrderManager Instance;

	public List<Entity> turnOrder = new();

	public PlayerEntity playerEntity;

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
		SpawnManager.OnStartGame += CreateTurnOrder;
		SpawnManager.OnPlayerSpawned += SetPlayer;
		SpawnManager.OnEnemySpawned += AddEnemyToList;
		Entity.OnTurnEndEvent += StartNewTurn;
	}
	void OnDisable()
	{
		SpawnManager.OnStartGame -= CreateTurnOrder;
		SpawnManager.OnPlayerSpawned -= SetPlayer;
		SpawnManager.OnEnemySpawned -= AddEnemyToList;
		Entity.OnTurnEndEvent -= StartNewTurn;
	}

	//create and start initial turn order
	void CreateTurnOrder()
	{
		turnOrder.Clear();
		turnOrder.Add(playerEntity);

		foreach (var entity in enemyEntities)
			turnOrder.Add(entity);

		StartInitialTurn(turnOrder[0]);
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

	//start new turns
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
	void SetPlayer(PlayerEntity playerEntity)
	{
		this.playerEntity = playerEntity;
	}
	void AddEnemyToList(Entity entity)
	{
		enemyEntities.Add(entity);
	}

	//button click
	public void DebugForceNextTurn()
	{
		StartNewTurn(turnOrder[0]);
	}
}
