using UnityEngine;
using static Woopsious.MapNodeData;
using static Woopsious.EntityData;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Woopsious
{
	public class MapNode : MonoBehaviour
	{
		[Header("Ui")]
		public TMP_Text encounterTitleText;
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
		public NodeEncounterType nodeEncounterType;

		[Header("Entity Spawn Table")]
		public List<EntityData> PossibleEntities = new();
		public int totalPossibleEntitiesSpawnChance;
		public int cheapistEnemyCost;

		[Header("Linked Nodes")]
		public List<MapNode> previousLinkedNodes = new();
		public List<MapNode> nextLinkedNodes = new();

		[Header("Debug options")]
		public bool DisplayEncounterEnemies;
		public ForceEncounterType forceEncounterType;
		public enum ForceEncounterType
		{
			noForceEncounter, forceBossFight, forceCardUpgrade
		}
		public bool forceEliteFight;
		public bool forceRuins;

		Color gold = new Color(1f, 0.85f, 0f);
		Color lightGray = new Color(0.6862745f, 0.6862745f, 0.6862745f);

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
			nodeEncounterType = mapNodeData.nodeType;

			ApplyRandomizedSettings(bossFightNode);
			CheckAndForceDebugSettings();
			SetEnemyTypes();

			UpdateEncounterTitleUi();
			UpdateEncounterModifiersUi();
			UpdateEncounterEnemiesUi();

			if (startingNode)
				CanTravelToNode();
			else
				LockNode();
		}
		public void LinkNodes(List<MapNode> previousLinkedNodes, List<MapNode> nextLinkedNodes)
		{
			this.previousLinkedNodes = previousLinkedNodes;
			this.nextLinkedNodes = nextLinkedNodes;
		}

		//start encounter
		void BeginEncounter()
		{
			GameManager.BeginCardCombat(this);

			CurrentlyAtNode();

			foreach (MapNode previousNode in previousLinkedNodes)
				previousNode.LockNode();
			foreach (MapNode nextNode in nextLinkedNodes)
				nextNode.CanTravelToNode();
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

			if (GetRandomNumber() < mapNodeData.chanceOfRuins)
				landTypes = AddRuinsLandType();

			if (GetRandomNumber() < mapNodeData.chanceOfEliteFight)
				nodeEncounterType = MakeEncounterElite();
		}
		void CheckAndForceDebugSettings()
		{
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

			if (forceEliteFight)
				nodeEncounterType = MakeEncounterElite();

			if (forceRuins)
				landTypes = AddRuinsLandType();
		}
		void SetEnemyTypes()
		{
			cheapistEnemyCost = 100000;

			foreach (EntityData entity in SpawnManager.instance.entityDataTypes)
			{
				if ((landTypes & entity.foundInLandTypes) != LandTypes.none) //toggle flag if land types match
				{
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
		LandTypes AddRuinsLandType()
		{
			LandTypes updateLandType = mapNodeData.landTypes | LandTypes.ruins;
			return updateLandType;
		}
		float GetRandomNumber()
		{
			return Random.Range(0f, 100f);
		}

		//update node state
		public void LockNode()
		{
			nodeState = NodeState.locked;
			startEncounterButton.gameObject.SetActive(false);
			backgroundImage.color = new Color(0.4f, 0f, 0f);
		}
		public void CanTravelToNode()
		{
			nodeState = NodeState.canTravel;
			startEncounterButton.gameObject.SetActive(true);
			backgroundImage.color = lightGray;
		}
		public void CurrentlyAtNode()
		{
			nodeState = NodeState.currentlyAt;
			startEncounterButton.gameObject.SetActive(false);
			backgroundImage.color = gold;
		}

		//update ui
		void UpdateEncounterTitleUi()
		{
			string encounterTitle = "";

			switch (nodeEncounterType)
			{
				case NodeEncounterType.basicFight:
				encounterTitle = "Basic Fight";
				break;
				case NodeEncounterType.eliteFight:
				encounterTitle = "<color=purple>Elite Fight</color>";
				break;
				case NodeEncounterType.bossFight:
				encounterTitle = "<color=red>Boss Fight</color>";
				break;
				case NodeEncounterType.eliteBossFight:
				encounterTitle = "<color=purple>Elite Boss Fight</color>";
				break;
				case NodeEncounterType.freeCardUpgrade:
				encounterTitle = "<color=#00FFFF>Free Card Upgrade</color>"; //Cyan
				break;
			}

			encounterTitleText.text = encounterTitle;
		}
		void UpdateEncounterModifiersUi()
		{
			string encounterModifiers = "Enviroment: \n";

			if (landTypes.HasFlag(LandTypes.grassland))
				encounterModifiers += "<color=green>Grasslands</color>";
			else if (landTypes.HasFlag(LandTypes.forest))
				encounterModifiers += "<color=#006400>Forest</color>"; //Dark green
			else if (landTypes.HasFlag(LandTypes.mountains))
				encounterModifiers += "<color=grey>Mountains</color>";
			else if (landTypes.HasFlag(LandTypes.caves))
				encounterModifiers += "<color=black>Caves</color>";

			if (landTypes.HasFlag(LandTypes.ruins))
				encounterModifiers += " with <color=#00FFFF>Ruins</color>"; //Cyan

			encounterModifiersText.text = encounterModifiers;
		}
		void UpdateEncounterEnemiesUi()
		{
			string encounterEnemies = "Possible Enemies: \n";

			if (nodeEncounterType == NodeEncounterType.freeCardUpgrade)
			{
				encounterEnemies += "None";
				encounterEnemiesText.text = encounterEnemies;
				return;
			}

			if (DisplayEncounterEnemies)
				encounterEnemies += DisplayEncouterEnemiesIndividualy();
			else
				encounterEnemies += DisplayEncouterEnemeyTypes();

			encounterEnemiesText.text = encounterEnemies;
		}
		string DisplayEncouterEnemiesIndividualy()
		{
			string encounterEnemies = "";

			foreach (EntityData entity in SpawnManager.instance.entityDataTypes)
			{
				if ((landTypes & entity.foundInLandTypes) != LandTypes.none) //check if any land type flags match
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
						case EnemyTypes.Abberrations:
						encounterEnemies += "<color=#800080>" + entity.name + "</color>" + ", "; //Eldritch Purple
						break;
					}
				}
			}

			return encounterEnemies;
		}
		string DisplayEncouterEnemeyTypes()
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
			if (enemyTypes.HasFlag(EnemyTypes.Abberrations))
				encounterEnemies += "<color=#800080>Abberrations</color>" + ", "; //Eldritch Purple

			return encounterEnemies;
		}
	}
}
