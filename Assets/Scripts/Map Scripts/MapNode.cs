using UnityEngine;
using static Woopsious.MapNodeData;
using static Woopsious.EntityData;
using TMPro;
using System.Collections.Generic;

namespace Woopsious
{
	public class MapNode : MonoBehaviour
	{
		[Header("Ui")]
		public TMP_Text encounterTitleText;
		public TMP_Text encounterModifiersText;
		public TMP_Text encounterEnemiesText;

		//runtime data
		public MapNodeData mapNodeData;
		public int entityBudget;
		public NodeState nodeState;
		public EnemyTypes enemyTypes;
		public LandTypes landTypes;
		public NodeEncounterType nodeType;

		void Start()
		{
			if (mapNodeData != null)
				Initilize(mapNodeData);
			else
				Debug.LogWarning("Map node data not set, ignore if intended");
		}

		public void Initilize(MapNodeData mapNodeData)
		{
			this.mapNodeData = mapNodeData;
			entityBudget = mapNodeData.entityBudget;
			nodeState = NodeState.locked;
			landTypes = mapNodeData.landTypes;

			if (GetRandomNumber() < mapNodeData.chanceOfFreeCardUpgrade)
			{
				SetNodeInteractTypeToFreeCardUpgrade();
				return;
			}

			if (GetRandomNumber() < mapNodeData.chanceOfRuins)
				landTypes = AddRuinsLandType();

			if (GetRandomNumber() < mapNodeData.chanceOfEliteFight)
				nodeType = MakeEncounterElite();

			UpdateEncounterTitle();
			UpdateEncounterModifiers();
			UpdateEncounterEnemies();
		}

		//update node runtime data
		void SetNodeInteractTypeToFreeCardUpgrade()
		{
			enemyTypes = EnemyTypes.none;
			nodeType = NodeEncounterType.freeCardUpgrade;
		}
		NodeEncounterType MakeEncounterElite()
		{
			if (nodeType == NodeEncounterType.basicFight)
				return NodeEncounterType.eliteFight;
			else if (nodeType == NodeEncounterType.bossFight)
				return NodeEncounterType.eliteBossFight;
			else
			{
				Debug.LogError("no elite version of encounter found cancelling");
				return nodeType;
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

		//update ui
		void UpdateEncounterTitle()
		{
			string encounterTitle = "";

			switch (nodeType)
			{
				case NodeEncounterType.basicFight:
				encounterTitle = "Basic Fight";
				break;
				case NodeEncounterType.eliteFight:
				encounterTitle = "Elite Fight";
				break;
				case NodeEncounterType.bossFight:
				encounterTitle = "Boss Fight";
				break;
				case NodeEncounterType.eliteBossFight:
				encounterTitle = "Elite Boss Fight";
				break;
				case NodeEncounterType.freeCardUpgrade:
				encounterTitle = "Free Card Upgrade";
				break;
			}

			encounterTitleText.text = encounterTitle;
		}
		void UpdateEncounterModifiers()
		{
			string encounterModifiers = "Enviroment: \n";

			switch (landTypes)
			{
				case LandTypes.grassland:
				encounterModifiers += "Grasslands";
				break;
				case LandTypes.forest:
				encounterModifiers += "Forest";
				break;
				case LandTypes.mountains:
				encounterModifiers += "Mountains";
				break;
				case LandTypes.caves:
				encounterModifiers += "Caves";
				break;
			}

			if (landTypes.HasFlag(LandTypes.ruins))
				encounterModifiers += " with Ruins";

			encounterModifiersText.text = encounterModifiers;
		}
		void UpdateEncounterEnemies()
		{
			string encounterEnemies = "Possible Enemies: \n";

			foreach (EntityData entity in SpawnManager.instance.debugSpawnEntities)
			{
				if ((landTypes & entity.foundInLandTypes) != LandTypes.none) //check if any land type flags match
					encounterEnemies += entity.name + ", ";
			}

			encounterEnemiesText.text = encounterEnemies;
		}
	}
}
