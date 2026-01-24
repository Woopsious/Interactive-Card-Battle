using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Woopsious.AbilitySystem;

namespace Woopsious
{
	public class CardCombatUi : MonoBehaviour
	{
		public static CardCombatUi instance;

		[Header("Ui Panels")]
		public GameObject CardDeckUi;
		public GameObject PlayedCardUi;
		public GameObject GameLostUi;
		public RectTransform spawnedEntitiesTransform;
		public RectTransform DraggedCardsTransform;

		[Header("Ui Elements")]
		public TMP_Text currentRoundInfoText;
		public TMP_Text currentTurnInfoText;

		public GameObject EndPlayerTurnButtonObj;

		[Header("Debug Ui Elements")]
		public GameObject DebugEndTurnButtonObj;
		public GameObject debugAddDummyCardButton;
		public GameObject debugStartCombatButton;
		public GameObject debugEndCombatButton;

		[Header("DEBUG FIGHT ENEMIES AT RUNTIME")]
		public List<EntityData> listOfEntitiesToFight = new();

		private void Awake()
		{
			instance = this;
		}
		private void OnEnable()
		{
			GameManager.OnGameStateChange += OnGameStateChange;
			TurnOrderManager.OnNewRoundStartEvent += OnNewRound;
			TurnOrderManager.OnStartTurn += OnNewTurn;
		}
		private void OnDisable()
		{
			GameManager.OnGameStateChange -= OnGameStateChange;
			TurnOrderManager.OnNewRoundStartEvent -= OnNewRound;
			TurnOrderManager.OnStartTurn -= OnNewTurn;
		}

		//Event Listeners
		private void OnNewRound(int currentRound)
		{
			UpdateCurrentRoundText(currentRound);
		}
		private void OnNewTurn(Entity entity)
		{
			ShowHideEndPlayerTurnButton(entity);
			UpdateCurrentTurnText(entity);
		}


		//UI UPDATES
		private void OnGameStateChange(GameManager.GameState gameState)
		{
			if (GameManager.GameState.MapView == gameState)
			{
				HideCardCombatUi();
			}
			else if (GameManager.GameState.CardCombat == gameState || GameManager.GameState.debugCombat == gameState)
			{
				ShowCardCombatUi();
			}
			else if (GameManager.GameState.CardCombatLoss == gameState)
			{
				ShowGameLossUi();
			}
		}
		private void UpdateCurrentRoundText(int currentRound)
		{
			currentRoundInfoText.text = "Round: " + currentRound;
		}
		private void UpdateCurrentTurnText(Entity entity)
		{
			currentTurnInfoText.text = entity.EntityData.entityName + "'s turn";
		}
		private void ShowHideEndPlayerTurnButton(Entity entity)
		{
			if (TurnOrderManager.Player() == entity)
				EndPlayerTurnButtonObj.SetActive(true);
			else
				EndPlayerTurnButtonObj.SetActive(false);
		}

		//event listeners
		private void HideCardCombatUi()
		{
			CardDeckUi.SetActive(false);
			PlayedCardUi.SetActive(false);
			GameLostUi.SetActive(false);

			currentRoundInfoText.gameObject.SetActive(false);
			currentTurnInfoText.gameObject.SetActive(false);

			debugAddDummyCardButton.SetActive(false);
			debugStartCombatButton.SetActive(true);
			debugEndCombatButton.SetActive(false);

			EndPlayerTurnButtonObj.SetActive(false);
			DebugEndTurnButtonObj.SetActive(false);
		}
		private void ShowCardCombatUi()
		{
			CardDeckUi.SetActive(true);
			PlayedCardUi.SetActive(true);
			GameLostUi.SetActive(false);

			currentRoundInfoText.gameObject.SetActive(true);
			currentTurnInfoText.gameObject.SetActive(true);

			debugAddDummyCardButton.SetActive(true);
			debugStartCombatButton.SetActive(false);
			debugEndCombatButton.SetActive(true);

			EndPlayerTurnButtonObj.SetActive(true);
			DebugEndTurnButtonObj.SetActive(true);
		}
		private void ShowGameLossUi()
		{
			GameLostUi.SetActive(true);
		}

		//BUTTON CALLS
		public void QuitToMainMenuOnLoss()
		{
			//for now just show map again
			GameManager.EnterMapView();
		}
		public void SkipPlayerTurn()
		{
			if (TurnOrderManager.CurrentEntitiesTurn() == TurnOrderManager.Player())
				TurnOrderManager.SkipCurrentEntitiesTurn();
		}

		//debug funcs for buttons
		public void DebugStartCombat()
		{
			if (listOfEntitiesToFight.Count == 0)
			{
				Debug.LogError("add entity data to list of entities to fight in inspector");
				return;
			}
			GameManager.DebugStartCardCombatGameState();
		}
		public void DebugEndCombat()
		{
			GameManager.EnterCardCombatWin();
		}
		public void DebugSkipTurn()
		{
			TurnOrderManager.SkipCurrentEntitiesTurn();
		}
		public void DebugAddDummyCard()
		{
			List<StatusEffectsData> debugDummyCards = new()
			{
				GameManager.instance.statusEffectsDataTypes[UnityEngine.Random.Range(0, GameManager.instance.statusEffectsDataTypes.Count)]
			};

			PlayerCardDeckHandler.AddDummyCards(debugDummyCards);
		}
	}
}
