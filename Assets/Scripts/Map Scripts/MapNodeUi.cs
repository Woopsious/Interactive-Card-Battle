using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Woopsious.EntityData;
using static Woopsious.MapNodeData;

namespace Woopsious
{
	public class MapNodeUi : MonoBehaviour
	{
		private MapNode mapNode;

		[Header("Ui Elements")]
		public TMP_Text encounterTitleText;
		public TMP_Text encounterLandTypeText;
		public TMP_Text encounterModifiersText;
		public TMP_Text encounterEnemiesText;
		public GameObject startEncounterButton;

		Image backgroundImage;

		[Header("Debug Ui Elements")]
		public TMP_Text debugDataText;

		[Header("Debug options")]
		public bool DisplayEncounterEnemies;

		private Color _ColourRed = new(0.55f, 0.25f, 0.25f);
		private Color _ColourGreen = new(0.25f, 0.5f, 0.25f);
		private Color _ColourGold = new(1f, 0.85f, 0f);

		void Awake()
		{
			mapNode = GetComponent<MapNode>();
			backgroundImage = GetComponent<Image>();
			mapNode.InitilizeUi += InitilizeUi;
			mapNode.NodeStateChange += UpdateNodeStateUi;
		}
		private void OnDestroy()
		{
			mapNode.InitilizeUi -= InitilizeUi;
			mapNode.NodeStateChange -= UpdateNodeStateUi;
		}

		void InitilizeUi()
		{
			encounterTitleText.text = UpdateEncounterTitleUi();
			encounterLandTypeText.text = UpdateEncounterLandTypeUi();
			encounterModifiersText.text = UpdateEncounterModifiersUi();
			encounterEnemiesText.text = UpdateEncounterEnemiesUi();

			if (debugDataText)
			{
				debugDataText.text = DebugDataTextToUi();
				debugDataText.gameObject.SetActive(true);
			}
		}

		//start encounter button press
		public void StartEncounterButton()
		{
			mapNode.BeginEncounter();
		}

		//node state ui changes
		public void UpdateNodeStateUi(NodeState nodeState)
		{
			switch (nodeState)
			{
				case NodeState.locked:
				startEncounterButton.SetActive(false);
				backgroundImage.color = _ColourRed;
				break;
				case NodeState.canTravel:
				startEncounterButton.SetActive(true);
				backgroundImage.color = _ColourGreen;
				break;
				case NodeState.currentlyAt:
				startEncounterButton.SetActive(false);
				backgroundImage.color = _ColourGold;
				break;
				case NodeState.previouslyVisited:
				startEncounterButton.SetActive(false);
				backgroundImage.color = _ColourGold;
				break;
			}
		}

		//update ui
		string UpdateEncounterTitleUi()
		{
			return RichTextManager.GetEncounterTypeTextColour(mapNode.nodeEncounterType);
		}
		string UpdateEncounterLandTypeUi()
		{
			return RichTextManager.GetLandTypesTextColour(mapNode.landTypes);
		}
		string UpdateEncounterModifiersUi()
		{
			string encounterModifiers = "Modifiers: \n";
			encounterModifiers += RichTextManager.GetLandModifiersTextColour(mapNode.landModifiers);
			return encounterModifiers;
		}
		string UpdateEncounterEnemiesUi()
		{
			string encounterEnemies = "Possible Enemies: \n";

			if (mapNode.nodeEncounterType == NodeEncounterType.freeCardUpgrade)
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
				if ((mapNode.landTypes & entity.foundInLandTypes) != LandTypes.none || 
					(mapNode.landModifiers & entity.foundWithLandModifiers) != LandModifiers.none)
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
			return RichTextManager.GetEnemyTypesTextColour(mapNode.enemyTypes);
		}
		string DebugDataTextToUi()
		{
			string debugData = "Encounter Difficulty: " + mapNode.encounterDifficulty + "\nEncounterBudget: " + mapNode.entityBudget;
			return debugData;
		}
	}
}
