using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Woopsious.AbilitySystem;

namespace Woopsious
{
	public class PlayerCardDeckUi : MonoBehaviour
	{
		public static PlayerCardDeckUi instance;

		public GameObject cardUiPrefab;

		[Header("Ui Panels")]
		public GameObject cardDeckUiPanel;
		public GameObject cardRewardsUiPanel;

		public Button cardDeckToggleButton;
		TMP_Text cardDeckToggleButtonText;

		[Header("Card Add Rewards Ui Elements")]
		public TMP_Text cardRewardsText;
		public TMP_Text selectedCardRewardsInfo;
		public GameObject cardRewardsParent;
		public Button acceptCardRewardsButton;
		public Button confirmAcceptCardRewardsButton;

		public List<CardHandler> cardRewardsSelection = new();

		[Header("Card Discard Ui Elements")]
		public Button startDiscardCardsButton;
		public Button CompleteDiscardCardsButton;
		public Button CancelDiscardCardsButton;

		[Header("Cards To Display deck data")]
		public List<CardHandler> cardsForDisplay = new();

		private void Awake()
		{
			instance = this;
			SetupButtons();
			cardDeckUiPanel.SetActive(false);
			cardRewardsUiPanel.SetActive(false);
			ToggleDiscardCardButtons(false);
		}
		private void SetupButtons()
		{
			acceptCardRewardsButton.onClick.AddListener(() => AcceptRewards());
			confirmAcceptCardRewardsButton.onClick.AddListener(() => ConfirmAcceptRewards());

			cardDeckToggleButtonText = cardDeckToggleButton.GetComponentInChildren<TMP_Text>();
			cardDeckToggleButton.onClick.AddListener(() => ShowHideCardDeckUi());
			cardDeckToggleButtonText.text = "Show Card Deck";

			startDiscardCardsButton.onClick.AddListener(() => StartCardDiscard());
			CompleteDiscardCardsButton.onClick.AddListener(() => CompleteCardDiscard());
			CancelDiscardCardsButton.onClick.AddListener(() => CancelCardDiscard());
		}

		private void OnEnable()
		{
			GameManager.OnEndCardCombatEvent += ShowCardRewardsUi;
		}
		private void OnDisable()
		{
			GameManager.OnEndCardCombatEvent -= ShowCardRewardsUi;
		}

		private void ShowHideCardDeckUi()
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
		private void UpdateCardUisInDisplay()
		{
			//create new starting deck info
			Dictionary<AttackData, int> cardDeckCount = new();

			foreach (AttackData cardAttackData in PlayerCardDeckHandler.Instance.PlayerCardDeck)
			{
				if (cardDeckCount.ContainsKey(cardAttackData))
					cardDeckCount[cardAttackData]++;
				else
					cardDeckCount.Add(cardAttackData, 1);
			}

			int index = 0;
			for (int i = 0; i < cardsForDisplay.Count; i++)
			{
				CardHandler card = cardsForDisplay[i];

				if (i < cardDeckCount.Count)
				{
					card.gameObject.SetActive(true);
				}
				else
				{
					card.gameObject.SetActive(false);
				}
			}

			foreach (KeyValuePair<AttackData, int> entry in cardDeckCount)
			{
				if (index > cardsForDisplay.Count)
					Debug.LogError("cards to set up greater than ui slots avalable to display info");

				cardsForDisplay[index].SetupCard(null, entry.Key, false, false);
				cardsForDisplay[index].Ui.SetupCardDeckCardUi(entry.Key, entry.Value);
				cardsForDisplay[index].gameObject.SetActive(true);
				index++;
			}

			// Hide the rest of the UIs if there are more than cardDeckCount
			for (int i = index; i < cardsForDisplay.Count; i++)
			{
				cardsForDisplay[i].gameObject.SetActive(false);
			}
		}

