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
		public string ruleName;

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

		public bool EvaluateAndApply(RuleContext ruleContext)
		{
			if (ruleCondition.Evaluate(ruleContext))
			{
				ruleOutcome.Apply(ruleContext);
				return true;
			}
			return false;
		}
	}
}
