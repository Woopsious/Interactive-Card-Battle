using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Woopsious.DamageData;

namespace Woopsious
{
	[CreateAssetMenu(fileName = "AttackData", menuName = "ScriptableObjects/AttackData")]
	[Serializable]
	public class AttackData : ScriptableObject
	{
		[Header("Attack Info")]
		public string attackName;
		public string attackDescription;

		public bool offensive;

		[Header("Damage Data")]
		public DamageData DamageData;

		[Header("Attack use rules")]
		[Range(0f, 1f)]
		public float attackUseChance;

		public string CreateDescription()
		{
			string description = attackDescription;
			description += "\n";

			if (DamageData.DamageValue != 0)
			{
				if (DamageData.damageType == DamageType.physical)
					description += "\nDeals " + DamageData.DamageValue + " physical damage";

				if (DamageData.isAoeAttack)
					description += " each to a max of " + DamageData.maxAoeTargets + " targets";

				switch (DamageData.multiHitSettings)
				{
					case MultiHitAttack.No:
					break;
					case MultiHitAttack.TwoHits:
					description += " 2x (" + DamageData.DamageValue * 2 + ")" ;
					break;
					case MultiHitAttack.ThreeHits:
					description += " 3x (" + DamageData.DamageValue * 3 + ")";
					break;
					case MultiHitAttack.FourHits:
					description += " 4x (" + DamageData.DamageValue * 4 + ")";
					break;
				}
			}
			if (DamageData.BlockValue != 0)
				description += "\nBlocks " + DamageData.BlockValue + " damage";
			if (DamageData.HealValue != 0)
				description += "\nHeals " + DamageData.HealValue + " damage";

			return description;
		}
	}
}
