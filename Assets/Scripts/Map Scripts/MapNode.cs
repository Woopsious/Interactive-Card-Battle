using UnityEngine;
using static Woopsious.MapNodeData;
using static Woopsious.EntityData;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;

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

		public Image backgroundImage;

		[Header("Runtime data")]
		public MapNodeData mapNodeData;
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
		public bool DisplayEncounterEnemies;
		public bool forceEliteFight;
		public ForceLandModifier forceLandModifier;
		[System.Flags]
		public enum ForceLandModifier
		{
			none = 0, ruins = 1, town = 2, cursed = 4, volcanic = 8, caves = 16
		}
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
			startEncounterButton.onClick.AddListener(delegate { BeginEncounter(); });
		}

		private void Start()
		{
			//Initilize(mapNodeData, true, false);
		}

		public void Initilize(MapNodeData mapNodeData, bool startingNode, bool bossFightNode)
		{
			this.mapNodeData = mapNodeData;
			entityBudget = mapNodeData.entityBudget;
			nodeState = NodeState.locked;
			landTypes = mapNodeData.landTypes;
			landModifiers = LandModifiers.none;
			nodeEncounterType = NodeEncounterType.basicFight;

			ApplyRandomizedSettings(bossFightNode);
			CheckAndForceDebugSettings();
			SetEnemyTypes();

			encounterTitleText.text = UpdateEncounterTitleUi();
			encounterLandTypeText.text = UpdateEncounterLandTypeUi();
			encounterModifiersText.text = UpdateEncounterModifiersUi();
			encounterEnemiesText.text = UpdateEncounterEnemiesUi();

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
			mapNode.AddThisNodeAsPreviousLink(this);
		}
		void AddThisNodeAsPreviousLink(MapNode mapNode)
		{
			previousLinkedNodes.Add(mapNode);
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

			if (forceLandModifier.HasFlag(ForceLandModifier.ruins))
				landModifiers = AddRuinsLandModifier();
			if (forceLandModifier.HasFlag(ForceLandModifier.town))
				landModifiers = AddTownLandModifier();
			if (forceLandModifier.HasFlag(ForceLandModifier.cursed))
				landModifiers = AddCursedLandModifier();
			if (forceLandModifier.HasFlag(ForceLandModifier.volcanic))
				landModifiers = AddVolcanicLandModifier();
			if (forceLandModifier.HasFlag(ForceLandModifier.caves))
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

			foreach (EntityData entity in SpawnManager.instance.entityDataTypes)
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

		public Task BuyEnemy(EntityData spawnedEntity)
		{
			entityBudget -= spawnedEntity.GetEntityCost();

			for (int i = PossibleEntities.Count - 1;  i > 0; i--)
			{
				if (entityBudget < PossibleEntities[i].GetEntityCost())
					PossibleEntities.Remove(PossibleEntities[i]);
			}

			return Task.CompletedTask;
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
			string encounterTitle = "";

			switch (nodeEncounterType)
			{
				case NodeEncounterType.basicFight:
				encounterTitle = "Basic Fight";
				break;
				case NodeEncounterType.eliteFight:
				encounterTitle = "<color=#800080>Elite Fight</color>"; //Eldritch Purple
				break;
				case NodeEncounterType.bossFight:
				encounterTitle = "<color=#C81919>Boss Fight</color>"; //Red
				break;
				case NodeEncounterType.eliteBossFight:
				encounterTitle = "<color=#800080>Elite Boss Fight</color>"; //Eldritch Purple
				break;
				case NodeEncounterType.freeCardUpgrade:
				encounterTitle = "<color=#00FFFF>Free Card Upgrade</color>"; //Cyan
				break;
			}

			return encounterTitle;
		}
		string UpdateEncounterLandTypeUi()
		{
			string encounterLandType = "";

			if (landTypes.HasFlag(LandTypes.grassland))
				encounterLandType += "<color=green>Grasslands</color>";
			else if (landTypes.HasFlag(LandTypes.hills))
				encounterLandType += "<color=#7C9A61>Hills</color>"; //Muted Green
			else if (landTypes.HasFlag(LandTypes.forest))
				encounterLandType += "<color=#006400>Forest</color>"; //Dark Green
			else if (landTypes.HasFlag(LandTypes.mountains))
				encounterLandType += "<color=#5A5E5B>Mountains</color>"; //Dark Slate
			else if (landTypes.HasFlag(LandTypes.desert))
				encounterLandType += "<color=#DCC38C>Desert</color>"; //Golden Sand
			else if (landTypes.HasFlag(LandTypes.tundra))
				encounterLandType += "<color=#CDE2EA>Tundra</color>"; //Icy Blue

			return encounterLandType;
		}
		string UpdateEncounterModifiersUi()
		{
			string encounterModifiers = "Modifiers: \n";

			if (landModifiers == LandModifiers.none)
				return encounterModifiers += "None";

			if (landModifiers.HasFlag(LandModifiers.ruins))
				encounterModifiers += "<color=#00FFFF>Ruins, </color>"; //Cyan
			if (landModifiers.HasFlag(LandModifiers.town))
				encounterModifiers += "<color=#00FFFF>Town, </color>"; //Cyan
			if (landModifiers.HasFlag(LandModifiers.cursed))
				encounterModifiers += "<color=#00FFFF>Cursed, </color>"; //Cyan
			if (landModifiers.HasFlag(LandModifiers.volcanic))
				encounterModifiers += "<color=#00FFFF>Volcanic, </color>"; //Cyan
			if (landModifiers.HasFlag(LandModifiers.caves))
				encounterModifiers += "<color=#00FFFF>Caves, </color>"; //Cyan

			encounterModifiers = RemoveLastComma(encounterModifiers);

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

			encounterEnemies = RemoveLastComma(encounterEnemies);

			return encounterEnemies;
		}
		string DisplayEncouterEnemiesIndividualy()
		{
			string encounterEnemies = "";

			foreach (EntityData entity in SpawnManager.instance.entityDataTypes)
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

			return encounterEnemies;
		}
		string DisplayEncouterEnemyTypes()
		{
			string encounterEnemies = "";

			if (enemyTypes.HasFlag(EnemyTypes.slime))
				encounterEnemies += "<color=#90EE90>Slimes</color>" + ", "; //light green
			if (enemyTypes.HasFlag(EnemyTypes.beast))
				encounterEnemies += "<color=#8B4513>Beasts</color>" + ", "; //Earthy Brown
			if (enemyTypes.HasFlag(EnemyTypes.humanoid))
				encounterEnemies += "Humanoids" + ", "; //
			if (enemyTypes.HasFlag(EnemyTypes.construct))
				encounterEnemies += "<color=#2a3439>Constructs</color>" + ", "; //Gun Metal
			if (enemyTypes.HasFlag(EnemyTypes.undead))
				encounterEnemies += "<color=#2F4F4F>Undead</color>" + ", "; //Bloodless Gray
			if (enemyTypes.HasFlag(EnemyTypes.Abberration))
				encounterEnemies += "<color=#800080>Abberrations</color>" + ", "; //Eldritch Purple

			return encounterEnemies;
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
