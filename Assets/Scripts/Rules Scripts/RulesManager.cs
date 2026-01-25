using System.Collections.Generic;
using UnityEngine;
using static Woopsious.RuleDefinition;

namespace Woopsious
{
	public class RulesManager : MonoBehaviour
	{
		public static RulesManager Instance;

		public List<RuleDefinition> rulesList = new();

		private HashSet<RuleDefinition> alreadyTriggeredRulesThisTurn = new();

		private void Awake()
		{
			Instance = this;
		}

		private void OnEnable()
		{
			TurnOrderManager.OnStartTurn += ResetRulesTriggeredThisTurn;
		}
		private void OnDisable()
		{
			TurnOrderManager.OnStartTurn -= ResetRulesTriggeredThisTurn;
		}

		public static void CheckRules(RuleTrigger ruleTrigger, RuleContext ruleContext)
		{
			foreach (RuleDefinition rule in Instance.rulesList)
			{
				if (rule.trigger == ruleTrigger)
				{
					if (rule.Evaluate(ruleContext))
					{
						RecordAndLogTriggeredRulesOnce(rule, ruleContext);
						rule.Apply(ruleContext);
					}
				}
			}
		}

		private void ResetRulesTriggeredThisTurn(Entity entity)
		{
			alreadyTriggeredRulesThisTurn.Clear();
		}
		private static void RecordAndLogTriggeredRulesOnce(RuleDefinition ruleDefinition, RuleContext ruleContext)
		{
			if (!Instance.alreadyTriggeredRulesThisTurn.Contains(ruleDefinition))
			{
				CombatLogUi.CreateLog(new(CombatLogContext.CombatLogEntry.ruleTrigger, ruleDefinition, ruleContext));
				Instance.alreadyTriggeredRulesThisTurn.Add(ruleDefinition);
			}
		}
	}
}
