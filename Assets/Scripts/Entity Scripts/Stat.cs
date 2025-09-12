using System;
using System.Collections.Generic;
using UnityEngine;

namespace Woopsious
{
	[Serializable]
	public class Stat
	{
		[SerializeField] public float baseValue;
		[SerializeField] public List<float> valueModifiers = new();
		[SerializeField] public float Value;

		[SerializeField] public StatType statType;
		public enum StatType
		{
			noType, damageRecieved, damageDealt, blockBonus, damageBonus, cardDrawAmount
		}

		public Stat(float baseValue, StatType statType)
		{
			this.baseValue = baseValue;
			this.statType = statType;
			valueModifiers.Clear();
			UpdateValue();
		}

		public void AddModifier(float newValue, StatType statType)
		{
			if (this.statType != statType) return; //ignore modifiers for other stat types

			valueModifiers.Add(newValue);
			UpdateValue();
		}

		public void RemoveModifier(float existingModifier, StatType statType)
		{
			if (this.statType != statType) return; //ignore modifiers for other stat types

			if (!valueModifiers.Contains(existingModifier))
				Debug.LogError("Tried removing modifer from stat that doesnt exist");

			valueModifiers.Remove(existingModifier);
			UpdateValue();
		}

		void UpdateValue()
		{
			Value = GetBaseAndModifiersTotal();
		}
		float GetBaseAndModifiersTotal()
		{
			float modifiers = baseValue;
			foreach (float modifier in valueModifiers)
				modifiers += modifier;

			return modifiers;
		}
	}
}