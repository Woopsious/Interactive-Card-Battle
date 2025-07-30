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

		[Header("Node Basic Settings")]
		public LandTypes landTypes;
		[System.Flags]
		public enum LandTypes : int
		{
			none = 0, grassland = 1, forest = 2, mountains = 4, caves = 8, ruins = 16
		}

		public NodeEncounterType nodeType;
		public enum NodeEncounterType
		{
			basicFight, eliteFight, bossFight, eliteBossFight, freeCardUpgrade
		}

		public int entityBudget;
		[Range(0f, 100f)]
		public float nodeSpawnChance;

		[Header("Chance Modifiers")]
		[Tooltip("Base Value: 8%")]
		[Range(0f, 100f)]
		public float chanceOfRuins;
		[Range(0f, 100f)]
		[Tooltip("Base Value: 5%")]
		public float chanceOfEliteFight;
		[Range(0f, 100f)]
		[Tooltip("Base Value: 5%")]
		public float chanceOfFreeCardUpgrade;
	}
}
