using System;
using System.Collections.Generic;
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

		[Header("Player Classes Panel")]
		public GameObject playerClassPanel;

		public TMP_Text playerInfoText;
		private List<CardUi> startingDeckCards = new();

		[Header("Entity Panel")]
		public GameObject entityPanel;

		public Button viewAbberationsButton;
		public Button viewBeastsButton;
		public Button viewConstructsButton;
		public Button viewHumaniodsButton;
		public Button viewSlimesButton;
		public Button viewUndeadButton;

		List<EntityData> abberationsEntityData = new();
		List<EntityData> beastsEntityData = new();
		List<EntityData> constructsEntityData = new();
		List<EntityData> humanoidsEntityData = new();
		List<EntityData> slimesEntityData = new();
		List<EntityData> undeadEntityData = new();

		public GameObject enemyTypesPanel;
		EnemyTypes enemyTypesBeingDisplayed;
		List<Button> enemyTypesButtons = new();
		public TMP_Text enemyInfoText;
		List<CardUi> enemyMoveCards = new();

		[Header("Land Types Panel")]
		public GameObject landTypesPanel;

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
			SetupMainDatabasePanelButton();
		}
		void SetupMainDatabasePanelButton()
		{
			viewPlayerClassButton.onClick.AddListener(() => ShowPlayerClassesUi());
			viewEntitiesButton.onClick.AddListener(() => ShowEntitiesUi());
			viewLandTypesButton.onClick.AddListener(() => ShowLandTypesUi());
		}

		void Start()
		{
			SetupPlayerClassesUiPanel();
			SetupEntityUiPanel();
		}
		void SetupPlayerClassesUiPanel()
		{
			playerInfoText.text = "Select Player Class Button Above";

			int i = 0;

			foreach (EntityData playerClass in GameManager.instance.playerClassDataTypes)
			{
				Button button = Instantiate(buttonPrefab).GetComponent<Button>();
				button.transform.SetParent(playerClassPanel.transform);
				button.onClick.AddListener(() => ShowPlayerClassInfo(playerClass));
				
				TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
				buttonText.text = playerClass.entityName + " Info";
				button.name = playerClass.entityName + "InfoButton";

				RectTransform rectTransform = button.GetComponent<RectTransform>();
				rectTransform.anchoredPosition = new Vector2((200 * i) + 10, -10);
				i++;
			}
		}
		void SetupEntityUiPanel()
		{
			viewAbberationsButton.onClick.AddListener(() => ShowEntityTypesPanel(EnemyTypes.Abberration));
			viewBeastsButton.onClick.AddListener(() => ShowEntityTypesPanel(EnemyTypes.beast));
			viewConstructsButton.onClick.AddListener(() => ShowEntityTypesPanel(EnemyTypes.construct));
			viewHumaniodsButton.onClick.AddListener(() => ShowEntityTypesPanel(EnemyTypes.humanoid));
			viewSlimesButton.onClick.AddListener(() => ShowEntityTypesPanel(EnemyTypes.slime));
			viewUndeadButton.onClick.AddListener(() => ShowEntityTypesPanel(EnemyTypes.undead));

			foreach (EntityData entityData in GameManager.instance.entityDataTypes)
			{
				switch (entityData.enemyType)
				{
					case EnemyTypes.Abberration:
					abberationsEntityData.Add(entityData); 
					break;

					case EnemyTypes.beast:
					beastsEntityData.Add(entityData);
					break;

					case EnemyTypes.construct:
					constructsEntityData.Add(entityData);
					break;

					case EnemyTypes.humanoid:
					humanoidsEntityData.Add(entityData);
					break;

					case EnemyTypes.slime:
					slimesEntityData.Add(entityData);
					break;

					case EnemyTypes.undead:
					undeadEntityData.Add(entityData);
					break;
				}
			}
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
			playerClassPanel.SetActive(false);
			entityPanel.SetActive(false);
			landTypesPanel.SetActive(false);
		
			HidePlayerClassesUi();
			HideEntitiesUi();
		}

		//player classes database
		public void ShowPlayerClassesUi()
		{
			playerClassPanel.SetActive(true);
			HideEntitiesUi();
			HideLandTypesUi();
		}
		public void HidePlayerClassesUi()
		{
			playerClassPanel.SetActive(false);
			playerInfoText.text = "Select Player Class Button Above";
			ClearUpStartingCardsInfo();
		}

		void ShowPlayerClassInfo(EntityData playerClassData)
		{
			playerInfoText.text = playerClassData.CreatePlayerClassInfo();
			ClearUpStartingCardsInfo();

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
			cardUi.gameObject.transform.SetParent(playerClassPanel.transform);
			cardUi.SetupUiCard(entry.Key, entry.Value);
			startingDeckCards.Add(cardUi);

			RectTransform rectTransform = cardUi.GetComponent<RectTransform>();
			int posX;
			int posY;

			if (index < 3)
			{
				posX = (200 * index) + 385;
				posY = (-250 * 0) + -125;
			}
			else if (index >= 3 && index < 6)
			{
				posX = (200 * (index - 3)) + 385;
				posY = (-250 * 1) + -125;
			}
			else
			{
				posX = (200 * (index - 6)) + 385;
				posY = (-250 * 2) + -125;
			}

			rectTransform.anchorMin = new Vector2(0.5f, 1);
			rectTransform.anchorMax = new Vector2(0.5f, 1);
			rectTransform.pivot = new Vector2(0.5f, 1);
			rectTransform.anchoredPosition = new Vector2(posX, posY);
		}
		void ClearUpStartingCardsInfo()
		{
			for (int i = startingDeckCards.Count - 1; i >= 0; i--)
				Destroy(startingDeckCards[i].gameObject);

			startingDeckCards.Clear();
		}

		//entities database
		public void ShowEntitiesUi()
		{
			HidePlayerClassesUi();
			entityPanel.SetActive(true);
			HideLandTypesUi();
		}
		public void HideEntitiesUi()
		{
			entityPanel.SetActive(false);
			enemyInfoText.text = "Select entity to view info";
			ClearUpEntityMoveCardsInfo();
		}

		void ShowEntityTypesPanel(EnemyTypes enemyType)
		{
			if (enemyTypesBeingDisplayed == enemyType) return;
			enemyTypesBeingDisplayed = enemyType;

			switch (enemyType)
			{
				case EnemyTypes.Abberration:
				SetupEntityTypesPanel(abberationsEntityData);
				break;

				case EnemyTypes.beast:
				SetupEntityTypesPanel(beastsEntityData);
				break;

				case EnemyTypes.construct:
				SetupEntityTypesPanel(constructsEntityData);
				break;

				case EnemyTypes.humanoid:
				SetupEntityTypesPanel(humanoidsEntityData);
				break;

				case EnemyTypes.slime:
				SetupEntityTypesPanel(slimesEntityData);
				break;

				case EnemyTypes.undead:
				SetupEntityTypesPanel(undeadEntityData);
				break;
			}
		}
		void SetupEntityTypesPanel(List<EntityData> entityDatas)
		{
			ClearUpEntityTypeButtons();
			int i = 0;

			foreach (EntityData entityData in entityDatas)
			{
				Button button = Instantiate(buttonPrefab).GetComponent<Button>();
				button.transform.SetParent(enemyTypesPanel.transform);
				button.onClick.AddListener(() => ShowEntityData(entityData));
				enemyTypesButtons.Add(button);

				TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
				buttonText.text = entityData.entityName + " Info";
				button.name = entityData.entityName + "InfoButton";

				RectTransform rectTransform = button.GetComponent<RectTransform>();
				rectTransform.anchoredPosition = new Vector2(10, (-75 * i) - 25);
				i++;
			}
		}
		void ClearUpEntityTypeButtons()
		{
			for (int i = enemyTypesButtons.Count - 1; i >= 0; i--)
				Destroy(enemyTypesButtons[i].gameObject);

			enemyTypesButtons.Clear();
		}

		void ShowEntityData(EntityData entityData)
		{
			ClearUpEntityMoveCardsInfo();

			enemyInfoText.text = entityData.CreateEntityInfo();
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
				ShowEntityMovesInfo(move, index);
				index++;
			}
		}
		void ShowEntityMovesInfo(AttackData attackMove, int index)
		{
			CardUi cardUi = Instantiate(cardUiPrefab).GetComponent<CardUi>();
			cardUi.gameObject.transform.SetParent(enemyTypesPanel.transform);
			cardUi.SetupUiCard(attackMove);
			enemyMoveCards.Add(cardUi);

			RectTransform rectTransform = cardUi.GetComponent<RectTransform>();
			int posX;
			int posY;

			if (index < 3)
			{
				posX = (200 * index) + 385;
				posY = (-250 * 0) + -125;
			}
			else if (index >= 3 && index < 6)
			{
				posX = (200 * (index - 3)) + 385;
				posY = (-250 * 1) + -125;
			}
			else
			{
				posX = (200 * (index - 6)) + 385;
				posY = (-250 * 2) + -125;
			}

			rectTransform.anchorMin = new Vector2(0.5f, 1);
			rectTransform.anchorMax = new Vector2(0.5f, 1);
			rectTransform.pivot = new Vector2(0.5f, 1);
			rectTransform.anchoredPosition = new Vector2(posX, posY);
		}
		void ClearUpEntityMoveCardsInfo()
		{
			for (int i = enemyMoveCards.Count - 1; i >= 0; i--)
				Destroy(enemyMoveCards[i].gameObject);

			enemyMoveCards.Clear();
		}

		//land types database
		public void ShowLandTypesUi()
		{
			HidePlayerClassesUi();
			HideEntitiesUi();
			landTypesPanel.SetActive(true);
		}
		public void HideLandTypesUi()
		{
			landTypesPanel.SetActive(false);
		}
	}
}
