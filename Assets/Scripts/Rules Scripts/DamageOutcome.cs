using UnityEngine;
using static Woopsious.DamageData;

namespace Woopsious
{
	// damage outcome
	[CreateAssetMenu(fileName = "DamageOutcome", menuName = "ScriptableObjects/Rule/DamageOutcome")]
	public class DamageOutcome : RuleOutcome
	{
		public DamageData damageData;

		public override void Apply(Entity entity)
		{
			damageData = new(entity, false, false, damageData.DamageValue); //assign self as damage source

			if (damageData.valueTypes.HasFlag(ValueTypes.damages))
				entity.ReceiveDamage(damageData);
			if (damageData.valueTypes.HasFlag(ValueTypes.blocks))
				entity.ReceiveBlock(damageData);
			if (damageData.valueTypes.HasFlag(ValueTypes.heals))
				entity.ReceiveHealing(damageData);
		}
	}
}
