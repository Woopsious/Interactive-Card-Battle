using System.Collections.Generic;
using UnityEngine;

public class EnemyMove
{
	public readonly AttackData attackData;

	int cooldownTimer;

	public EnemyMove(AttackData attackData)
	{
		this.attackData = attackData;
		cooldownTimer = 0;
	}

	public bool CanUseAttack()
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
