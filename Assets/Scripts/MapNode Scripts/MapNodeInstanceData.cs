using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static Woopsious.EntityData;
using static Woopsious.MapNodeController;
using static Woopsious.MapNodeDefinition;

namespace Woopsious
{
	public class MapNodeInstanceData
	{
		private MapNodeDefinition mapNodeData;

		[Header("Data")]
		public float encounterDifficulty;
		public int cardRewardChoiceCount;
		public int cardRewardSelectionCount;
		public float cardRewardRarityBoost;
		public int entityBudget;
		public NodeState nodeState;
		public EnemyTypes enemyTypes;
		public LandTypes landTypes;
		public LandModifiers landModifiers;
		public NodeEncounterType nodeEncounterType;
		public bool IsMapEndNode { get; private set; }

		[Header("Entity Spawn Table")]
		public List<EntityData> PossibleEntities = new();
		public int totalPossibleEntitiesSpawnChance;
		public int cheapestEnemyCost;

		//encounter Difficulty modifiers (CHANGE THEM HERE)
		private readonly float eliteFightDifficultyModifier = 0.025f;
		private readonly float bossFightDifficultyModifier = 0.05f;
		private readonly float eliteBossFightDifficultyModifier = 0.1f;

		private readonly float ruinsDifficultyModifier = 0.025f;
		private readonly float townDifficultyModifier = 0.1f;
		private readonly float cursedDifficultyModifier = 0.05f;
		private readonly float volcanicDifficultyModifier = 0.025f;
		private readonly float cavesDifficultyModifier = 0.025f;

		private readonly System.Random systemRandom = new();

		public MapNodeInstanceData(MapNodeDefinition mapNodeData)
		{
			this.mapNodeData = mapNodeData;
			nodeState = NodeState.locked;
			landTypes = mapNodeData.landTypes;
			landModifiers = LandModifiers.none;
			nodeEncounterType = NodeEncounterType.basicFight;
		}

		public void DebugForceSettings(bool forceSettings, bool forceEliteFight, ForceEncounterType forceEncounterType, LandModifiers forceLandModifiers)
		{
			if (!forceSettings) return;

			if (forceEliteFight)
				nodeEncounterType = MakeEncounterElite();

			if (forceLandModifiers.HasFlag(LandModifiers.ruins))
				landModifiers = AddRuinsLandModifier();
			if (forceLandModifiers.HasFlag(LandModifiers.town))
				landModifiers = AddTownLandModifier();
			if (forceLandModifiers.HasFlag(LandModifiers.cursed))
				landModifiers = AddCursedLandModifier();
			if (forceLandModifiers.HasFlag(LandModifiers.volcanic))
				landModifiers = AddVolcanicLandModifier();
			if (forceLandModifiers.HasFlag(LandModifiers.caves))
				landModifiers = AddCavesLandModifier();

			switch (forceEncounterType)
			{
				case ForceEncounterType.noForceEncounter:
				break;
				case ForceEncounterType.forceBossFight:
				nodeEncounterType = MakeEncounterBossFight();
				break;
				case ForceEncounterType.forceCardUpgrade:
				nodeEncounterType = MakeEncounterFreeCardUpgrade();
				break;
			}
		}

		//Randomize Node Data
		public void RandomizeEncounterType(bool bossFightNode)
		{
			if (bossFightNode)
				nodeEncounterType = MakeEncounterBossFight();
			else
			{
				if (GetRandomNumber() < mapNodeData.chanceOfFreeCardUpgrade)
				{
					nodeEncounterType = MakeEncounterFreeCardUpgrade();
					return;
				}
			}

			if (GetRandomNumber() < mapNodeData.chanceOfEliteFight)
				nodeEncounterType = MakeEncounterElite();
		}
		public void RandomizedLandModifiers()
		{
			if (mapNodeData.applyableLandModifiers.HasFlag(LandModifiers.ruins) && GetRandomNumber() < mapNodeData.chanceOfRuins)
				landModifiers = AddRuinsLandModifier();
			if (mapNodeData.applyableLandModifiers.HasFlag(LandModifiers.town) && GetRandomNumber() < mapNodeData.chanceOfTown)
				landModifiers = AddTownLandModifier();
			if (mapNodeData.applyableLandModifiers.HasFlag(LandModifiers.cursed) && GetRandomNumber() < mapNodeData.chanceOfCursed)
				landModifiers = AddCursedLandModifier();
			if (mapNodeData.applyableLandModifiers.HasFlag(LandModifiers.volcanic) && GetRandomNumber() < mapNodeData.chanceOfVolcanic)
				landModifiers = AddVolcanicLandModifier();
			if (mapNodeData.applyableLandModifiers.HasFlag(LandModifiers.caves) && GetRandomNumber() < mapNodeData.chanceOfCaves)
				landModifiers = AddCavesLandModifier();
		}
		public void CheckIfMapEndNode(int columnIndex, bool bossFightNode)
		{
			if (columnIndex == 9 && bossFightNode)
				IsMapEndNode = true;
			else IsMapEndNode = false;
		}

