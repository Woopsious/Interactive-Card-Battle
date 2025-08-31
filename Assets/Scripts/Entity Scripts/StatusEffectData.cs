using System;
using UnityEngine;
using static Woopsious.Stat;

namespace Woopsious
{
	[CreateAssetMenu(fileName = "StatusEffectData", menuName = "ScriptableObjects/StatusEffectsData")]
	[Serializable]
	public class StatusEffectsData : ScriptableObject
	{
		[Header("Effect Info")]
		public string effectName;
		public string effectDescription;

		[Header("Effect Stacks")]
		public bool hasStacks;
		public int effectStacks;
		public int maxEffectStacks;

		[Header("Effect Lifetime")]
		public bool hasLifetime;
		public int effectTurnLifetime;

		[Header("Effect Damage")]
		public bool isDoT;
		public bool isPercentage;
		public float effectValue;
		public StatType effectStatModifierType;

		public string CreateInGameDescription()
		{
			string description = effectDescription;
			return EffectDescription(description);
		}
		public string CreateTooltipDescription()
		{
			string description = effectName + " Effect";
			return EffectDescription(description);
		}

		string EffectDescription(string description)
		{
			if (hasStacks)
			{
				if (effectValue > 0)
					description += $"\n+{effectStacks} stacks of {effectName} (max:{maxEffectStacks})";
				else
					description += $"\n{effectStacks} stacks of {effectName} (max:-{maxEffectStacks})";
			}

			if (effectStatModifierType != StatType.noType)
			{
				if (effectValue > 0)
				{
					if (effectStatModifierType == StatType.damageDealt)
						description += $"\nIncreases damage dealt by {effectValue * 100}%";

					else if (effectStatModifierType == StatType.damageRecieved)
						description += $"\nIncreases damage recieved by {effectValue * 100}%";

					else if (effectStatModifierType == StatType.damageBonus)
						description += $"\n+{effectValue} damage per stack (max:{effectValue * maxEffectStacks})";

					else if (effectStatModifierType == StatType.blockBonus)
						description += $"\n+{effectValue} block per stack (max:{effectValue * maxEffectStacks})";
				}
				else
				{
					if (effectStatModifierType == StatType.damageDealt)
						description += $"\nDecreases damage dealt by {effectValue * 100}%";

					else if (effectStatModifierType == StatType.damageRecieved)
						description += $"\nDecreases damage recieved by {effectValue * 100}%";

					else if (effectStatModifierType == StatType.damageBonus)
						description += $"\n{effectValue} damage per stack (max:{effectValue * maxEffectStacks})";

					else if (effectStatModifierType == StatType.blockBonus)
						description += $"\n{effectValue} block per stack (max:{effectValue * maxEffectStacks})";
				}
			}

			if (isDoT)
				description += $"\nDeals {effectValue} damage every turn";

			if (hasLifetime)
				description += $"\nLasts {effectTurnLifetime} turns";

			return description;
		}
	}
}
