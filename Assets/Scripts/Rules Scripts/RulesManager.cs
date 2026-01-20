using System.Collections.Generic;
using UnityEngine;
using static Woopsious.RuleDefinition;

namespace Woopsious
{
	public class RulesManager : MonoBehaviour
	{
		public static RulesManager Instance;

		public List<RuleDefinition> rulesList = new();

		private void Awake()
		{
			Instance = this;
		}

		public static void CheckRules(RuleTrigger ruleTrigger, Entity conditionEntity, Entity outcomeEntity)
		{
			foreach (RuleDefinition rule in Instance.rulesList)
			{
				if (rule.trigger == ruleTrigger)
					rule.EvaluateAndApply(conditionEntity, outcomeEntity);
			}
		}
	}
}
