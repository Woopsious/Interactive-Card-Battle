using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static Woopsious.DamageData;

namespace Woopsious
{
	public class EntityMoves : MonoBehaviour
	{
		Entity entity;

		public List<EnemyMoveSet> enemyMovesList = new();
		public AttackData previousUsedMove;
		public int previousUsedMoveIndex;

		public static event Action<Entity, AttackData> OnEnemyMoveFound;
		public static event Action OnEnemyAttack;
		public static event Action OnEnemyAttackCancel;

		CancellationTokenSource cancellationToken = new();

		void Awake()
		{
			entity = GetComponent<Entity>();
		}

		void OnEnable()
		{
			TurnOrderManager.OnNewRoundStartEvent += NewRoundStart;
			Entity.OnTurnEndEvent += CancelMove;
		}
		void OnDisable()
		{
			TurnOrderManager.OnNewRoundStartEvent -= NewRoundStart;
			Entity.OnTurnEndEvent -= CancelMove;
		}

		public void InitilizeMoveSet(Entity entity)
		{
			if (entity.EntityData.moveSetOrder.Count == 0)
			{
				Debug.LogError("Entity move set order at 0");
				return;
			}

			previousUsedMove = null;
			previousUsedMoveIndex = -1;

			enemyMovesList.Clear();

			foreach (MoveSetData moveSet in entity.EntityData.moveSetOrder)
				enemyMovesList.Add(new EnemyMoveSet(entity, moveSet));
		}

		//new enemy entity attacks
		public async void PickNextMove()
		{
			cancellationToken = new CancellationTokenSource();

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

				entity.EndTurn();
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
			OnEnemyMoveFound?.Invoke(entity, attackData);

			await Task.Delay(3000);
			if (token.IsCancellationRequested)
			{
				OnEnemyAttackCancel?.Invoke();
				return;
			}

			if (TurnOrderManager.Player() == null) return;

			CardUi card = SpawnManager.SpawnCard();
			card.SetupCard(entity, attackData, false);

			if (card.Offensive)
				card.cardHandler.EnemyUseCard(entity, TurnOrderManager.Player());
			else
				card.cardHandler.EnemyUseCard(entity, entity);

			OnEnemyAttack?.Invoke();
		}

		//events
		void NewRoundStart(int currentRound)
		{
			foreach (EnemyMoveSet move in enemyMovesList)
				move.NewRound();
		}
		void CancelMove(Entity entity)
		{
			if (entity != this.entity) return;
			cancellationToken.Cancel();
		}
	}

	public class EnemyMoveSet
	{
		public readonly Entity entity;

		public List<EnemyMove> moves = new();

		public EnemyMoveSet(Entity entity, MoveSetData moveSetData)
		{
			this.entity = entity;

			foreach (AttackData attackData in moveSetData.moveSetMoves)
				moves.Add(new EnemyMove(attackData));
		}

		//check if move set has any move usable
		public bool CanUseMoveFromMoveSet()
		{
			foreach (EnemyMove move in moves)
			{
				if (move.CanUseMove()) return true;
			}
			//no usable moves found in move set
			return false;
		}

		//use move (should never return null)
		public EnemyMove UseMove()
		{
			EnemyMove moveToUse = PickRandomMoveFromList(moves);
			moveToUse.UseAttack();
			return moveToUse;
		}

		//pick random move from moves available based on weighted use chance (excluding healing moves)
		EnemyMove PickRandomMoveFromList(List<EnemyMove> availableMoves)
		{
			float totalMoveUseChance = 0;
			List<float> moveUseChanceTable = new();

			foreach (EnemyMove move in availableMoves)
			{
				totalMoveUseChance += move.attackData.attackUseChance;
				moveUseChanceTable.Add(move.attackData.attackUseChance);
			}

			float rand = UnityEngine.Random.Range(0, totalMoveUseChance);
			float cumalativeChance = 0;

			for (int i = 0; i < moveUseChanceTable.Count; i++) //pick move to use based on weighted chance
			{
				cumalativeChance += moveUseChanceTable[i];

				if (rand <= cumalativeChance)
					return availableMoves[i];
			}

			return null;
		}

		//update turn cooldowns on new round
		public void NewRound()
		{
			foreach (EnemyMove move in moves)
				move.NewRound();
		}
	}

	public class EnemyMove
	{
		public readonly AttackData attackData;

		public EnemyMove(AttackData attackData)
		{
			this.attackData = attackData;
		}

		public bool CanUseMove()
		{
			return true;
		}

		public void UseAttack()
		{
			//noop
		}

		public void NewRound()
		{
			//noop
		}
	}
}
