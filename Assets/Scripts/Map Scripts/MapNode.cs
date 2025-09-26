using UnityEngine;
using static Woopsious.MapNodeData;
using static Woopsious.EntityData;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;
using static UnityEngine.EventSystems.EventTrigger;

namespace Woopsious
{
	public class MapNode : MonoBehaviour
	{
		[Header("Ui")]
		public TMP_Text encounterTitleText;
		public TMP_Text encounterLandTypeText;
		public TMP_Text encounterModifiersText;
		public TMP_Text encounterEnemiesText;
		public Button startEncounterButton;

		Image backgroundImage;
		public TMP_Text debugDataText;

		[Header("Runtime data")]
		public MapNodeData mapNodeData;
		public float encounterDifficulty;
		public int entityBudget;
		public NodeState nodeState;
		public EnemyTypes enemyTypes;
		public LandTypes landTypes;
		public LandModifiers landModifiers;
		public NodeEncounterType nodeEncounterType;

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
		public bool DisplayEncounterEnemies;
		public bool forceEliteFight;
		public LandModifiers forceLandModifier;
		public ForceEncounterType forceEncounterType;
		public enum ForceEncounterType
		{
			noForceEncounter, forceBossFight, forceCardUpgrade
		}

		private Color _ColourRed = new(0.55f, 0.25f, 0.25f);
		private Color _ColourGreen = new(0.25f, 0.5f, 0.25f);
		private Color _ColourGold = new(1f, 0.85f, 0f);

		private readonly System.Random systemRandom = new();

		void Awake()
		{
			backgroundImage = GetComponent<Image>();
			startEncounterButton.onClick.AddListener(() => BeginEncounter());
		}

