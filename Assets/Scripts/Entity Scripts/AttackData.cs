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

		[Header("Damage Info")]
		public Damage DamageInfo;

		[Header("Attack use rules")]
		[Range(0f, 1f)]
		public float attackUseChance;

		public string CreateDescription()
		{
			string attackDescription = this.attackDescription;
			attackDescription += "\n";

			if (DamageInfo.DamageValue != 0)
			{
				if (DamageInfo.DamageType == DamageType.physical)
					attackDescription += "\nDeals " + DamageInfo.DamageValue + " physical damage";

				if (DamageInfo.isAoeAttack)
					attackDescription += " each to a max of " + DamageInfo.maxAoeTargets + " targets";

				switch (DamageInfo.multiHitSettings)
				{
					case Damage.MultiHitAttack.No:
					break;
					case Damage.MultiHitAttack.TwoHits:
					attackDescription += " 2x (" + DamageInfo.DamageValue * 2 + ")" ;
					break;
					case Damage.MultiHitAttack.ThreeHits:
					attackDescription += " 3x (" + DamageInfo.DamageValue * 3 + ")";
					break;
					case Damage.MultiHitAttack.FourHits:
					attackDescription += " 4x (" + DamageInfo.DamageValue * 4 + ")";
					break;
				}
			}
			if (DamageInfo.BlockValue != 0)
				attackDescription += "\nBlocks " + DamageInfo.BlockValue + " damage";
			if (DamageInfo.HealValue != 0)
				attackDescription += "\nHeals " + DamageInfo.HealValue + " damage";

			return attackDescription;
		}
	}
}
