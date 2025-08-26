using System;
using UnityEngine;
using static Woopsious.StatModifierData;

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
		public int effectStacks;
		public int maxEffectStacks;

		[Header("Effect Lifetime")]
		public bool hasLifetime;
		public bool isDoT;
		public int effectTurnLifetime;

		[Header("Effect Damage")]
		public bool isPercentage;
		public float effectValue;
		public StatType effectStatModifierType;

		public string CreateDescription()
		{
			string description = effectDescription;
			description += "\n";

			return description;
		}
	}
}
