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

		//encounter Difficulty modifiers (CHANGE THEM HERE)
		private readonly float eliteFightDifficultyModifier = 0.025f;
		private readonly float bossFightDifficultyModifier = 0.05f;
		private readonly float eliteBossFightDifficultyModifier = 0.1f;

		private readonly float ruinsDifficultyModifier = 0.025f;
		private readonly float townDifficultyModifier = 0.1f;
		private readonly float cursedDifficultyModifier = 0.05f;
		private readonly float volcanicDifficultyModifier = 0.025f;
		private readonly float cavesDifficultyModifier = 0.025f;


		public float CalculateEncounterDifficultyFromModifiers(int columnIndex, MapNode mapNode)
		{
			float difficultyModifier = baseEncounterDifficulty;

			int increaseDifficultyEveryXColumns = 3;
			int timesByLimit = columnIndex / increaseDifficultyEveryXColumns;

			for (int i = 0; i < timesByLimit; i++) //increase difficulty every x columns by x amount
				difficultyModifier += 0.2f;

			if (mapNode.landModifiers.HasFlag(LandModifiers.ruins))
				difficultyModifier += ruinsDifficultyModifier;
			if (mapNode.landModifiers.HasFlag(LandModifiers.town))
				difficultyModifier -= townDifficultyModifier;
			if (mapNode.landModifiers.HasFlag(LandModifiers.cursed))
				difficultyModifier += cursedDifficultyModifier;
			if (mapNode.landModifiers.HasFlag(LandModifiers.volcanic))
				difficultyModifier += volcanicDifficultyModifier;
			if (mapNode.landModifiers.HasFlag(LandModifiers.caves))
				difficultyModifier += cavesDifficultyModifier;

			switch (mapNode.nodeEncounterType)
			{
				case NodeEncounterType.eliteFight:
				difficultyModifier += eliteFightDifficultyModifier;
				break;
				case NodeEncounterType.bossFight:
				difficultyModifier += bossFightDifficultyModifier;
				break;
				case NodeEncounterType.eliteBossFight:
				difficultyModifier += eliteBossFightDifficultyModifier;
				break;
				default: break;
			}

			return difficultyModifier;
		}
		public int CalculateCardRewardChoiceCount(MapNode mapNode)
		{
			int cardRewards = 2;

			switch (mapNode.nodeEncounterType)
			{
				case NodeEncounterType.eliteFight:
				cardRewards = 3;
				break;
				case NodeEncounterType.bossFight:
				cardRewards = 4;
				break;
				case NodeEncounterType.eliteBossFight:
				cardRewards = 5;
				break;
				default: break;
			}

			return cardRewards;
		}
		public int CalculateCardRewardsSelectionCount(MapNode mapNode)
		{
			int cardRewards = 1;

			switch (mapNode.nodeEncounterType)
			{
				case NodeEncounterType.eliteFight:
				cardRewards = 1;
				break;
				case NodeEncounterType.bossFight:
				cardRewards = 2;
				break;
				case NodeEncounterType.eliteBossFight:
				cardRewards = 2;
				break;
				default: break;
			}

			return cardRewards;
		}
		public float CalculateCardRewardRarityBoost(MapNode mapNode)
		{
			float cardRarityBoost = 0f;

			switch (mapNode.nodeEncounterType)
			{
				case NodeEncounterType.eliteFight:
				cardRarityBoost = 0.05f;
				break;
				case NodeEncounterType.bossFight:
				cardRarityBoost = 0.05f;
				break;
				case NodeEncounterType.eliteBossFight:
				cardRarityBoost = 0.01f;
				break;
				default: break;
			}

			return cardRarityBoost;
		}

		public string CreateMapNodeLandTypeInfo()
		{
			string landTypeInfo = RichTextManager.GetLandTypesTextColour(landTypes);

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
	}
}
