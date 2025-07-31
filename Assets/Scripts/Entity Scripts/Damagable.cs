using System;
using UnityEngine;

namespace Woopsious
{
	public class DamageData
	{
		public Entity entityDamageSource;
		public bool damageIgnoresBlock;

		public readonly int damage;
		public readonly DamageType damageType;
		public enum DamageType
		{
			block, heal, physical
		}

		public DamageData(Entity entityDamageSource, bool damageIgnoresBlock, int damage, DamageType damageType)
		{
			this.entityDamageSource = entityDamageSource;
			this.damageIgnoresBlock = damageIgnoresBlock;
			this.damage = damage;
			this.damageType = damageType;
		}
	}

	public interface IDamagable
	{
		void OnHit(DamageData damageData);
	}
}
