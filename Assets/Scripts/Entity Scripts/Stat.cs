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

		public Stat(float baseValue)
		{
			this.baseValue = baseValue;
			valueModifiers.Clear();
			UpdateValue();
		}

		public void AddModifier(float newValue)
		{
			valueModifiers.Add(newValue);
			UpdateValue();
		}

		public void RemoveModifier(float existingModifier)
		{
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

    public class StatModifierData
    {
		public float ModifierValue { get; private set; }
		public StatType StatModifierType { get; private set; }
		public enum StatType
		{
			noType, damageRecieved, damageDealt
		}

		public StatModifierData(float modifierValue, StatType statModifierType)
		{
			ModifierValue = modifierValue;
			StatModifierType = statModifierType;
		}
	}
}