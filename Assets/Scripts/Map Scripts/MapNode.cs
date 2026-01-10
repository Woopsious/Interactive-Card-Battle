using UnityEngine;
using static Woopsious.MapNodeData;
using static Woopsious.EntityData;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Woopsious
{
	public class MapNode : MonoBehaviour
	{
		[Header("Runtime data")]
		public MapNodeData mapNodeData;
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
		public int cheapistEnemyCost;

		[Header("Linked Nodes")]
		public List<MapNode> previousLinkedNodes = new();
		public List<MapNode> nextLinkedNodes = new();
		public List<MapNode> siblingNodes = new();

		[Header("Debug options")]
		public bool debugRuntimeData;
		public bool forceEliteFight;
		public LandModifiers forceLandModifier;
		public ForceEncounterType forceEncounterType;
		public enum ForceEncounterType
		{
			noForceEncounter, forceBossFight, forceCardUpgrade
		}

		private readonly System.Random systemRandom = new();
		public event Action InitilizeUi;
		public event Action<NodeState> NodeStateChange;

		public void Initilize(int columnIndex, MapNodeData mapNodeData, bool startingNode, bool bossFightNode)
		{
			this.mapNodeData = mapNodeData;
			nodeState = NodeState.locked;
			landTypes = mapNodeData.landTypes;
			landModifiers = LandModifiers.none;
			nodeEncounterType = NodeEncounterType.basicFight;

			ApplyRandomizedSettings(bossFightNode);
			CheckIfMapEndNode(columnIndex, bossFightNode);
			CheckAndForceDebugSettings();
			SetEnemyTypes();

			encounterDifficulty = mapNodeData.CalculateEncounterDifficultyFromModifiers(GameManager.WorldDifficulty, columnIndex, this);
			cardRewardChoiceCount = mapNodeData.CalculateCardRewardChoiceCount(this);
			cardRewardSelectionCount = mapNodeData.CalculateCardRewardsSelectionCount(this);
			cardRewardRarityBoost = mapNodeData.CalculateCardRewardRarityBoost(this);
			entityBudget = Mathf.RoundToInt(mapNodeData.baseEntityBudget * encounterDifficulty);

			if (startingNode)
				UpdateNodeState(NodeState.canTravel);
			else
				UpdateNodeState(NodeState.locked);

			InitilizeUi?.Invoke();
		}

		//node linking
		public void AddSiblingNodes(Dictionary<int, MapNode> siblingNodes)
		{
			for (int i = 0; i < siblingNodes.Count; i++)
			{
				if (siblingNodes[i] == this) continue;
				this.siblingNodes.Add(siblingNodes[i]);
			}
		}
		public void AddLinkToNextNode(MapNode mapNode)
		{
			nextLinkedNodes.Add(mapNode);
			mapNode.previousLinkedNodes.Add(this);
		}
		public void AddLinkToPreviousNode(MapNode mapNode)
		{
			previousLinkedNodes.Add(mapNode);
			mapNode.nextLinkedNodes.Add(this);
		}

		//UPDATE NODE SETTINGS AT RUNTIME
		private void ApplyRandomizedSettings(bool bossFightNode)
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

			if (mapNodeData.applyableLandModifiers.HasFlag(LandModifiers.ruins) && GetRandomNumber() < mapNodeData.chanceOfRuins)
				landModifiers = AddRuinsLandModifier();
			if (mapNodeData.applyableLandModifiers.HasFlag(LandModifiers.town) && GetRandomNumber() < mapNodeData.chanceOfRuins)
				landModifiers = AddTownLandModifier();
			if (mapNodeData.applyableLandModifiers.HasFlag(LandModifiers.cursed) && GetRandomNumber() < mapNodeData.chanceOfRuins)
				landModifiers = AddCursedLandModifier();
			if (mapNodeData.applyableLandModifiers.HasFlag(LandModifiers.volcanic) && GetRandomNumber() < mapNodeData.chanceOfRuins)
				landModifiers = AddVolcanicLandModifier();
			if (mapNodeData.applyableLandModifiers.HasFlag(LandModifiers.caves) && GetRandomNumber() < mapNodeData.chanceOfRuins)
				landModifiers = AddCavesLandModifier();

			if (GetRandomNumber() < mapNodeData.chanceOfEliteFight)
				nodeEncounterType = MakeEncounterElite();
		}
		private void CheckIfMapEndNode(int columnIndex, bool bossFightNode)
		{
			if (columnIndex == 9 && bossFightNode)
				IsMapEndNode = true;
			else IsMapEndNode = false;
		}
		private void CheckAndForceDebugSettings()
		{
			if (forceEliteFight)
				nodeEncounterType = MakeEncounterElite();

			if (forceLandModifier.HasFlag(LandModifiers.ruins))
				landModifiers = AddRuinsLandModifier();
			if (forceLandModifier.HasFlag(LandModifiers.town))
				landModifiers = AddTownLandModifier();
			if (forceLandModifier.HasFlag(LandModifiers.cursed))
				landModifiers = AddCursedLandModifier();
			if (forceLandModifier.HasFlag(LandModifiers.volcanic))
				landModifiers = AddVolcanicLandModifier();
			if (forceLandModifier.HasFlag(LandModifiers.caves))
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
		private void SetEnemyTypes()
		{
			cheapistEnemyCost = 100000;

			foreach (EntityData entity in GameManager.instance.enemyDataTypes)
			{
				//toggle flag if land types match
				if ((landTypes & entity.foundInLandTypes) != LandTypes.none  || (landModifiers & entity.foundWithLandModifiers) != LandModifiers.none)
				{
					if (PossibleEntities.Count != 0)
						if (PossibleEntities.Contains(entity)) continue;

					PossibleEntities.Add(entity);
					totalPossibleEntitiesSpawnChance += (int)entity.entitySpawnChance;
					enemyTypes |= entity.enemyType;

					int entityCost = entity.GetEntityCost();
					if (entityCost < cheapistEnemyCost)
						cheapistEnemyCost = entityCost;
				}
			}
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

		//start encounter
		public void BeginEncounter()
		{
			UpdateNodeState(NodeState.currentlyAt);

			foreach (MapNode previousNode in previousLinkedNodes) //lock prev nodes
			{
				if (previousNode.nodeState == NodeState.currentlyAt)
					previousNode.UpdateNodeState(NodeState.previouslyVisited);
				else
					previousNode.UpdateNodeState(NodeState.locked);
			}

			foreach (MapNode nextNode in nextLinkedNodes) //unlock next nodes
				nextNode.UpdateNodeState(NodeState.canTravel);

			foreach (MapNode siblingNode in siblingNodes) //lock sibling nodes
				siblingNode.UpdateNodeState(NodeState.locked);

			if (nodeEncounterType == NodeEncounterType.freeCardUpgrade)
				GameManager.EnterCardCombatWin();
			else
				GameManager.EnterCardCombat(this);
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

		//update node state
		public void UpdateNodeState(NodeState nodeState)
		{
			this.nodeState = nodeState;
			NodeStateChange?.Invoke(nodeState);
		}
	}
}
