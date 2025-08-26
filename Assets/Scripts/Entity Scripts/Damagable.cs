using System;
using System.Collections.Generic;
using UnityEngine;
using static Woopsious.DamageData;

namespace Woopsious
{
	[Serializable]
	public class DamageData
	{
		public Entity EntityDamageSource { get; private set; }
		public bool DamageReflectable { get; private set; }
		public bool DamageIgnoresBlock { get; private set; }

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
		public DamageType damageType;
		public enum DamageType
		{
			physical
		}
		[Tooltip("if marked as aoe or is multi hit attack, value should represent damage done to single target")]
		public int DamageValue;

		[Header("Other Values")]
		public int BlockValue;
		public int HealValue;

		public DamageData(Entity entityDamageSource, DamageData damageData) //base
		{
			EntityDamageSource = entityDamageSource;
			DamageReflectable = true;
			DamageIgnoresBlock = false;

			isAoeAttack = damageData.isAoeAttack;
			maxAoeTargets = damageData.maxAoeTargets;
			multiHitSettings = damageData.multiHitSettings;

			damageType = damageData.damageType;
			DamageValue = damageData.DamageValue;

			BlockValue = damageData.BlockValue;
			HealValue = damageData.HealValue;
		}

		public DamageData(Entity entityDamageSource, bool reflectedDamage, int damageValue) //reflected damage
		{
			EntityDamageSource = entityDamageSource;
			DamageReflectable = false;
			DamageIgnoresBlock = true;

			isAoeAttack = false;
			maxAoeTargets = 0;
			multiHitSettings = MultiHitAttack.No;

			damageType = DamageType.physical;
			DamageValue = damageValue;
		}

		public DamageData(Entity entityDamageSource, int damageValue) //simple damage
		{
			EntityDamageSource = entityDamageSource;
			DamageReflectable = true;
			DamageIgnoresBlock = false;

			isAoeAttack = false;
			maxAoeTargets = 0;
			multiHitSettings = MultiHitAttack.No;

			damageType = DamageType.physical;
			DamageValue = damageValue;
		}

		public DamageData(bool isBlockValue, int value) //simple blocking/healing
		{
			if (isBlockValue)
				BlockValue = value;
			else
				HealValue = value;
		}
	}

	public interface IDamagable
	{
		void RecieveDamage(DamageData damageData);
		void RecieveBlock(DamageData damageData);
		void RecieveHealing(DamageData damageData);
	}
}
