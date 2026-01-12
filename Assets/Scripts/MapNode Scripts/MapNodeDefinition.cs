using UnityEngine;

namespace Woopsious
{
	[CreateAssetMenu(fileName = "MapNodeData", menuName = "ScriptableObjects/MapNode")]
	public class MapNodeDefinition : ScriptableObject
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
			none = 0, grassland = 1, hills = 2, forest = 4, mountains = 8, desert = 16, tundra = 32, freeCard = 64
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
		[Range(0f, 1f)]
		public float nodeSpawnChance;
		[Range(0f, 1f)]
		public float baseEncounterDifficulty;
		public int baseEntityBudget;

		[Header("Chance Modifiers")]
		[Tooltip("Base Value: 15%")]
		[Range(0f, 1f)]
		public float chanceOfRuins;
		[Tooltip("Base Value: 15%")]
		[Range(0f, 1f)]
		public float chanceOfTown;
		[Tooltip("Base Value: 15%")]
		[Range(0f, 1f)]
		public float chanceOfCursed;
		[Tooltip("Base Value: 15%")]
		[Range(0f, 1f)]
		public float chanceOfVolcanic;
		[Tooltip("Base Value: 15%")]
		[Range(0f, 1f)]
		public float chanceOfCaves;
		[Range(0f, 1f)]
		[Tooltip("Base Value: 10%")]
		public float chanceOfEliteFight;
		[Range(0f, 1f)]
		[Tooltip("Base Value: 10%")]
		public float chanceOfFreeCardUpgrade;

		public string CreateMapNodeLandTypeInfo()
		{
			string landTypeInfo = RichTextManager.GetLandTypesTextColour(landTypes);

			landTypeInfo += $"\n\nChance of spawning: {nodeSpawnChance * 100}%";
			landTypeInfo += $"\nChance of elite fight: {chanceOfEliteFight * 100}%";
			landTypeInfo += $"\nChance of free card upgrade: {chanceOfFreeCardUpgrade * 100}%\n";

			if (applyableLandModifiers.HasFlag(LandModifiers.ruins))
				landTypeInfo += $"\nChance of Ruins: {chanceOfRuins * 100}%";
			if (applyableLandModifiers.HasFlag(LandModifiers.town))
				landTypeInfo += $"\nChance of Town: {chanceOfTown * 100}%";
			if (applyableLandModifiers.HasFlag(LandModifiers.cursed))
				landTypeInfo += $"\nChance of Cursed: {chanceOfCursed * 100}%";
			if (applyableLandModifiers.HasFlag(LandModifiers.volcanic))
				landTypeInfo += $"\nChance of Volcanic: {chanceOfVolcanic * 100}%";
			if (applyableLandModifiers.HasFlag(LandModifiers.caves))
				landTypeInfo += $"\nChance of Caves: {chanceOfCaves * 100}%";

			return landTypeInfo;
		}
	}
}
