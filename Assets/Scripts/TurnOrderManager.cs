using System;
using System.Collections.Generic;
using UnityEngine;

public class TurnOrderManager : MonoBehaviour
{
	public static TurnOrderManager Instance;

	public List<Entity> turnOrder = new();

	public Entity playerEntity;

	public List<Entity> enemyEntities = new();

	public Entity currentEntityTurn;

	public static event Action<Entity> OnNewTurnEvent;
	public static event Action<Entity> OnPlayerTurnStartEvent;

	private void Awake()
	{
		Instance = this;
		CreateTurnOrder();
	}

	void OnEnable()
	{
		Entity.OnTurnEndEvent += StartNewTurn;
	}
	void OnDisable()
	{
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
		OnNewTurnEvent?.Invoke(currentEntityTurn);

		if (currentEntityTurn.entityData.isPlayer)
			OnPlayerTurnStartEvent?.Invoke(currentEntityTurn);
	}

	//add/remove as turns happen
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
		OnNewTurnEvent?.Invoke(currentEntityTurn);

		if (currentEntityTurn.entityData.isPlayer)
			OnPlayerTurnStartEvent?.Invoke(currentEntityTurn);
	}

	//button click
	public void DebugForceNextTurn()
	{
		StartNewTurn(turnOrder[0]);
	}
}
