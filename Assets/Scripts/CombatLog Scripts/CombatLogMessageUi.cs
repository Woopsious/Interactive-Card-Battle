using TMPro;
using UnityEngine;
using Woopsious.AbilitySystem;

namespace Woopsious
{
	public class CombatLogMessageUi : MonoBehaviour
	{
		private RectTransform rectTransform;
		public TMP_Text logText;

		private void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
		}

		public void CreateLogMessage(CombatLogContext combatLogContext)
		{
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
		private string LogDamageMessage(CombatLogContext combatLogContext, string message)
		{
			message += $"{GetEntityName(combatLogContext, true)} dealt {combatLogContext.DamageDataContext.DamageValue} " +
				$"damage to {GetEntityName(combatLogContext, false)}";
			return message;
		}
		private string LogBlockMessage(CombatLogContext combatLogContext, string message)
		{
			if (combatLogContext.SourceEntity == combatLogContext.TargetEntity)
				message += $"{GetEntityName(combatLogContext, true)} defends self for {combatLogContext.DamageDataContext.BlockValue}";
			else
			{
				message += $"{GetEntityName(combatLogContext, true)} defends {GetEntityName(combatLogContext, false)} " +
					$"for {combatLogContext.DamageDataContext.BlockValue}";
			}
			return message;
		}
		private string LogHealMessage(CombatLogContext combatLogContext, string message)
		{
			if (combatLogContext.SourceEntity == combatLogContext.TargetEntity)
				message += $"{GetEntityName(combatLogContext, true)} heals self for {combatLogContext.DamageDataContext.HealValue}";
			else
			{
				message += $"{GetEntityName(combatLogContext, true)} heals {GetEntityName(combatLogContext, false)} " +
					$"for {combatLogContext.DamageDataContext.HealValue}";
			}
			return message;
		}

		//effect logging
		private string LogGainedStatusEffectMessage(CombatLogContext combatLogContext, string message)
		{
			message += $"{GetEntityName(combatLogContext, true)} gained {GetEffectName(combatLogContext)} effect";
			return message;
		}
		private string LogLostStatusEffectsMessage(CombatLogContext combatLogContext, string message)
		{
			message += $"{GetEntityName(combatLogContext, true)} lost {GetEffectName(combatLogContext)} effect";
			return message;
		}

		//values from effects logging
		private string LogDamageFromEffectMessage(CombatLogContext combatLogContext, string message)
		{
			message += $"{GetEntityName(combatLogContext, true)} recieved {combatLogContext.DamageDataContext.DamageValue} " +
				$"damage from {GetEffectName(combatLogContext)} effect";
			return message;
		}

		//whilst rule system not mostly finished just log rule name 
		private string LogRuleTriggerMessage(CombatLogContext combatLogContext, string message)
		{
			message += $"{combatLogContext.RuleDefinition.ruleName} triggered";

			/*
			if (combatLogContext.RuleDefinition.trigger == RuleDefinition.RuleTrigger.attack)
				message = GetRuleAttackTriggerLog(message);
			else
				Debug.LogError("Trigger Type not handled");
			*/

			return message;
		}
		private string GetRuleAttackTriggerLog(string message)
		{
			return message;
		}

		//helpers
		private string GetEntityName(CombatLogContext combatLogContext, bool getSourceEntityName)
		{
			if (getSourceEntityName)
			{
				if (combatLogContext.SourceEntity.EntityData.isPlayer)
					return "Player";
				else
					return $"{combatLogContext.SourceEntity.EntityData.entityName}";
			}
			else
			{
				if (combatLogContext.TargetEntity.EntityData.isPlayer)
					return "Player";
				else
					return $"{combatLogContext.TargetEntity.EntityData.entityName}";
			}
		}
		private string GetEffectName(CombatLogContext combatLogContext)
		{
			return combatLogContext.StatusEffect.effectName;
		}
	}
}
