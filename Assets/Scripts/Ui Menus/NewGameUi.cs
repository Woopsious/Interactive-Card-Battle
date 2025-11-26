using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Woopsious.EntityData;
using static Woopsious.CardHandler;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Woopsious
{
	public class NewGameUi : MonoBehaviour
	{
		public static NewGameUi instance;

		public GameObject newGamePanel;
		public TMP_Text titleText;

		[Header("Play As Class Buttons")]
		public Button playMageButton;
		public Button playRangerButton;
		public Button playRogueButton;
		public Button playWarriorButton;

		[Header("Class Info Ui Elements")]
		public GameObject classInfoPanel;
		public TMP_Text classInfoText;
		public Button startGameAsClassButton;
		TMP_Text startGameAsClassText;
		List<CardHandler> cardList = new();

		void OnEnable()
		{
			SceneManager.sceneLoaded += OnSceneLoadedEvent;
			MainMenuUi.ShowMainMenuUi += HideNewGameUi;
			MainMenuUi.ShowNewGameUi += ShowNewGameUi;
			MainMenuUi.ShowSaveSlotsUi += HideNewGameUi;
			MainMenuUi.ShowSettingsUi += HideNewGameUi;
			MainMenuUi.ShowDatabaseUi += HideNewGameUi;
		}
		void OnDestroy()
		{
			SceneManager.sceneLoaded -= OnSceneLoadedEvent;
			MainMenuUi.ShowMainMenuUi -= HideNewGameUi;
			MainMenuUi.ShowNewGameUi -= ShowNewGameUi;
			MainMenuUi.ShowSaveSlotsUi -= HideNewGameUi;
			MainMenuUi.ShowSettingsUi -= HideNewGameUi;
			MainMenuUi.ShowDatabaseUi -= HideNewGameUi;
		}

		private void Start()
		{
			SetUpButtons();
		}
		void SetUpButtons()
		{
			playMageButton.onClick.AddListener(() => GameManager.SetPlayerClass(GetPlayerClass(PlayerClass.Mage)));
			playMageButton.onClick.AddListener(() => SetUpClassInfoUi(GetPlayerClass(PlayerClass.Mage)));

			playRangerButton.onClick.AddListener(() => GameManager.SetPlayerClass(GetPlayerClass(PlayerClass.Ranger)));
			playRangerButton.onClick.AddListener(() => SetUpClassInfoUi(GetPlayerClass(PlayerClass.Ranger)));

			playRogueButton.onClick.AddListener(() => GameManager.SetPlayerClass(GetPlayerClass(PlayerClass.Rogue)));
			playRogueButton.onClick.AddListener(() => SetUpClassInfoUi(GetPlayerClass(PlayerClass.Rogue)));

			playWarriorButton.onClick.AddListener(() => GameManager.SetPlayerClass(GetPlayerClass(PlayerClass.Warrior)));
			playWarriorButton.onClick.AddListener(() => SetUpClassInfoUi(GetPlayerClass(PlayerClass.Warrior)));

			startGameAsClassText = startGameAsClassButton.GetComponentInChildren<TMP_Text>();
			startGameAsClassText.text = "Select Class To Play";
			startGameAsClassButton.interactable = false;
			startGameAsClassButton.onClick.AddListener(() => GameManager.LoadGameScene());
		}
		EntityData GetPlayerClass(PlayerClass playerClass)
		{
			foreach (EntityData player in GameManager.instance.playerClassDataTypes)
			{
				if (player.playerClass == playerClass)
					return player;
			}

			Debug.LogError("failed to get player class, returning null");
			return null;
		}

		//show player class info
		void SetUpClassInfoUi(EntityData playerClassData)
		{
			ClearCardUiList();
			classInfoText.text = playerClassData.CreatePlayerClassInfo();
			startGameAsClassText.text = "Start Game As " + playerClassData.entityName;
			startGameAsClassButton.interactable = true;

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
			CardHandler card = Instantiate(DatabaseManagerUi.instance.cardUiPrefab).GetComponent<CardHandler>();
			card.gameObject.transform.SetParent(classInfoPanel.transform);
			card.SetupCard(CardInitType.Informational, null, entry.Key, false, entry.Value);
			cardList.Add(card);
			SetCardUiPosition(card.GetComponent<RectTransform>(), index);
		}

		//set card ui positions when showing player class and enemy info
		void SetCardUiPosition(RectTransform rectTransform, int index)
		{
			int posX;
			int posY;

			if (index < 3)
			{
				posX = (250 * index) + 250;
				posY = (-250 * 0) + -25;
			}
			else if (index >= 3 && index < 6)
			{
				posX = (250 * (index - 3)) + 250;
				posY = (-250 * 1) + -25;
			}
			else
			{
				posX = (250 * (index - 6)) + 250;
				posY = (-250 * 2) + -25;
			}

			rectTransform.anchorMin = new Vector2(0.5f, 1);
			rectTransform.anchorMax = new Vector2(0.5f, 1);
			rectTransform.pivot = new Vector2(0.5f, 1);
			rectTransform.anchoredPosition = new Vector2(posX, posY);
		}

		//reset reusable card ui list
		void ClearCardUiList()
		{
			for (int i = cardList.Count - 1; i >= 0; i--)
				Destroy(cardList[i].gameObject);

			cardList.Clear();
		}

		//scene change events
		void OnSceneLoadedEvent(Scene loadedScene, LoadSceneMode mode)
		{
			if (loadedScene.name == GameManager.instance.gameScene)
			{
				HideNewGameUi();
			}
		}

		void ShowNewGameUi()
		{
			newGamePanel.SetActive(true);
			MainMenuUi.Instance.backButton.gameObject.transform.SetParent(newGamePanel.transform);
		}
		void HideNewGameUi()
		{
			newGamePanel.SetActive(false);
		}
	}

}