using System.Collections.Generic;
using UnityEngine;
using Woopsious.AbilitySystem;

namespace Woopsious
{
	// Status effect outcome
	[CreateAssetMenu(fileName = "StatusEffectOutcome", menuName = "ScriptableObjects/Rule/StatusEffectOutcome")]
	public class StatusEffectOutcome : RuleOutcome
	{
		public List<StatusEffectsData> effects = new();

		public override void Apply(RuleContext ruleContext)
		{
			ruleContext.OutcomeEntity.StatusEffectsHandler.AddStatusEffects(effects);
		}
	}
}
