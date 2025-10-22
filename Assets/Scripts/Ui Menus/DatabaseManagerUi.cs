using System;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;
using static Woopsious.EntityData;

namespace Woopsious
{
	public class DatabaseManagerUi : MonoBehaviour
	{
		public static DatabaseManagerUi instance;

		public GameObject buttonPrefab;
		public GameObject cardUiPrefab;

		[Header("Database Panel")]
		public GameObject databasePanel;

		public Button viewPlayerClassButton;
		public Button viewEntitiesButton;
		public Button viewLandTypesButton;

		[Header("Database Outer Panel")]
		public GameObject databaseOuterPanel;

		public Transform outerButtonsParent;
		public List<Button> outerButtons = new(); //set up to display player classes/entity types/land types

		[Header("Database Inner Panel")]
		public GameObject databaseInnerPanel;

		public Transform innerButtonsParent;
		public List<Button> innerButtons = new(); //set up to display entities of a specific entity type

		public TMP_Text infoToDisplayText;
		public TMP_Text ClassesMovesText;

		List<CardUi> cardUiList = new();

		void OnEnable()
		{
			MainMenuUi.ShowMainMenuUi += HideDatabaseUi;
			MainMenuUi.ShowNewGameUi += HideDatabaseUi;
			MainMenuUi.ShowSaveSlotsUi += HideDatabaseUi;
			MainMenuUi.ShowSettingsUi += HideDatabaseUi;
			MainMenuUi.ShowDatabaseUi += ShowDatabaseUi;
		}
		void OnDestroy()
		{
			MainMenuUi.ShowMainMenuUi -= HideDatabaseUi;
			MainMenuUi.ShowNewGameUi -= HideDatabaseUi;
			MainMenuUi.ShowSaveSlotsUi -= HideDatabaseUi;
			MainMenuUi.ShowSettingsUi -= HideDatabaseUi;
			MainMenuUi.ShowDatabaseUi += ShowDatabaseUi;
		}

		void Awake()
		{
			instance = this;
			SetupMainDatabaseButtons();
			SetupButtonsForPanels();
		}

		void SetupMainDatabaseButtons()
		{
			viewPlayerClassButton.onClick.AddListener(() => ShowPlayerClassesUi());
			viewEntitiesButton.onClick.AddListener(() => ShowEntitiesUi());
			viewLandTypesButton.onClick.AddListener(() => ShowMapNodeTypesUi());
		}
		void SetupButtonsForPanels()
		{
			int outerButtons = 10;

			for (int i = 0; i < outerButtons; i++)
				SetupOuterButtons(i);

			int innerButtons = 15;
			for (int i = 0; i < innerButtons; i++)
				SetupInnerButtons(i);
		}
		void SetupOuterButtons(int i)
		{
			Button button = Instantiate(buttonPrefab).GetComponent<Button>();
			outerButtons.Add(button);
			button.name = "OuterButton" + i;

			RectTransform rectTransform = button.GetComponent<RectTransform>();
			rectTransform.SetParent(outerButtonsParent);
			rectTransform.sizeDelta = new Vector2(200, 50);
			rectTransform.anchoredPosition = new Vector2(10, (-98 * i) - 25);
		}
		void SetupInnerButtons(int i)
		{
			Button button = Instantiate(buttonPrefab).GetComponent<Button>();
			innerButtons.Add(button);
			button.name = "InnerButton" + i;

			RectTransform rectTransform = button.GetComponent<RectTransform>();
			rectTransform.SetParent(innerButtonsParent);
			rectTransform.sizeDelta = new Vector2(160, 40);
			rectTransform.anchoredPosition = new Vector2(10, (-64f * i) - 25);
		}

		//main database panel
		void ShowDatabaseUi()
		{
			MainMenuUi.Instance.backButton.gameObject.transform.SetParent(databasePanel.transform);
			databasePanel.SetActive(true);
		}
		void HideDatabaseUi()
		{
			databasePanel.SetActive(false);

			ResetAllUiElements();
		}

