using System.Collections.Generic;
using UnityEngine;
using Woopsious.AbilitySystem;
using Woopsious.ComplexStats;

/// <summary>
/// EXAMPLE OF TYPES OF RULES
/// 
/// if entity has x status effect do y
/// if entity x exists do y to entity
/// 
/// </summary>

namespace Woopsious
{
	[CreateAssetMenu(fileName = "Rule", menuName = "ScriptableObjects/Rule")]
	public class RuleDefinition : ScriptableObject
	{
		[Header("Rule Trigger")]
		public RuleTrigger trigger;
		public enum RuleTrigger
		{
			Unset, cardBattleStart, attack
		}

		[Header("Rule Condition")]
		public RuleCondition ruleCondition;  // Can reference any RuleCondition type (e.g., EntityCondition, StatusEffectCondition)

		[Header("Rule Outcome")]
		public RuleOutcome ruleOutcome;      // Can reference any RuleOutcome type (e.g., EntityOutcome, StatusEffectOutcome)

		public bool EvaluateAndApply(Entity conditionalEntity, Entity outcomeEntity)
		{
			if (ruleCondition.Evaluate(conditionalEntity))
			{
				ruleOutcome.Apply(outcomeEntity);
				return true;
			}
			return false;
		}
	}
}
