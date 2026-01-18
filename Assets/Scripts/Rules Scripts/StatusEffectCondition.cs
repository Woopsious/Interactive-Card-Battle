using UnityEngine;
using Woopsious.AbilitySystem;

namespace Woopsious
{
	// Status effect condition example
	[CreateAssetMenu(fileName = "StatusEffectCondition", menuName = "ScriptableObjects/Rule/StatusEffectCondition")]
	public class StatusEffectCondition : RuleCondition
	{
		public StatusEffectsData requiredEffect;

		public override bool Evaluate(Entity entity)
		{
			foreach (var effect in entity.StatusEffectsHandler.currentStatusEffects)
			{
				if (effect.StatusEffectsData == requiredEffect)
					return true;
			}
			return false;
		}
	}
}