		public void Initilize(int columnIndex, MapNodeData mapNodeData, bool startingNode, bool bossFightNode)
		{
			this.mapNodeData = mapNodeData;
			nodeState = NodeState.locked;
			landTypes = mapNodeData.landTypes;
			landModifiers = LandModifiers.none;
			nodeEncounterType = NodeEncounterType.basicFight;

			ApplyRandomizedSettings(bossFightNode);
			CheckAndForceDebugSettings();
			SetEnemyTypes();
			encounterDifficulty = mapNodeData.CalculateEncounterDifficultyFromModifiers(columnIndex, this);
			entityBudget = Mathf.RoundToInt(mapNodeData.baseEntityBudget * encounterDifficulty);

			encounterTitleText.text = UpdateEncounterTitleUi();
			encounterLandTypeText.text = UpdateEncounterLandTypeUi();
			encounterModifiersText.text = UpdateEncounterModifiersUi();
			encounterEnemiesText.text = UpdateEncounterEnemiesUi();

			if (debugDataText)
			{
				debugDataText.text = DebugDataTextToUi();
				debugDataText.gameObject.SetActive(true);
			}

			if (startingNode)
				CanTravelToNode();
			else
				LockNode();
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
		void ApplyRandomizedSettings(bool bossFightNode)
		{
			if (GetRandomNumber() < mapNodeData.chanceOfFreeCardUpgrade)
			{
				MakeEncounterFreeCardUpgrade();
				return;
			}

			if (bossFightNode)
				nodeEncounterType = MakeEncounterBossFight();

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
		void CheckAndForceDebugSettings()
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
		void SetEnemyTypes()
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
		NodeEncounterType MakeEncounterFreeCardUpgrade()
		{
			return NodeEncounterType.freeCardUpgrade;
		}
		NodeEncounterType MakeEncounterBossFight()
		{
			return NodeEncounterType.bossFight;
		}
		NodeEncounterType MakeEncounterElite()
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
		LandModifiers AddRuinsLandModifier()
		{
			LandModifiers addedModifier = landModifiers | LandModifiers.ruins;
			return addedModifier;
		}
		LandModifiers AddTownLandModifier()
		{
			LandModifiers addedModifier = landModifiers | LandModifiers.town;
			return addedModifier;
		}
		LandModifiers AddCursedLandModifier()
		{
			LandModifiers addedModifier = landModifiers | LandModifiers.cursed;
			return addedModifier;
		}
		LandModifiers AddVolcanicLandModifier()
		{
			LandModifiers addedModifier = landModifiers | LandModifiers.volcanic;
			return addedModifier;
		}
		LandModifiers AddCavesLandModifier()
		{
			LandModifiers addedModifier = landModifiers | LandModifiers.caves;
			return addedModifier;
		}
		double GetRandomNumber()
		{
			double roll = systemRandom.NextDouble() * 100;
			return roll;
		}

		//start encounter
		void BeginEncounter()
		{
			GameManager.BeginCardCombat(this);
			CurrentlyAtNode();

			foreach (MapNode previousNode in previousLinkedNodes) //lock prev nodes
			{
				if (previousNode.nodeState == NodeState.currentlyAt)
					previousNode.PreviouslyVisitedNode();
				else
					previousNode.LockNode();
			}

			foreach (MapNode nextNode in nextLinkedNodes) //unlock next nodes
				nextNode.CanTravelToNode();

			foreach (MapNode node in siblingNodes) //lock sibling nodes
				node.LockNode();
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
		public void LockNode()
		{
			nodeState = NodeState.locked;
			startEncounterButton.gameObject.SetActive(false);
			backgroundImage.color = _ColourRed;
		}
		void CanTravelToNode()
		{
			nodeState = NodeState.canTravel;
			startEncounterButton.gameObject.SetActive(true);
			backgroundImage.color = _ColourGreen;
		}
		void CurrentlyAtNode()
		{
			nodeState = NodeState.currentlyAt;
			startEncounterButton.gameObject.SetActive(false);
			backgroundImage.color = _ColourGold;
		}
		void PreviouslyVisitedNode()
		{
			nodeState = NodeState.previouslyVisited;
			startEncounterButton.gameObject.SetActive(false);
			backgroundImage.color = _ColourGold;
		}

		//update ui
		string UpdateEncounterTitleUi()
		{
			return RichTextManager.GetEncounterTypeTextColour(nodeEncounterType);
		}
		string UpdateEncounterLandTypeUi()
		{
			return RichTextManager.GetLandTypesTextColour(landTypes);
		}
		string UpdateEncounterModifiersUi()
		{
			string encounterModifiers = "Modifiers: \n";
			encounterModifiers += RichTextManager.GetLandModifiersTextColour(landModifiers);
			return encounterModifiers;
		}
		string UpdateEncounterEnemiesUi()
		{
			string encounterEnemies = "Possible Enemies: \n";

			if (nodeEncounterType == NodeEncounterType.freeCardUpgrade)
			{
				encounterEnemies += "None";
				return encounterEnemies;
			}

			if (DisplayEncounterEnemies)
				encounterEnemies += DisplayEncouterEnemiesIndividualy();
			else
				encounterEnemies += DisplayEncouterEnemyTypes();

			return encounterEnemies;
		}
		string DisplayEncouterEnemiesIndividualy()
		{
			string encounterEnemies = "";

			foreach (EntityData entity in GameManager.instance.enemyDataTypes)
			{
				//check if any LandType/LandModifier flags match
				if ((landTypes & entity.foundInLandTypes) != LandTypes.none || (landModifiers & entity.foundWithLandModifiers) != LandModifiers.none)
				{
					switch (entity.enemyType)
					{
						case EnemyTypes.slime:
						encounterEnemies += "<color=#90EE90>" + entity.name + "</color>" + ", "; //light green
						break;
						case EnemyTypes.beast:
						encounterEnemies += "<color=#8B4513>" + entity.name + "</color>" + ", "; //Earthy Brown
						break;
						case EnemyTypes.humanoid:
						encounterEnemies += "" + entity.name + "" + ", "; //
						break;
						case EnemyTypes.construct:
						encounterEnemies += "<color=#2a3439>" + entity.name + "</color>" + ", "; //Gun Metal
						break;
						case EnemyTypes.undead:
						encounterEnemies += "<color=#2F4F4F>" + entity.name + "</color>" + ", "; //Bloodless Gray
						break;
						case EnemyTypes.Abberration:
						encounterEnemies += "<color=#800080>" + entity.name + "</color>" + ", "; //Eldritch Purple
						break;
					}
				}
			}
			encounterEnemies = RichTextManager.RemoveLastComma(encounterEnemies);
			return encounterEnemies;
		}
		string DisplayEncouterEnemyTypes()
		{
			return RichTextManager.GetEnemyTypesTextColour(enemyTypes);
		}
		string DebugDataTextToUi()
		{
			string debugData = "Encounter Difficulty: " + encounterDifficulty + "\nEncounterBudget: " + entityBudget;
			return debugData;
		}
	}
}
