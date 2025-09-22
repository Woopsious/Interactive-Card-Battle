using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Woopsious
{
	public class PlayerCardDeckUi : MonoBehaviour
	{
		public static PlayerCardDeckUi instance;

		[Header("Ui Elements")]
		public GameObject cardDeckUiPanel;

		public Button cardDeckToggleButton;
		TMP_Text cardDeckToggleButtonText;

		public Button startDiscardCardsButton;
		public Button CompleteDiscardCardsButton;
		public Button CancelDiscardCardsButton;

		public List<CardUi> cardUisForDisplay = new();

		[Header("Runtime data")]
		//player card deck data
		public List<AttackData> playerCardDeck = new();
		//queued cards to be discarded
		public List<AttackData> cardsToRemoveFromPlayerDeck = new();

		private void Awake()
		{
			instance = this;
			SetupButtons();
			cardDeckUiPanel.SetActive(false);
			ToggleDiscardCardButtons(false);
		}
		private void Start()
		{
			AddNewCardsToPlayerDeck(GameManager.PlayerClass.cards);
		}
		void SetupButtons()
		{
			cardDeckToggleButtonText = cardDeckToggleButton.GetComponentInChildren<TMP_Text>();
			cardDeckToggleButton.onClick.AddListener(() => ShowHideCardDeckUi());
			cardDeckToggleButtonText.text = "Show Card Deck";

			startDiscardCardsButton.onClick.AddListener(() => StartCardDiscard());
			CompleteDiscardCardsButton.onClick.AddListener(() => CompleteCardDiscard());
			CancelDiscardCardsButton.onClick.AddListener(() => CancelCardDiscard());
		}

		void ShowHideCardDeckUi()
		{
			if (cardDeckUiPanel.activeInHierarchy)
			{
				cardDeckToggleButtonText.text = "Show Card Deck";
				cardDeckUiPanel.SetActive(false);
			}
			else
			{
				cardDeckToggleButtonText.text = "Hide Card Deck";
				cardDeckUiPanel.SetActive(true);
				UpdateCardUisInDisplay();
			}
		}

		//update ui when showing cards
		void UpdateCardUisInDisplay()
		{
			//create new starting deck info
			Dictionary<AttackData, int> cardDeckCount = new();

			foreach (AttackData cardAttackData in playerCardDeck)
			{
				if (cardDeckCount.ContainsKey(cardAttackData))
					cardDeckCount[cardAttackData]++;
				else
					cardDeckCount.Add(cardAttackData, 1);
			}

			int index = 0;
			for (int i = 0; i < cardUisForDisplay.Count; i++)
			{
				CardUi cardUi = cardUisForDisplay[i];

				if (i < cardDeckCount.Count)
				{
					cardUi.gameObject.SetActive(true);
				}
				else
				{
					cardUi.gameObject.SetActive(false);
				}
			}

			foreach (KeyValuePair<AttackData, int> entry in cardDeckCount)
			{
				if (index > cardUisForDisplay.Count)
					Debug.LogError("cards to set up greater than ui slots avalable to display info");

				cardUisForDisplay[index].SetupUiCard(entry.Key, entry.Value);
				cardUisForDisplay[index].gameObject.SetActive(true);
				index++;
			}

			// Hide the rest of the UIs if there are more than cardDeckCount
			for (int i = index; i < cardUisForDisplay.Count; i++)
			{
				cardUisForDisplay[i].gameObject.SetActive(false);
			}
		}

		//player card deck updates
		public static List<AttackData> PlayerCardsInDeck()
		{
			return instance.playerCardDeck;
		}
		public void AddNewCardsToPlayerDeck(List<AttackData> cardDatas)
		{
			for (int i = 0; i < cardDatas.Count; i++)
			{
				AttackData cardData = cardDatas[i];
				playerCardDeck.Add(cardData);
			}
		}
		public void RemoveCardsFromPlayerDeck(List<AttackData> cardDatas)
		{
			for (int i = cardDatas.Count - 1;  i >= 0; i--)
			{
				AttackData cardData = cardDatas[i];

				if (playerCardDeck.Contains(cardData))
					playerCardDeck.Remove(cardData);
				else
					Debug.LogError("Tried removing card that doesnt exist in player card deck");
			}
		}

		//queue cards to be discarded
		public static void AddCardToBeDiscarded(AttackData cardData)
		{
			instance.cardsToRemoveFromPlayerDeck.Add(cardData);
		}
		public static void RemoveCardFromBeingDiscarded(AttackData cardData)
		{
			if (instance.cardsToRemoveFromPlayerDeck.Contains(cardData))
				instance.cardsToRemoveFromPlayerDeck.Remove(cardData);
			else
				Debug.LogError("Tried removing card that doesnt exist from queue of card to be discarded");
		}

		//discard card ui + actions
		void ToggleDiscardCardButtons(bool showCompleteAndCancelButtons)
		{
			if (showCompleteAndCancelButtons)
			{
				startDiscardCardsButton.gameObject.SetActive(false);
				CancelDiscardCardsButton.gameObject.SetActive(true);
				CompleteDiscardCardsButton.gameObject.SetActive(true);
			}
			else
			{
				startDiscardCardsButton.gameObject.SetActive(true);
				CancelDiscardCardsButton.gameObject.SetActive(false);
				CompleteDiscardCardsButton.gameObject.SetActive(false);
			}
		}
		void StartCardDiscard()
		{
			foreach (CardUi cardUi in cardUisForDisplay)
				cardUi.ToggleDiscardCardUi(true);

			ToggleDiscardCardButtons(true);
		}
		void CompleteCardDiscard()
		{
			foreach (CardUi cardUi in cardUisForDisplay)
				cardUi.ToggleDiscardCardUi(false);

			RemoveCardsFromPlayerDeck(cardsToRemoveFromPlayerDeck);

			ToggleDiscardCardButtons(false);
			UpdateCardUisInDisplay();
		}
		void CancelCardDiscard()
		{
			foreach (CardUi cardUi in cardUisForDisplay)
				cardUi.ToggleDiscardCardUi(false);

			cardsToRemoveFromPlayerDeck.Clear();
			ToggleDiscardCardButtons(false);
		}
	}
}
