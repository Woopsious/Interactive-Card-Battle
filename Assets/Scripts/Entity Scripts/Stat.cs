using System;
using System.Collections.Generic;
using UnityEngine;

namespace Woopsious
{
	[Serializable]
	public class Stat
	{
		[SerializeField] private float baseValue;
		[SerializeField] private List<float> valueModifiers = new();
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
			float finalValue = baseValue;

			foreach (var modifier in valueModifiers)
				finalValue += modifier;

			Value = finalValue;
		}
	}
}