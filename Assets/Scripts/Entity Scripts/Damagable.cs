using System;
using System.Collections.Generic;
using UnityEngine;

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
		public ValueTypes valueTypes;
		[Flags]
		public enum ValueTypes
		{
			none = 0, dealsDamage = 1, blocks = 2, heals = 4
		}
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

		[Header("Possible Status Effects")]
		public List<StatusEffectsData> statusEffectsForTarget = new();
		public List<StatusEffectsData> statusEffectsForSelf = new();

		public DamageData(Entity entityDamageSource, DamageData damageData) //base
		{
			EntityDamageSource = entityDamageSource;
			DamageReflectable = true;
			DamageIgnoresBlock = false;

			isAoeAttack = damageData.isAoeAttack;
			maxAoeTargets = damageData.maxAoeTargets;
			multiHitSettings = damageData.multiHitSettings;

			valueTypes = damageData.valueTypes;
			damageType = damageData.damageType;
			DamageValue = damageData.DamageValue;

			BlockValue = damageData.BlockValue;
			HealValue = damageData.HealValue;

			//statusEffectsForTarget.Clear();
			statusEffectsForTarget = damageData.statusEffectsForTarget;

			//statusEffectsForSelf.Clear();
			statusEffectsForSelf = damageData.statusEffectsForSelf;
		}

		public DamageData(Entity entityDamageSource, bool damageReflectable, bool damageIgnoresBlock, int damageValue) //simple damage
		{
			EntityDamageSource = entityDamageSource;
			DamageReflectable = damageReflectable;
			DamageIgnoresBlock = damageIgnoresBlock;

			isAoeAttack = false;
			maxAoeTargets = 0;
			multiHitSettings = MultiHitAttack.No;

			valueTypes = ValueTypes.dealsDamage;
			damageType = DamageType.physical;
			DamageValue = damageValue;
		}

		public DamageData(ValueTypes valueType, int value) //simple blocking/healing
		{
			if (valueType == ValueTypes.none)
				Debug.LogError("Value type not set");
			else if (valueType == ValueTypes.blocks)
				BlockValue = value;
			else if (valueType == ValueTypes.heals)
				HealValue = value;
			else
				Debug.LogError("Damage done in different overload method");
		}
	}

	public interface IDamagable
	{
		void RecieveDamage(DamageData damageData);
		void RecieveBlock(DamageData damageData);
		void RecieveHealing(DamageData damageData);
	}
}