		//sub funcs
		private NodeEncounterType MakeEncounterFreeCardUpgrade()
		{
			return NodeEncounterType.freeCardUpgrade;
		}
		private NodeEncounterType MakeEncounterBossFight()
		{
			return NodeEncounterType.bossFight;
		}
		private NodeEncounterType MakeEncounterElite()
		{
			if (nodeEncounterType == NodeEncounterType.basicFight)
				return NodeEncounterType.eliteFight;
			else if (nodeEncounterType == NodeEncounterType.bossFight)
				return NodeEncounterType.eliteBossFight;
			else
			{
				Debug.LogError("no elite version of encounter found cancelling");
				return nodeEncounterType;
			}
		}
		private LandModifiers AddRuinsLandModifier()
		{
			LandModifiers addedModifier = landModifiers | LandModifiers.ruins;
			return addedModifier;
		}
		private LandModifiers AddTownLandModifier()
		{
			LandModifiers addedModifier = landModifiers | LandModifiers.town;
			return addedModifier;
		}
		private LandModifiers AddCursedLandModifier()
		{
			LandModifiers addedModifier = landModifiers | LandModifiers.cursed;
			return addedModifier;
		}
		private LandModifiers AddVolcanicLandModifier()
		{
			LandModifiers addedModifier = landModifiers | LandModifiers.volcanic;
			return addedModifier;
		}
		private LandModifiers AddCavesLandModifier()
		{
			LandModifiers addedModifier = landModifiers | LandModifiers.caves;
			return addedModifier;
		}
		private double GetRandomNumber()
		{
			double roll = systemRandom.NextDouble();
			return roll;
		}

		public void CalculateEncounterDifficulty(int columnIndex)
		{
			float difficultyModifier = GameManager.WorldDifficulty + mapNodeData.baseEncounterDifficulty;

			int increaseDifficultyEveryXColumns = 3;
			int timesByLimit = columnIndex / increaseDifficultyEveryXColumns;

			switch (nodeEncounterType)
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

			for (int i = 0; i < timesByLimit; i++) //increase difficulty every x columns by x amount
				difficultyModifier += 0.2f;

			if (landModifiers.HasFlag(LandModifiers.ruins))
				difficultyModifier += ruinsDifficultyModifier;
			if (landModifiers.HasFlag(LandModifiers.town))
				difficultyModifier -= townDifficultyModifier;
			if (landModifiers.HasFlag(LandModifiers.cursed))
				difficultyModifier += cursedDifficultyModifier;
			if (landModifiers.HasFlag(LandModifiers.volcanic))
				difficultyModifier += volcanicDifficultyModifier;
			if (landModifiers.HasFlag(LandModifiers.caves))
				difficultyModifier += cavesDifficultyModifier;

			encounterDifficulty = difficultyModifier;
		}
		public void CalculateCardRewardValues()
		{
			cardRewardChoiceCount = 2;
			cardRewardSelectionCount = 1;
			cardRewardRarityBoost = 0f;

			switch (nodeEncounterType)
			{
				case NodeEncounterType.eliteFight:
				cardRewardChoiceCount = 3;
				cardRewardSelectionCount = 1;
				cardRewardRarityBoost = 0.05f;
				break;
				case NodeEncounterType.bossFight:
				cardRewardChoiceCount = 4;
				cardRewardSelectionCount = 2;
				cardRewardRarityBoost = 0.05f;
				break;
				case NodeEncounterType.eliteBossFight:
				cardRewardChoiceCount = 5;
				cardRewardSelectionCount = 2;
				cardRewardRarityBoost = 0.1f;
				break;
				default: break;
			}
		}

		public void CalculateEntityBudget()
		{
			entityBudget = Mathf.RoundToInt(mapNodeData.baseEntityBudget * encounterDifficulty);
		}
		public void CalculatePossibleEnemies()
		{
			cheapestEnemyCost = 100000;

			foreach (EntityData entity in GameManager.instance.enemyDataTypes)
			{
				//toggle flag if land types match
				if ((landTypes & entity.foundInLandTypes) != LandTypes.none || (landModifiers & entity.foundWithLandModifiers) != LandModifiers.none)
				{
					if (PossibleEntities.Count != 0)
						if (PossibleEntities.Contains(entity)) continue;

					PossibleEntities.Add(entity);
					totalPossibleEntitiesSpawnChance += (int)entity.entitySpawnChance;
					enemyTypes |= entity.enemyType;

					int entityCost = entity.GetEntityCost();
					if (entityCost < cheapestEnemyCost)
						cheapestEnemyCost = entityCost;
				}
			}
		}
		public Task BuyEnemyAndUpdatePossibleEntities(EntityData spawnedEntity)
		{
			entityBudget -= spawnedEntity.GetEntityCost();
			totalPossibleEntitiesSpawnChance = 0;

			for (int i = PossibleEntities.Count - 1; i >= 0; i--)
			{
				if (entityBudget < PossibleEntities[i].GetEntityCost())
					PossibleEntities.Remove(PossibleEntities[i]);
				else
					totalPossibleEntitiesSpawnChance += (int)PossibleEntities[i].entitySpawnChance;
			}

			return Task.CompletedTask;
		}
	}
}