		//show and set up specific database ui
		void ShowPlayerClassesUi()
		{
			ResetAllUiElements();
			SetupPlayerClassesButtons();

			infoToDisplayText.text = "Click button to view player class info";
			ClassesMovesText.text = "Classes Starting Deck";
			ClassesMovesText.gameObject.SetActive(true);
		}
		void ShowEntitiesUi()
		{
			ResetAllUiElements();
			SetupEnemiesOfTypeButtons();

			infoToDisplayText.text = "Click button to view enemy info";
			ClassesMovesText.text = "Enemy Moves";
			ClassesMovesText.gameObject.SetActive(true);
			innerButtonsParent.gameObject.SetActive(true);
		}
		void ShowMapNodeTypesUi()
		{
			ResetAllUiElements();
			SetupLandTypeButtons();
			infoToDisplayText.text = "Click button to view land type info";
		}

		//player classes database
		void SetupPlayerClassesButtons()
		{
			for (int i = 0; i < GameManager.instance.playerClassDataTypes.Count; i++)
			{
				EntityData playerClass = GameManager.instance.playerClassDataTypes[i];
				Button button = outerButtons[i];
				button.onClick.AddListener(() => ShowPlayerClassInfo(playerClass));

				TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
				buttonText.text = playerClass.entityName + " Info";
				button.name = playerClass.entityName + "InfoButton";
				button.gameObject.SetActive(true);
			}
		}
		void ShowPlayerClassInfo(EntityData playerClassData)
		{
			ClearCardUiList();
			infoToDisplayText.text = playerClassData.CreatePlayerClassInfo();

			//create new starting deck info
			Dictionary<AttackData, int> cardDeckCount = new();

			foreach (AttackData cardAttackData in playerClassData.cards)
			{
				if (cardDeckCount.ContainsKey(cardAttackData))
					cardDeckCount[cardAttackData]++;
				else
					cardDeckCount.Add(cardAttackData, 1);
			}

			int index = 0;
			foreach (KeyValuePair<AttackData, int> entry in cardDeckCount)
			{
				DisplayStartingCardInfo(entry, index);
				index++;
			}
		}
		void DisplayStartingCardInfo(KeyValuePair<AttackData, int> entry, int index)
		{
			CardUi cardUi = Instantiate(cardUiPrefab).GetComponent<CardUi>();
			cardUi.gameObject.transform.SetParent(databaseInnerPanel.transform);
			cardUi.SetupUiCard(entry.Key, entry.Value);
			cardUiList.Add(cardUi);
			SetCardUiPosition(cardUi.GetComponent<RectTransform>(), index);
		}

		//entities database
		void SetupEnemiesOfTypeButtons()
		{
			SetupEnemiesOfTypeButton(0, GameManager.instance.AbberationsEnemyData);
			SetupEnemiesOfTypeButton(1, GameManager.instance.BeastsEnemyData);
			SetupEnemiesOfTypeButton(2, GameManager.instance.ConstructsEnemyData);
			SetupEnemiesOfTypeButton(3, GameManager.instance.HumanoidsEnemyData);
			SetupEnemiesOfTypeButton(4, GameManager.instance.SlimesEnemyData);
			SetupEnemiesOfTypeButton(5, GameManager.instance.UndeadEnemyData);
		}
		void SetupEnemiesOfTypeButton(int i, List<EntityData> enemiesOfType)
		{
			if (enemiesOfType.Count == 0)
			{
				Debug.LogWarning("No Enemies of this type to display");
				return;
			}

			Button button = outerButtons[i];
			button.onClick.AddListener(() => SetupEnemyButtons(enemiesOfType));

			TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
			buttonText.text = enemiesOfType[0].enemyType + "s";
			button.name = enemiesOfType[0].enemyType + "InfoButton";
			button.gameObject.SetActive(true);
		}

