using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EXAMPLE OF TYPES OF RULES
/// 
/// if entity has x status effect do y
/// if entity x exists do y to entity
/// 
/// </summary>

namespace Woopsious
{
	[CreateAssetMenu(fileName = "Rule", menuName = "ScriptableObjects/Rule/RuleDefinition")]
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
		public bool hasMultipleConditions;
		public bool allConditionsNeedToBeMet;

		public RuleCondition ruleCondition;  // Can reference any RuleCondition type (e.g., EntityCondition, StatusEffectCondition)
		public List<RuleCondition> ruleConditions = new();

		[Header("Rule Outcome")]
		public bool hasMultipleOutcomes;

		public RuleOutcome ruleOutcome;      // Can reference any RuleOutcome type (e.g., EntityOutcome, StatusEffectOutcome)
		public List<RuleOutcome> ruleOutcomes = new();

		public bool Evaluate(RuleContext ruleContext)
		{
			if (!hasMultipleConditions)
				return ruleCondition.Evaluate(ruleContext);
			else
			{
				if (ruleConditions.Count == 0)
					Debug.LogError("Rule Conditions list is empty");

				foreach (RuleCondition condition in ruleConditions)
				{
					bool conditionResult = condition.Evaluate(ruleContext);

					if (allConditionsNeedToBeMet && conditionResult == false)
						return false;
					else if (!allConditionsNeedToBeMet && conditionResult == true)
						return true;
				}

				return allConditionsNeedToBeMet;
			}
		}
		public void Apply(RuleContext ruleContext)
		{
			if (!hasMultipleOutcomes)
				ruleOutcome.Apply(ruleContext);
			else
			{
				if (ruleOutcomes.Count == 0)
					Debug.LogError("Rule Outcomes list is empty");

				foreach (RuleOutcome outcome in ruleOutcomes)
					outcome.Apply(ruleContext);
			}
		}
	}
}
