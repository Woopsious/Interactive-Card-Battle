using System;
using System.Collections.Generic;
using UnityEngine;
using Woopsious.AbilitySystem;

namespace Woopsious
{
	public interface IDamagable
	{
		void ReceiveDamage(DamageData damageData);
		void ReceiveBlock(DamageData damageData);
		void ReceiveHealing(DamageData damageData);
	}
}
