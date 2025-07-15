using UnityEngine;
using static Woopsious.MapNodeData;
using static Woopsious.EntityData;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

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

		[Header("Debug options")]
		public ForceEncounterType forceEncounterType;
		public enum ForceEncounterType
		{
			noForceEncounter, forceCardUpgrade, forceBossFight
		}
		public bool debugForceEliteFight;
		public bool debugForceRuins;

		void Start()
		{
			startEncounterButton.onClick.AddListener(delegate { BeginEncounter(); });

			if (mapNodeData != null)
				Initilize(mapNodeData, true);
			else
				Debug.LogWarning("Map node data not set, ignore if intended");
		}

		public void Initilize(MapNodeData mapNodeData, bool startingNode)
		{
			this.mapNodeData = mapNodeData;
			entityBudget = mapNodeData.entityBudget;
			nodeState = NodeState.locked;
			landTypes = mapNodeData.landTypes;
			nodeEncounterType = mapNodeData.nodeType;

			ApplyRandomizedSettings();
			CheckAndForceDebugSettings();

			UpdateEncounterTitle();
			UpdateEncounterModifiers();
			UpdateEncounterEnemies();

			if (startingNode)
				CanTravelToNode();
			else
				LockNode();
		}

		//start encounter
		void BeginEncounter()
		{
			GameManager.BeginCardCombat();
		}

		//UPDATE NODE SETTINGS AT RUNTIME
		void CheckAndForceDebugSettings()
		{
			switch (forceEncounterType)
			{
				case ForceEncounterType.noForceEncounter:
				break;
				case ForceEncounterType.forceCardUpgrade:
				nodeEncounterType = NodeEncounterType.freeCardUpgrade;
				break;
				case ForceEncounterType.forceBossFight:
				nodeEncounterType = NodeEncounterType.bossFight;
				break;
			}

			if (debugForceEliteFight)
				nodeEncounterType = MakeEncounterElite();

			if (debugForceRuins)
				landTypes = AddRuinsLandType();
		}
		void ApplyRandomizedSettings()
		{
			if (GetRandomNumber() < mapNodeData.chanceOfFreeCardUpgrade)
			{
				SetNodeInteractTypeToFreeCardUpgrade();
				return;
			}

			if (GetRandomNumber() < mapNodeData.chanceOfRuins)
				landTypes = AddRuinsLandType();

			if (GetRandomNumber() < mapNodeData.chanceOfEliteFight)
				nodeEncounterType = MakeEncounterElite();
		}

		//sub funcs
		void SetNodeInteractTypeToFreeCardUpgrade()
		{
			enemyTypes = EnemyTypes.none;
			nodeEncounterType = NodeEncounterType.freeCardUpgrade;
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
		}
		public void CanTravelToNode()
		{
			nodeState = NodeState.canTravel;
			startEncounterButton.gameObject.SetActive(true);
		}
		public void CurrentlyAtNode()
		{
			nodeState = NodeState.currentlyAt;
			startEncounterButton.gameObject.SetActive(false);
		}

		//update ui
		void UpdateEncounterTitle()
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
		void UpdateEncounterModifiers()
		{
			string encounterModifiers = "Enviroment: \n";

			if (landTypes.HasFlag(LandTypes.grassland))
				encounterModifiers += "<color=green>Grasslands</color>";
			else if (landTypes.HasFlag(LandTypes.forest))
				encounterModifiers += "<color=#006400>Forest</color>"; //Gray
			else if (landTypes.HasFlag(LandTypes.mountains))
				encounterModifiers += "<color=grey>Mountains</color>";
			else if (landTypes.HasFlag(LandTypes.caves))
				encounterModifiers += "<color=black>Caves</color>";

			if (landTypes.HasFlag(LandTypes.ruins))
			{
				encounterModifiers += " with <color=#00FFFF>Ruins</color>"; //Cyan
				Debug.LogError("node has ruins: " + landTypes.ToString());
			}

			encounterModifiersText.text = encounterModifiers;
		}
		void UpdateEncounterEnemies()
		{
			string encounterEnemies = "Possible Enemies: \n";

			if (nodeEncounterType == NodeEncounterType.freeCardUpgrade)
			{
				encounterEnemies += "None";
				encounterEnemiesText.text = encounterEnemies;
				return;
			}

			foreach (EntityData entity in SpawnManager.instance.debugSpawnEntities)
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
			encounterEnemiesText.text = encounterEnemies;
		}
	}
}
