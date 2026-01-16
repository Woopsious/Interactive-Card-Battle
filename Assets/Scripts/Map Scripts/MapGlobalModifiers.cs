using System.Collections.Generic;
using UnityEngine;
using Woopsious.ComplexStats;

namespace Woopsious
{
	[System.Serializable]
	public class MapGlobalModifiers
	{
		private static readonly System.Random systemRandom = new();

		[Header("World Modifiers")]
		public List<WorldModifer> worldModifers = new();

		public void RandomizeWorldModifiers()
		{
			int number = NumberOfWorldModifiersToActivate();
			List<int> chosenModifierIndexes = ChooseWorldModifiersToActivate(number);

			Debug.LogError("chosen modifiers count:" + chosenModifierIndexes.Count);

			foreach (WorldModifer worldModifer in worldModifers) //reset all modifers
				worldModifer.UpdateWorldModifier(false, 0);

			for (int i = 0; i < chosenModifierIndexes.Count; i++) //randomize modifers for chosen ones
			{
				WorldModifer chosenModifier = worldModifers[chosenModifierIndexes[i]];
				float modifierBaseValue = (float)systemRandom.Next(-5, 6) / 20; //get values and convert them to -0.25-0.25
				Debug.LogError(modifierBaseValue);
				chosenModifier.UpdateWorldModifier(true, modifierBaseValue);
			}
		}
		private int NumberOfWorldModifiersToActivate()
		{
			//1 modifiers = 20% chance etc...
			List<float> worldModifierChances = new List<float>
			{
				0.9f, 0.9f, 0.9f
			};

			float total = 0;
			foreach (float chance in worldModifierChances)
				total += chance;

			float roll = (float)systemRandom.NextDouble() * total;
			float cumulativeChance = 0;

			for (int modifiersToCreate = 0; modifiersToCreate < worldModifierChances.Count; modifiersToCreate++)
			{
				cumulativeChance += worldModifierChances[modifiersToCreate];

				if (roll <= cumulativeChance)
					return modifiersToCreate;
			}

			return 0;
		}
		private List<int> ChooseWorldModifiersToActivate(int modifiersToCreate)
		{
			List<int> chosenModifierIndexes = new();

			if (modifiersToCreate == 0) return chosenModifierIndexes;

			while (chosenModifierIndexes.Count < modifiersToCreate)
			{
				int roll = systemRandom.Next(0, worldModifers.Count);

				if (chosenModifierIndexes.Contains(roll)) continue;
				else
				{
					chosenModifierIndexes.Add(roll);
				}
			}

			return chosenModifierIndexes;
		}

		public StatModifier GetModifierStat(StatType statToGet)
		{
			foreach (WorldModifer worldModifer in worldModifers)
			{
				if (worldModifer.statToEffect == statToGet)
					return new(MapController.Instance, worldModifer.isPercentage, worldModifer.modiferValue);
			}

			Debug.LogError("Failed to get value based on stat type");
			return new(MapController.Instance, true, 0);
		}


		[System.Serializable]
		public class WorldModifer
		{
			public bool ModifierActive { get; private set; }
			public StatType statToEffect;
			public bool isPercentage;
			public float minModiferValue;
			public float maxModiferValue;
			public float modiferValue;

			public void UpdateWorldModifier(bool active, float modiferValue)
			{
				ModifierActive = active;
				if (active)
					this.modiferValue = modiferValue;
				else
					this.modiferValue = 0;
			}
		}
	}
}
