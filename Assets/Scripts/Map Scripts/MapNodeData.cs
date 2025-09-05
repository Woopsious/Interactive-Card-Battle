using Unity.Android.Gradle;
using Unity.VisualScripting;
using UnityEngine;

namespace Woopsious
{
	[CreateAssetMenu(fileName = "MapNodeData", menuName = "ScriptableObjects/MapNode")]
	public class MapNodeData : ScriptableObject
	{
		public string nodeName;
		public enum NodeState
		{
			locked, canTravel, currentlyAt, previouslyVisited
		}

		[Header("Land Type")]
		public LandTypes landTypes;
		[System.Flags]
		public enum LandTypes : int
		{
			none = 0, grassland = 1, hills = 2, forest = 4, mountains = 8, desert = 16, tundra = 32
		}

		[Header("Modifiers")]
		public int maxLandModifiers;
		public LandModifiers applyableLandModifiers;
		[System.Flags]
		public enum LandModifiers : int
		{
			none = 0, ruins = 1, town = 2, cursed = 4, volcanic = 8, caves = 16
		}
		public enum NodeEncounterType
		{
			basicFight, eliteFight, bossFight, eliteBossFight, freeCardUpgrade
		}

		[Header("Node Settings")]
		[Range(0f, 100f)]
		public float nodeSpawnChance;
		[Range(0.5f, 1.5f)]
		public float baseEncounterDifficulty;
		public int baseEntityBudget;

		[Header("Chance Modifiers")]
		[Tooltip("Base Value: 15%")]
		[Range(0f, 100f)]
		public float chanceOfRuins;
		[Tooltip("Base Value: 15%")]
		[Range(0f, 100f)]
		public float chanceOfTown;
		[Tooltip("Base Value: 15%")]
		[Range(0f, 100f)]
		public float chanceOfCursed;
		[Tooltip("Base Value: 15%")]
		[Range(0f, 100f)]
		public float chanceOfVolcanic;
		[Tooltip("Base Value: 15%")]
		[Range(0f, 100f)]
		public float chanceOfCaves;
		[Range(0f, 100f)]
		[Tooltip("Base Value: 10%")]
		public float chanceOfEliteFight;
		[Range(0f, 100f)]
		[Tooltip("Base Value: 10%")]
		public float chanceOfFreeCardUpgrade;

		public float CalculateEncounterDifficultyFromModifiers(int columnIndex, MapNode mapNode)
		{
			float difficultyModifier = baseEncounterDifficulty;

			int increaseDifficultyEveryXColumns = 3;
			int timesByLimit = columnIndex / increaseDifficultyEveryXColumns;

			for (int i = 0; i < timesByLimit; i++) //increase difficulty every x columns by x amount
				difficultyModifier += 0.2f;

			if (mapNode.landModifiers.HasFlag(LandModifiers.ruins))
				difficultyModifier += 0.025f;
			if (mapNode.landModifiers.HasFlag(LandModifiers.town))
				difficultyModifier -= 0.1f;
			if (mapNode.landModifiers.HasFlag(LandModifiers.cursed))
				difficultyModifier += 0.05f;
			if (mapNode.landModifiers.HasFlag(LandModifiers.volcanic))
				difficultyModifier += 0.025f;
			if (mapNode.landModifiers.HasFlag(LandModifiers.caves))
				difficultyModifier += 0.025f;

			switch (mapNode.nodeEncounterType)
			{
				case NodeEncounterType.eliteFight:
				difficultyModifier += 0.025f;
				break;
				case NodeEncounterType.bossFight:
				difficultyModifier += 0.05f;
				break;
				case NodeEncounterType.eliteBossFight:
				difficultyModifier += 0.1f;
				break;
				default:
				difficultyModifier += 0f;
				break;
			}

			return difficultyModifier;
		}
		public string CreateMapNodeLandTypeInfo()
		{
			string landTypeInfo = "";
			switch (landTypes)
			{
				case LandTypes.grassland:
				landTypeInfo += "Grasslands";
				break;

				case LandTypes.hills:
				landTypeInfo += "Hills";
				break;

				case LandTypes.forest:
				landTypeInfo += "Forest";
				break;

				case LandTypes.mountains:
				landTypeInfo += "Mountains";
				break;

				case LandTypes.desert:
				landTypeInfo += "Desert";
				break;

				case LandTypes.tundra:
				landTypeInfo += "Tundra";
				break;
			}

			landTypeInfo += $"\n\nChance of spawning: {nodeSpawnChance}%";
			landTypeInfo += $"\nChance of elite fight: {chanceOfEliteFight}%";
			landTypeInfo += $"\nChance of free card upgrade: {chanceOfFreeCardUpgrade}%\n";

			if (applyableLandModifiers.HasFlag(LandModifiers.ruins))
				landTypeInfo += $"\nChance of Ruins: {chanceOfRuins}%";
			if (applyableLandModifiers.HasFlag(LandModifiers.town))
				landTypeInfo += $"\nChance of Town: {chanceOfTown}%";
			if (applyableLandModifiers.HasFlag(LandModifiers.cursed))
				landTypeInfo += $"\nChance of Cursed: {chanceOfCursed}%";
			if (applyableLandModifiers.HasFlag(LandModifiers.volcanic))
				landTypeInfo += $"\nChance of Volcanic: {chanceOfVolcanic}%";
			if (applyableLandModifiers.HasFlag(LandModifiers.caves))
				landTypeInfo += $"\nChance of Caves: {chanceOfCaves}%";

			return landTypeInfo;
		}

		string RemoveLastComma(string input)
		{
			int lastCommaIndex = input.LastIndexOf(',');

			if (lastCommaIndex >= 0)
				return input.Remove(lastCommaIndex, 1);

			return input;
		}
	}
}
