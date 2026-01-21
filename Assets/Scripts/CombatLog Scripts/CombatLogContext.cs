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
			damage, block, heal, statusEffectLost, ruleTrigger, debug
		}

		public Entity SourceEntity { get; private set; }
		public Entity TargetEntity { get; private set; }

		public DamageData DamageDataContext { get; private set; }
		public List<StatusEffectsData> LostStatusEffects { get; private set; } = new();

		public RuleDefinition RuleDefinition { get; private set; }
		public RuleContext RuleContext { get; private set; }

		public string DebugLogMessage { get; private set; }

		/// <summary>
		/// create logs for damage, block and healing
		/// </summary>
		public CombatLogContext(CombatLogEntry entryType, Entity sourceEntity, Entity targetEntity, DamageData damageDataContext)
		{
			if (entryType == CombatLogEntry.statusEffectLost || entryType == CombatLogEntry.ruleTrigger)
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
		/// create logs for effect changes
		/// </summary>
		public CombatLogContext(CombatLogEntry entryType, Entity sourceEntity, Entity targetEntity, List<StatusEffectsData> statusEffects)
		{
			if (entryType != CombatLogEntry.statusEffectLost)
			{
				Debug.LogError("Invalid log type for StatusEffects");
				return;
			}

			EntryType = entryType;
			SourceEntity = sourceEntity;
			TargetEntity = targetEntity;
			LostStatusEffects = new List<StatusEffectsData>(statusEffects);
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

		public CombatLogContext(CombatLogEntry entryType)
		{
			if (entryType != CombatLogEntry.debug)
			{
				Debug.LogError("Invalid log type for DebugCombatLog");
				return;
			}

			EntryType = entryType;
			DebugLogMessage = "This is a Debug combat log";
		}
	}
}
