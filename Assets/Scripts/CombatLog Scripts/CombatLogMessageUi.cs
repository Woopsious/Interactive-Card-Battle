using TMPro;
using UnityEngine;
using Woopsious.AbilitySystem;

namespace Woopsious
{
	public class CombatLogMessageUi : MonoBehaviour
	{
		private RectTransform rectTransform;
		public TMP_Text logText;

		public void CreateLogMessage(CombatLogContext combatLogContext)
		{
			rectTransform = rectTransform != null ? rectTransform : GetComponent<RectTransform>();

			string message = $"R{TurnOrderManager.Instance.currentRound}. T{TurnOrderManager.Instance.currentTurn}. ";

			switch (combatLogContext.EntryType)
			{
				case CombatLogContext.CombatLogEntry.damage:
				message = LogDamageMessage(combatLogContext, message);
				break;
				case CombatLogContext.CombatLogEntry.block:
				message = LogBlockMessage(combatLogContext, message);
				break;
				case CombatLogContext.CombatLogEntry.heal:
				message = LogHealMessage(combatLogContext, message);
				break;
				case CombatLogContext.CombatLogEntry.effectDamage:
				message = LogDamageFromEffectMessage(combatLogContext, message);
				break;
				case CombatLogContext.CombatLogEntry.statusEffectGained:
				message = LogGainedStatusEffectMessage(combatLogContext, message);
				break;
				case CombatLogContext.CombatLogEntry.statusEffectLost:
				message = LogLostStatusEffectsMessage(combatLogContext, message);
				break;
				case CombatLogContext.CombatLogEntry.ruleTrigger:
				message = LogRuleTriggerMessage(combatLogContext, message);
				break;
				case CombatLogContext.CombatLogEntry.debug:
				message = combatLogContext.DebugLogMessage;
				break;
			}

			logText.text = message;
			rectTransform.sizeDelta = new Vector2(300, logText.preferredHeight + 10f);
		}

		//value types logging
		private string LogDamageMessage(CombatLogContext context, string message)
		{
			message += $"{GetEntityName(context, true)} dealt {context.DamageDataContext.DamageValue} " +
				$"damage to {GetEntityName(context, false)}";
			return message;
		}
		private string LogBlockMessage(CombatLogContext context, string message)
		{
			if (context.SourceEntity == context.TargetEntity)
				message += $"{GetEntityName(context, true)} defends self for {context.DamageDataContext.BlockValue}";
			else
			{
				message += $"{GetEntityName(context, true)} defends {GetEntityName(context, false)} " +
					$"for {context.DamageDataContext.BlockValue}";
			}
			return message;
		}
		private string LogHealMessage(CombatLogContext context, string message)
		{
			if (context.SourceEntity == context.TargetEntity)
				message += $"{GetEntityName(context, true)} heals self for {context.DamageDataContext.HealValue}";
			else
			{
				message += $"{GetEntityName(context, true)} heals {GetEntityName(context, false)} " +
					$"for {context.DamageDataContext.HealValue}";
			}
			return message;
		}

		//effect logging
		private string LogGainedStatusEffectMessage(CombatLogContext context, string message)
		{
			message += $"{GetEntityName(context, true)} gained {GetEffectName(context)} effect";
			return message;
		}
		private string LogLostStatusEffectsMessage(CombatLogContext context, string message)
		{
			message += $"{GetEntityName(context, true)} lost {GetEffectName(context)} effect";
			return message;
		}

		//values from effects logging
		private string LogDamageFromEffectMessage(CombatLogContext context, string message)
		{
			message += $"{GetEntityName(context, true)} recieved {context.DamageDataContext.DamageValue} " +
				$"damage from {GetEffectName(context)} effect";
			return message;
		}

		//whilst rule system not mostly finished just log rule name 
		private string LogRuleTriggerMessage(CombatLogContext context, string message)
		{
			message += $"Rule {GetRuleName(context)} triggered {GetRuleTriggerType(context)}";

			if (context.RuleDefinition.trigger == RuleDefinition.RuleTrigger.reaction)
				return LogRuleDamageReactionMessage(context);

			/*
			if (combatLogContext.RuleDefinition.trigger == RuleDefinition.RuleTrigger.attack)
				message = GetRuleAttackTriggerLog(message);
			else
				Debug.LogError("Trigger Type not handled");
			*/

			return message;
		}
		private string LogRuleDamageReactionMessage(CombatLogContext context)
		{
			return $"{context.RuleContext.ConditionalEntity.EntityData.name} triggred {context.RuleDefinition.ruleName} rule" +
				$"on {context.RuleContext.OutcomeEntity.EntityData.name}";
		}

		//helpers
		private string GetEntityName(CombatLogContext context, bool getSourceEntityName)
		{
			if (getSourceEntityName)
			{
				if (context.SourceEntity.EntityData.isPlayer)
					return "Player";
				else
					return $"{context.SourceEntity.EntityData.entityName}";
			}
			else
			{
				if (context.TargetEntity.EntityData.isPlayer)
					return "Player";
				else
					return $"{context.TargetEntity.EntityData.entityName}";
			}
		}
		private string GetEffectName(CombatLogContext context)
		{
			return context.StatusEffect.effectName;
		}
		private string GetRuleTriggerType(CombatLogContext context)
		{
			return context.RuleDefinition.trigger switch
			{
				RuleDefinition.RuleTrigger.Unset => "(Error Trigger type for rule not set)",
				RuleDefinition.RuleTrigger.cardBattleStart => "on battle start",
				RuleDefinition.RuleTrigger.reaction => "from reaction",
				_ => "(Error Trigger text not set)",
			};
		}
		private string GetRuleName(CombatLogContext context)
		{
			return context.RuleDefinition.ruleName;
		}
	}
}
