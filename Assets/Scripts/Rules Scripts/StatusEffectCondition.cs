using UnityEngine;
using Woopsious.AbilitySystem;

namespace Woopsious
{
	// Status effect condition example
	[CreateAssetMenu(fileName = "StatusEffectCondition", menuName = "ScriptableObjects/Rule/StatusEffectCondition")]
	public class StatusEffectCondition : RuleCondition
	{
		public StatusEffectsData requiredEffect;

		public override bool Evaluate(RuleContext ruleContext)
		{
			foreach (var effect in ruleContext.ConditionalEntity.StatusEffectsHandler.currentStatusEffects)
			{
				if (effect.StatusEffectsData == requiredEffect)
					return true;
			}
			return false;
		}
	}
}
