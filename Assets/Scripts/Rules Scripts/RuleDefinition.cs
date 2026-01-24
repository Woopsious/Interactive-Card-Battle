using System.Collections.Generic;
using System.Data;
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
			Unset, cardBattleStart, reaction
		}

		[Header("Rule Condition")]
		public RuleCondition ruleCondition;  // Can reference any RuleCondition type (e.g., EntityCondition, StatusEffectCondition)

		[Header("Rule Outcome")]
		public RuleOutcome ruleOutcome;      // Can reference any RuleOutcome type (e.g., EntityOutcome, StatusEffectOutcome)

		public bool EvaluateAndApply(RuleContext ruleContext)
		{
			Debug.LogError($"rule {ruleName} eval");
			if (ruleCondition.Evaluate(ruleContext))
			{
				if (CombatLogUi.instance != null && trigger == RuleTrigger.reaction)
					CombatLogUi.CreateLog(new(CombatLogContext.CombatLogEntry.ruleTrigger, this, ruleContext));

				return true;
			}
			return false;
		}
	}
}
