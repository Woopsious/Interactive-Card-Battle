using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental.FileFormat;
using UnityEngine;
using Woopsious.AbilitySystem;

namespace Woopsious
{
	public class CombatLogContext
	{
		public CombatLogEntry EntryType { get; private set; }
		public enum CombatLogEntry
		{
			damage, block, heal, effectDamage, statusEffectGained, statusEffectLost, ruleTrigger, debug
		}

		public Entity SourceEntity { get; private set; }
		public Entity TargetEntity { get; private set; }

		public DamageData DamageDataContext { get; private set; }
		public StatusEffectsData StatusEffect { get; private set; }

		public RuleDefinition RuleDefinition { get; private set; }
		public RuleContext RuleContext { get; private set; }

		public string DebugLogMessage { get; private set; }

		/// <summary>
		/// create logs for damage, block and healing
		/// </summary>
		public CombatLogContext(CombatLogEntry entryType, Entity sourceEntity, Entity targetEntity, DamageData damageDataContext)
		{
			if (entryType == CombatLogEntry.effectDamage || 
				entryType == CombatLogEntry.statusEffectGained || entryType == CombatLogEntry.statusEffectLost || 
				entryType == CombatLogEntry.ruleTrigger)
			{
				Debug.LogError("Invalid log type for DamageDataContext");
				return;
			}

			EntryType = entryType;
			SourceEntity = sourceEntity;
			TargetEntity = targetEntity;
			DamageDataContext = damageDataContext;
		}

		/// <summary>
		/// create logs for damage from effects
		/// </summary>
		public CombatLogContext(CombatLogEntry entryType, Entity effectedEntity, DamageData damageDataContext, StatusEffectsData statusEffect)
		{
			if (entryType != CombatLogEntry.effectDamage)
			{
				Debug.LogError("Invalid log type for damage from StatusEffects");
				return;
			}

			EntryType = entryType;
			SourceEntity = effectedEntity;
			TargetEntity = effectedEntity;
			DamageDataContext = damageDataContext;
			StatusEffect = statusEffect;
		}

		/// <summary>
		/// create logs for effect changes
		/// </summary>
		public CombatLogContext(CombatLogEntry entryType, Entity effectedEntity, StatusEffectsData statusEffect)
		{
			if (entryType != CombatLogEntry.statusEffectGained && entryType != CombatLogEntry.statusEffectLost)
			{
				Debug.LogError("Invalid log type for StatusEffects");
				return;
			}

			EntryType = entryType;
			SourceEntity = effectedEntity;
			TargetEntity = effectedEntity;
			StatusEffect = statusEffect;
		}

		/// <summary>
		/// create logs based on rules being triggered
		/// </summary>
		public CombatLogContext(CombatLogEntry entryType, RuleDefinition ruleDefinition, RuleContext ruleContext)
		{
			if (entryType != CombatLogEntry.ruleTrigger)
			{
				Debug.LogError("Invalid log type for RuleContext");
				return;
			}

			EntryType = entryType;
			RuleDefinition = ruleDefinition;
			RuleContext = ruleContext;
		}

		public CombatLogContext(CombatLogEntry entryType, string debugMessage)
		{
			if (entryType != CombatLogEntry.debug)
			{
				Debug.LogError("Invalid log type for DebugCombatLog");
				return;
			}

			string message = "Debug Combat Log, ";
			message += debugMessage;

			EntryType = entryType;
			DebugLogMessage = message;
		}
	}
}
