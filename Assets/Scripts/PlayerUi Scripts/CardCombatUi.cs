using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Woopsious
{
	public class CardCombatUi : MonoBehaviour
	{
		public static CardCombatUi instance;

		[Header("Ui Panels")]
		public GameObject CardDeckUi;
		public GameObject PlayedCardUi;
		public GameObject MovingCardsArea;

		[Header("Ui Elements")]
		public TMP_Text currentRoundInfoText;
		public TMP_Text currentTurnInfoText;
		public TMP_Text winLooseText;

		public GameObject returnToMapButtonObj;
		public GameObject DebugReturnToMapButtonObj;
		public GameObject EndPlayerTurnButtonObj;
		public GameObject DebugEndTurnButtonObj;

		void Awake()
		{
			instance = this;
		}
		void OnEnable()
		{
			GameManager.OnShowMapEvent += ShowMap;
			GameManager.OnStartCardCombatEvent += StartCardCombat;
			GameManager.OnEndCardCombatEvent += EndCardCombat;
			TurnOrderManager.OnNewRoundStartEvent += OnNewRound;
			TurnOrderManager.OnNewTurnEvent += OnNewTurn;
		}
		void OnDisable()
		{
			GameManager.OnShowMapEvent -= ShowMap;
			GameManager.OnStartCardCombatEvent -= StartCardCombat;
			GameManager.OnEndCardCombatEvent -= EndCardCombat;
			TurnOrderManager.OnNewRoundStartEvent -= OnNewRound;
			TurnOrderManager.OnNewTurnEvent -= OnNewTurn;
		}

		//Event Listeners
		void OnNewRound(int currentRound)
		{
			UpdateCurrentRoundText(currentRound);
		}
		void OnNewTurn(Entity entity)
		{
			ShowHideEndPlayerTurnButton(entity);
			UpdateCurrentTurnText(entity);
		}


		//UI UPDATES
		void UpdateCurrentRoundText(int currentRound)
		{
			currentRoundInfoText.text = "Round: " + currentRound;
		}
		void UpdateCurrentTurnText(Entity entity)
		{
			currentTurnInfoText.text = entity.entityData.entityName + "'s turn";
		}
		void ShowHideEndPlayerTurnButton(Entity entity)
		{
			if (TurnOrderManager.Player() == entity)
				EndPlayerTurnButtonObj.SetActive(true);
			else
				EndPlayerTurnButtonObj.SetActive(false);
		}

		//event listeners
		void ShowMap()
		{
			CardDeckUi.SetActive(false);
			PlayedCardUi.SetActive(false);
			MovingCardsArea.SetActive(false);

			currentRoundInfoText.gameObject.SetActive(false);
			currentTurnInfoText.gameObject.SetActive(false);

			winLooseText.gameObject.SetActive(false);
			returnToMapButtonObj.SetActive(false);
			DebugReturnToMapButtonObj.SetActive(false);

			EndPlayerTurnButtonObj.SetActive(false);
			DebugEndTurnButtonObj.SetActive(false);
		}
		void StartCardCombat()
		{
			CardDeckUi.SetActive(true);
			PlayedCardUi.SetActive(true);
			MovingCardsArea.SetActive(true);

			currentRoundInfoText.gameObject.SetActive(true);
			currentTurnInfoText.gameObject.SetActive(true);

			winLooseText.gameObject.SetActive(false);
			returnToMapButtonObj.SetActive(false);
			DebugReturnToMapButtonObj.SetActive(true);

			EndPlayerTurnButtonObj.SetActive(true);
			DebugEndTurnButtonObj.SetActive(true);
		}
		void EndCardCombat(bool playerWon)
		{
			if (playerWon)
			{
				winLooseText.text = "Player Won";
			}
			else
			{
				winLooseText.text = "Player Lost";
			}

			winLooseText.gameObject.SetActive(true);
			returnToMapButtonObj.SetActive(true);
		}

		//BUTTON CALLS
		public void ReturnToMap()
		{
			GameManager.ShowMap();
		}
		public void SkipPlayerTurn()
		{
			if (TurnOrderManager.CurrentEntitiesTurn() == TurnOrderManager.Player())
				TurnOrderManager.SkipCurrentEntitiesTurn();
		}
		public void DebugSkipTurn()
		{
			TurnOrderManager.SkipCurrentEntitiesTurn();
		}
	}
}
