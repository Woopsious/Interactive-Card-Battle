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

		//set at runtime
		public enum NodeEncounterType
		{
			basicFight, eliteFight, bossFight, eliteBossFight, freeCardUpgrade
		}

		[Header("Node Settings")]
		[Range(0f, 100f)]
		public float nodeSpawnChance;
		public int entityBudget;

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
	}
}
