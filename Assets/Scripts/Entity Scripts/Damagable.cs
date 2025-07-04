using System;
using UnityEngine;

namespace Woopsious
{
	public class DamageData
	{
		public readonly int damage;
		public readonly DamageType damageType;
		public enum DamageType
		{
			block, heal, physical
		}

		public DamageData(int damage, DamageType damageType)
		{
			this.damage = damage;
			this.damageType = damageType;
		}
	}

	public interface IDamagable
	{
		void OnHit(DamageData damageData);
	}
}
