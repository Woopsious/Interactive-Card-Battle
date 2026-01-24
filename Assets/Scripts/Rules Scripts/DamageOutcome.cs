using UnityEngine;
using static Woopsious.DamageData;

namespace Woopsious
{
	// damage outcome
	[CreateAssetMenu(fileName = "DamageOutcome", menuName = "ScriptableObjects/Rule/DamageOutcome")]
	public class DamageOutcome : RuleOutcome
	{
		public ValueTypes applyOutcomeOn;
		public DamageData damageSettings;

		public override void Apply(RuleContext ruleContext)
		{
			//ignore self, will need expanding if for instance u get bonus healing on urself whilst under x effect etc...
			//if (ruleContext.DamageDataContext.EntityDamageSource == ruleContext.ConditionalEntity) return;

			DamageData outcomeDamage = new(ruleContext.ConditionalEntity, damageSettings);

			// apply all outcomes when atleast 1 flag matches
			if ((applyOutcomeOn & ruleContext.DamageDataContext.valueTypes) == ValueTypes.none)
				return;

			if (outcomeDamage.valueTypes.HasFlag(ValueTypes.damages))
				ruleContext.OutcomeEntity.ReceiveDamage(outcomeDamage);

			if (outcomeDamage.valueTypes.HasFlag(ValueTypes.blocks))
				ruleContext.OutcomeEntity.ReceiveBlock(outcomeDamage);

			if (outcomeDamage.valueTypes.HasFlag(ValueTypes.heals))
				ruleContext.OutcomeEntity.ReceiveHealing(outcomeDamage);
		}
	}
}
