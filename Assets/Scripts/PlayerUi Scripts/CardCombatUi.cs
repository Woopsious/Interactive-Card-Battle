using System;
using TMPro;
using UnityEngine;

namespace Woopsious
{
	public class CardCombatUi : MonoBehaviour
	{
		public static CardCombatUi instance;

		[Header("Ui Panels")]
		public GameObject CardDeckUi;
		public GameObject PlayedCardUi;
		public GameObject MovingCardsArea;

		public TMP_Text winLooseText;
		public GameObject StartCombatButtonObj;

		public GameObject EndPlayerTurnButtonObj;
		public GameObject DebugEndTurnButtonObj;

		void Awake()
		{
			instance = this;

			CardDeckUi.SetActive(false);
			PlayedCardUi.SetActive(false);
			MovingCardsArea.SetActive(false);

			winLooseText.gameObject.SetActive(false);
			StartCombatButtonObj.SetActive(true);
			EndPlayerTurnButtonObj.SetActive(false);
			DebugEndTurnButtonObj.SetActive(false);
		}

		void OnEnable()
		{
			GameManager.OnEndCardCombatEvent += EndCardCombat;
			TurnOrderManager.OnNewTurnEvent += ShowHideEndPlayerTurnButton;
		}

		void OnDisable()
		{
			GameManager.OnEndCardCombatEvent -= EndCardCombat;
			TurnOrderManager.OnNewTurnEvent -= ShowHideEndPlayerTurnButton;
		}

		//UI UPDATES
		void ShowHideEndPlayerTurnButton(Entity entity)
		{
			if (TurnOrderManager.Player() == entity)
				EndPlayerTurnButtonObj.SetActive(true);
			else
				EndPlayerTurnButtonObj.SetActive(false);
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

			StartCombatButtonObj.SetActive(true);
			winLooseText.gameObject.SetActive(true);
		}

		//BUTTON CALLS
		public void BeginCardCombat()
		{
			CardDeckUi.SetActive(true);
			PlayedCardUi.SetActive(true);
			MovingCardsArea.SetActive(true);

			winLooseText.gameObject.SetActive(false);
			StartCombatButtonObj.SetActive(false);
			EndPlayerTurnButtonObj.SetActive(true);
			DebugEndTurnButtonObj.SetActive(true);

			GameManager.BeginCardCombat();
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
