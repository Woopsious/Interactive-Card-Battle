using System;
using System.Collections.Generic;
using UnityEngine;
using static Woopsious.DamageData;

namespace Woopsious
{
	public class DamageData
	{
		public Entity EntityDamageSource { get; private set; }
		public bool DamageIgnoresBlock { get; private set; }

		public Damage DamageInfo { get; private set; }
		public enum DamageType
		{
			physical
		}

		public DamageData(Entity entityDamageSource, bool damageIgnoresBlock, Damage damageInfo)
		{
			EntityDamageSource = entityDamageSource;
			DamageIgnoresBlock = damageIgnoresBlock;
			DamageInfo = damageInfo;
		}
	}

	[Serializable]
	public class Damage
	{
		[Header("Aoe Settings")]
		public bool isAoeAttack;
		public int maxAoeTargets;

		[Header("Multihit Settings")]
		public MultiHitAttack multiHitSettings;
		public enum MultiHitAttack
		{
			No, TwoHits, ThreeHits, FourHits
		}

		[Header("Damage Values")]
		public DamageType DamageType;
		[Tooltip("if marked as aoe or is multi hit attack, value should represent damage doen to single target")]
		public int DamageValue;

		[Header("Other Values")]
		public int BlockValue;
		public int HealValue;

		public Damage(DamageType damageType, int damageValue, int blockValue, int healValue)
		{
			DamageValue = damageValue;
			DamageType = damageType;
			BlockValue = blockValue;
			HealValue = healValue;
		}
	}

	public interface IDamagable
	{
		void RecieveDamage(DamageData damageData);
		void RecieveBlock(DamageData damageData);
		void RecieveHealing(DamageData damageData);
	}
}
