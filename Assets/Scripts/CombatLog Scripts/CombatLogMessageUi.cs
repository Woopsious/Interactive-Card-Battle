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

		private string LogDamageMessage(CombatLogContext combatLogContext, string message)
		{
			message += $"{combatLogContext.SourceEntity} dealt {combatLogContext.DamageDataContext.DamageValue} to {combatLogContext.TargetEntity}";
			return message;
		}
		private string LogBlockMessage(CombatLogContext combatLogContext, string message)
		{
			if (combatLogContext.SourceEntity == combatLogContext.TargetEntity)
				message += $"{combatLogContext.SourceEntity} defends self for {combatLogContext.DamageDataContext.BlockValue}";
			else
			{
				message += 
					$"{combatLogContext.SourceEntity} defends {combatLogContext.TargetEntity} for {combatLogContext.DamageDataContext.BlockValue}";
			}
			return message;
		}
		private string LogHealMessage(CombatLogContext combatLogContext, string message)
		{
			if (combatLogContext.SourceEntity == combatLogContext.TargetEntity)
				message += $"{combatLogContext.SourceEntity} heals self for {combatLogContext.DamageDataContext.HealValue}";
			else
			{
				message +=
					$"{combatLogContext.SourceEntity} heals {combatLogContext.TargetEntity} for {combatLogContext.DamageDataContext.HealValue}";
			}
			return message;
		}

		private string LogLostStatusEffectsMessage(CombatLogContext combatLogContext, string message)
		{
			message += $"{combatLogContext.SourceEntity} lost ";

			int count = 0;
			foreach (StatusEffectsData statusEffect in combatLogContext.LostStatusEffects)
			{
				message += $"{statusEffect.effectName}, ";
				count++;
			}

			if (count == 1)
				message = $"{RichTextManager.RemoveLastComma(message)} effect";
			else
				message = $"{RichTextManager.RemoveLastComma(message)} effects";

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
	}
}