		void SetupEnemyButtons(List<EntityData> enemiesOfType)
		{
			ResetInnerButtons();

			for (int i = 0; i < enemiesOfType.Count; i++)
			{
				EntityData enemyData = enemiesOfType[i];
				Button button = innerButtons[i];
				button.onClick.AddListener(() => ShowEnemyInfo(enemyData));

				TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
				buttonText.fontSize = 16;
				buttonText.text = enemyData.entityName + " Info";
				button.name = enemyData.entityName + "InfoButton";
				button.gameObject.SetActive(true);
			}
		}
		void ShowEnemyInfo(EntityData entityData)
		{
			ClearCardUiList();
			infoToDisplayText.text = entityData.CreateEntityInfo();
			List<AttackData> entityMoves = new();

			foreach (MoveSetData moveSet in entityData.moveSetOrder)
			{
				foreach (AttackData attackData in moveSet.moveSetMoves)
				{
					if (entityMoves.Contains(attackData)) continue;
					entityMoves.Add(attackData);
				}
			}

			int index = 0;
			foreach (AttackData move in entityMoves)
			{
				ShowEnemyMovesInfo(move, index);
				index++;
			}
		}
		void ShowEnemyMovesInfo(AttackData attackMove, int index)
		{
			CardUi cardUi = Instantiate(cardUiPrefab).GetComponent<CardUi>();
			cardUi.gameObject.transform.SetParent(databaseInnerPanel.transform);
			cardUi.SetupUiCard(attackMove);
			cardUiList.Add(cardUi);
			SetCardUiPosition(cardUi.GetComponent<RectTransform>(), index);
		}

		//Map Node types database
		void SetupLandTypeButtons()
		{
			for (int i = 0; i < GameManager.instance.mapNodeDataTypes.Count; i++)
			{
				MapNodeData mapNodeData = GameManager.instance.mapNodeDataTypes[i];
				Button button = outerButtons[i];
				button.onClick.AddListener(() => ShowMapNodeData(mapNodeData));

				TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
				buttonText.text = mapNodeData.nodeName + " Info";
				button.name = mapNodeData.nodeName + "InfoButton";
				button.gameObject.SetActive(true);
			}
		}
		void ShowMapNodeData(MapNodeData mapNodeData)
		{
			infoToDisplayText.text = mapNodeData.CreateMapNodeLandTypeInfo();
		}

		//set card ui positions when showing player class and enemy info
		void SetCardUiPosition(RectTransform rectTransform, int index)
		{
			int posX;
			int posY;

			if (index < 3)
			{
				posX = (200 * index) + 300;
				posY = (-250 * 0) + -125;
			}
			else if (index >= 3 && index < 6)
			{
				posX = (200 * (index - 3)) + 300;
				posY = (-250 * 1) + -125;
			}
			else
			{
				posX = (200 * (index - 6)) + 300;
				posY = (-250 * 2) + -125;
			}

			rectTransform.anchorMin = new Vector2(0.5f, 1);
			rectTransform.anchorMax = new Vector2(0.5f, 1);
			rectTransform.pivot = new Vector2(0.5f, 1);
			rectTransform.anchoredPosition = new Vector2(posX, posY);
		}

		//reset ui elements
		void ResetAllUiElements()
		{
			ClassesMovesText.gameObject.SetActive(false);
			innerButtonsParent.gameObject.SetActive(false);
			infoToDisplayText.text = "Select button above to view databases";

			ResetOuterButtons();
			ResetInnerButtons();
			ClearCardUiList();
		}
		void ResetOuterButtons()
		{
			for(int i = 0; i < outerButtons.Count; i++)
			{
				Button button = outerButtons[i];
				button.gameObject.SetActive(false);
				button.onClick.RemoveAllListeners();
				button.name = "OuterButton" + i;
			}
		}
		void ResetInnerButtons()
		{
			for (int i = 0; i < innerButtons.Count; i++)
			{
				Button button = innerButtons[i];
				button.gameObject.SetActive(false);
				button.onClick.RemoveAllListeners();
				button.name = "InnerButton" + i;
			}
		}

		//reset reusable card ui list
		void ClearCardUiList()
		{
			for (int i = cardUiList.Count - 1; i >= 0; i--)
				Destroy(cardUiList[i].gameObject);

			cardUiList.Clear();
		}
	}
}
