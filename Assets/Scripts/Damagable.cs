using System;
using UnityEngine;
using static CardData;

public class DamageData
{
	public readonly int Damage;
	public readonly DamageType DamageType;

	public DamageData(CardData cardData, bool playerCard)
	{
		if (playerCard)
		{
			Damage = cardData.damage;
			DamageType = cardData.damageType;
		}
		else
		{
			Damage = (int)(cardData.damage * 0.66f);
			DamageType = cardData.damageType;
		}
	}
}

public interface IDamagable
{
	void OnHit(DamageData damageData);
}
