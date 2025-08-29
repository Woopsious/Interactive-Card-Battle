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
		}
		void OnDestroy()
		{
			SceneManager.sceneLoaded -= OnSceneLoadedEvent;
			MainMenuUi.ShowMainMenuUi -= HideNewGameUi;
			MainMenuUi.ShowNewGameUi -= ShowNewGameUi;
			MainMenuUi.ShowSaveSlotsUi -= HideNewGameUi;
			MainMenuUi.ShowSettingsUi -= HideNewGameUi;
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
			playMageButton.onClick.AddListener(delegate { GameManager.SetPlayerClass(GetPlayerClass(PlayerClass.Mage)); });
			playMageButton.onClick.AddListener(delegate { GameManager.LoadGameScene(); });

			playRangerButton.onClick.AddListener(delegate { GameManager.SetPlayerClass(GetPlayerClass(PlayerClass.Ranger)); });
			playRangerButton.onClick.AddListener(delegate { GameManager.LoadGameScene(); });

			playRogueButton.onClick.AddListener(delegate { GameManager.SetPlayerClass(GetPlayerClass(PlayerClass.Rogue)); });
			playRogueButton.onClick.AddListener(delegate { GameManager.LoadGameScene(); });

			playWarriorButton.onClick.AddListener(delegate { GameManager.SetPlayerClass(GetPlayerClass(PlayerClass.Warrior)); });
			playWarriorButton.onClick.AddListener(delegate { GameManager.LoadGameScene(); });
		}
		EntityData GetPlayerClass(PlayerClass playerClass)
		{
			foreach (EntityData player in SpawnManager.instance.playerClassDataTypes)
			{
				if (player.playerClass == playerClass)
					return player;
			}

			return null;
		}

		//set info for every class on start
		void SetUpClassInfoUi()
		{
			foreach (EntityData playerData in SpawnManager.instance.playerClassDataTypes)
			{
				if (playerData.playerClass == PlayerClass.Mage)
					mageInfoText.text = SetClassInfoForUi(playerData);
				else if (playerData.playerClass == PlayerClass.Ranger)
					rangerInfoText.text = SetClassInfoForUi(playerData);
				else if (playerData.playerClass == PlayerClass.Rogue)
					rogueInfoText.text = SetClassInfoForUi(playerData);
				else if (playerData.playerClass == PlayerClass.Warrior)
					warriorInfoText.text = SetClassInfoForUi(playerData);
			}
		}
		string SetClassInfoForUi(EntityData playerData)
		{
			string classInfo = "";

			classInfo += playerData.entityName + "\n\n";

			classInfo += $"Health: {playerData.maxHealth}\n\n";

			classInfo += "STARTING CARDS\n";
			classInfo += SetClassCardsInfo(playerData.cards) + "\n\n";

			classInfo += "Class specialty ";
			classInfo += ExplainClassGimmick(playerData);

			return classInfo;
		}
		string SetClassCardsInfo(List<AttackData> playerCards)
		{
			int multiCardsCount = 0;
			int attackCardsCount = 0;
			int blockCardsCount = 0;
			int healCardsCount = 0;

			foreach (AttackData attackData in playerCards)
			{
				DamageData damageData = attackData.DamageData;
				if (damageData.DamageValue != 0 && damageData.BlockValue != 0 || damageData.DamageValue != 0 && damageData.HealValue != 0)
					multiCardsCount++;
				else if (damageData.DamageValue != 0)
					attackCardsCount++;
				else if (damageData.BlockValue != 0)
					blockCardsCount++;
				else if (damageData.HealValue != 0)
					healCardsCount++;
			}

			string cardsInfo = "";
			cardsInfo += $"Multi Cards: {multiCardsCount}\nAttack Cards: {attackCardsCount}\nBlock Cards: {blockCardsCount}\nHeal Cards: {healCardsCount}";
			return cardsInfo;
			//at some point also add the dispalying of each type of starting cards details
		}
		string ExplainClassGimmick(EntityData playerData)
		{
			string classGimmickInfo = "";

			if (playerData.playerClass == PlayerClass.Mage)
			{
				classGimmickInfo += $"\"Potent Magic\"\n Has a {playerData.chanceOfDoubleDamage}% to deal double the damage of a card";
			}
			else if (playerData.playerClass == PlayerClass.Ranger)
			{
				classGimmickInfo += $"\"Resourceful\"\n Heals for {playerData.healOnKillPercentage}% of health on killing an enemy";
			}
			else if (playerData.playerClass == PlayerClass.Rogue)
			{
				classGimmickInfo += $"\"Trickster\"\n Reflects {playerData.damageReflectedPercentage}% of damage recieved onto attacker";
			}
			else if (playerData.playerClass == PlayerClass.Warrior)
			{
				classGimmickInfo += $"\"Stalwart\"\n Has a permanent {playerData.baseBlock} block";
			}

			return classGimmickInfo;
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