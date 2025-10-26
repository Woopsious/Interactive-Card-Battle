using System;
using System.Collections.Generic;
using UnityEngine;
using Woopsious.AbilitySystem;

namespace Woopsious
{
	public interface IDamagable
	{
		void RecieveDamage(DamageData damageData);
		void RecieveBlock(DamageData damageData);
		void RecieveHealing(DamageData damageData);
	}
}
