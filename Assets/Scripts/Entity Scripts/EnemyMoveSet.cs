using System;
using System.Collections.Generic;
using static DamageData;

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
			if (move.CanUseMove())
			{
				if (move.attackData.damageType == DamageType.heal && NeedsHealing(move))
					return true;
				else if (move.attackData.damageType != DamageType.heal)
					return true;
				else 
					return false;
			}
		}
		//no attack found in move
		return false;
	}

	//use move (should never return null)
	public EnemyMove UseMove()
	{
		EnemyMove moveToUse = HasHealOptionAndNeedsHeal();

		if (moveToUse == null)
		{
			List<EnemyMove> moveOptions = GetAvailableMoveOptions();

			if (moveOptions != null)
				moveToUse = PickRandomMoveFromList(moveOptions);
		}

		moveToUse.UseAttack();
		return moveToUse;
	}

	//additional logic
	EnemyMove HasHealOptionAndNeedsHeal()
	{
		foreach (EnemyMove move in moves)
		{
			if (move.attackData.damageType != DamageType.heal) continue;

			if (NeedsHealing(move))
				return move;
		}

		return null;
	}
	bool NeedsHealing(EnemyMove move)
	{
		int missingHealh = entity.entityData.maxHealth - entity.health;

		if (missingHealh > move.attackData.damage * 0.5f)
			return true;
		else
			return false;
	}

	//create new list of moves from available moves
	List<EnemyMove> GetAvailableMoveOptions()
	{
		List<EnemyMove> moveOptions = new();

		foreach (EnemyMove move in moves)
		{
			if (move.CanUseMove() && move.attackData.damageType != DamageType.heal) //exclude heal cards
				moveOptions.Add(move);
		}

		if (moveOptions.Count > 0)
			return moveOptions;
		else
			return null;
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

	public int cooldownTimer;

	public EnemyMove(AttackData attackData)
	{
		this.attackData = attackData;
		cooldownTimer = 0;
	}

	public bool CanUseMove()
	{
		if (cooldownTimer <= 0)
			return true;
		else 
			return false;
	}

	public void UseAttack()
	{
		cooldownTimer = attackData.attackCooldownTurns;
	}

	public void NewRound()
	{
		cooldownTimer--;
	}
}

[Serializable]
public class MoveSetData
{
	public List<AttackData> moveSetMoves = new();
}