		//card rewards ui
		private void ShowCardRewardsUi(bool playerWins)
		{
			if (!playerWins) return; //nothing to do

			SetUpNewCardRewardsUi();
			cardRewardsUiPanel.SetActive(true);
			confirmAcceptCardRewardsButton.gameObject.SetActive(false);
			UpdateCardsSelectedForRewardsCount(0);
		}
		private void HideCardRewardsUi()
		{
			cardRewardsUiPanel.SetActive(false);
			confirmAcceptCardRewardsButton.gameObject.SetActive(false);
		}

		//card rewards ui set up
		private void SetUpNewCardRewardsUi()
		{
			MapNode mapNode = GameManager.CurrentlyVisitedMapNode; //generate card rewards here
			int cardChoiceCount = mapNode.cardRewardChoiceCount;
			int cardRewardSelectionLimit = mapNode.cardRewardSelectionCount;

			if (cardRewardSelectionLimit == 1)
				cardRewardsText.text = $"YOU WON\nSelect {cardRewardSelectionLimit} card to add to your deck";
			else
				cardRewardsText.text = $"YOU WON\nSelect {cardRewardSelectionLimit} cards to add to your deck";

			bool evenAmountOfCards = false;
			if (cardChoiceCount % 2 == 0)
				evenAmountOfCards = true;

			cardRewardsSelection = PlayerCardDeckHandler.GenerateCardRewards(cardRewardsParent);

			for (int i = 0; i < cardChoiceCount; i++)
				SetCardPosition(cardRewardsSelection[i].GetComponent<RectTransform>(), i, cardChoiceCount, evenAmountOfCards);
		}
		private void SetCardPosition(RectTransform cardTransform, int index, int cardCount, bool evenAmountOfCards)
		{
			int spacing = 200;
			float startPos;
			float xPos;

			if (evenAmountOfCards)
				startPos = -(cardCount / 2) * spacing + 100;
			else
				startPos = -((cardCount - 1) / 2) * spacing;

			xPos = startPos + (index * spacing);

			cardTransform.anchorMin = new Vector2(0.5f, 1);
			cardTransform.anchorMax = new Vector2(0.5f, 1);
			cardTransform.pivot = new Vector2(0.5f, 1);
			cardTransform.anchoredPosition = new Vector2(xPos, -200);
		}
		public static void UpdateCardsSelectedForRewardsCount(int count)
		{
			int cardsSelectionLimit = GameManager.CurrentlyVisitedMapNode.cardRewardSelectionCount;
			instance.selectedCardRewardsInfo.text = $"Selected Cards {count}/{cardsSelectionLimit}";
		}

		//accept card rewards button calls
		private void AcceptRewards()
		{
			if (PlayerCardDeckHandler.CanSelectCardAsReward())
				confirmAcceptCardRewardsButton.gameObject.SetActive(true);
			else
				ConfirmAcceptRewards();
		}
		private void ConfirmAcceptRewards()
		{
			HideCardRewardsUi();
			PlayerCardDeckHandler.AddCardsToDeck(true);
			GameManager.ShowMap();

			for (int i = instance.cardRewardsSelection.Count - 1; i >= 0; i--)
				Destroy(instance.cardRewardsSelection[i]);
		}

		//discard card ui updates
		private void ToggleDiscardCardButtons(bool showCompleteAndCancelButtons)
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
		private void StartCardDiscard()
		{
			foreach (CardHandler card in cardsForDisplay)
				card.Ui.ToggleDiscardCardUi(true);

			ToggleDiscardCardButtons(true);
		}
		private void CompleteCardDiscard()
		{
			foreach (CardHandler card in cardsForDisplay)
				card.Ui.ToggleDiscardCardUi(false);

			PlayerCardDeckHandler.DiscardCardsFromDeck(true);

			ToggleDiscardCardButtons(false);
			UpdateCardUisInDisplay();
		}
		private void CancelCardDiscard()
		{
			foreach (CardHandler card in cardsForDisplay)
				card.Ui.ToggleDiscardCardUi(false);

			PlayerCardDeckHandler.DiscardCardsFromDeck(false);
			ToggleDiscardCardButtons(false);
		}
	}
}
