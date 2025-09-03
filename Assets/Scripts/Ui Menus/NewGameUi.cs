using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Woopsious.EntityData;
using static Woopsious.DamageData;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Woopsious
{
	public class NewGameUi : MonoBehaviour
	{
		public static NewGameUi instance;

		public GameObject newGamePanel;

		[Header("Class Ui Info")]
		public TMP_Text mageInfoText;
		public TMP_Text rangerInfoText;
		public TMP_Text rogueInfoText;
		public TMP_Text warriorInfoText;

		[Header("Class Select Buttons")]
		public Button playMageButton;
		public Button playRangerButton;
		public Button playRogueButton;
		public Button playWarriorButton;

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

		private void Awake()
		{
			SetUpButtons();
		}
		private void Start()
		{
			SetUpClassInfoUi();
		}

		void SetUpButtons()
		{
			playMageButton.onClick.AddListener(() => GameManager.SetPlayerClass(GetPlayerClass(PlayerClass.Mage)));
			playMageButton.onClick.AddListener(() => GameManager.LoadGameScene());

			playRangerButton.onClick.AddListener(() => GameManager.SetPlayerClass(GetPlayerClass(PlayerClass.Ranger)));
			playRangerButton.onClick.AddListener(() => GameManager.LoadGameScene());

			playRogueButton.onClick.AddListener(() => GameManager.SetPlayerClass(GetPlayerClass(PlayerClass.Rogue)));
			playRogueButton.onClick.AddListener(() => GameManager.LoadGameScene());

			playWarriorButton.onClick.AddListener(() => GameManager.SetPlayerClass(GetPlayerClass(PlayerClass.Warrior)));
			playWarriorButton.onClick.AddListener(() => GameManager.LoadGameScene());
		}
		EntityData GetPlayerClass(PlayerClass playerClass)
		{
			foreach (EntityData player in GameManager.instance.playerClassDataTypes)
			{
				if (player.playerClass == playerClass)
					return player;
			}

			return null;
		}

		//set info for every class on start
		void SetUpClassInfoUi()
		{
			foreach (EntityData playerData in GameManager.instance.playerClassDataTypes)
			{
				if (playerData.playerClass == PlayerClass.Mage)
					mageInfoText.text = playerData.CreatePlayerClassInfo();
				else if (playerData.playerClass == PlayerClass.Ranger)
					rangerInfoText.text = playerData.CreatePlayerClassInfo();
				else if (playerData.playerClass == PlayerClass.Rogue)
					rogueInfoText.text = playerData.CreatePlayerClassInfo();
				else if (playerData.playerClass == PlayerClass.Warrior)
					warriorInfoText.text = playerData.CreatePlayerClassInfo();
			}
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