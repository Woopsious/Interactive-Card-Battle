using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

namespace Woopsious
{
	public class DatabaseManagerUi : MonoBehaviour
	{
		public static DatabaseManagerUi instance;

		public GameObject buttonPrefab;

		[Header("Database Panel")]
		public GameObject databasePanel;

		public Button viewPlayerClassButton;
		public Button viewEntitiesButton;
		public Button viewLandTypesButton;

		[Header("Player Classes Panel")]
		public GameObject playerClassPanel;
		public GameObject cardUiPrefab;

		private List<CardUi> startingDeckCards = new();
		public TMP_Text playerInfoText;

		[Header("Entity Panel")]
		public GameObject entityPanel;

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
