using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Woopsious.CardHandler;

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
		public Button completeDiscardCardsButton;
		public Button cancelDiscardCardsButton;

		[Header("Player Cards Ui Info")]
		public TMP_Text uiTitleText;
		public List<CardHandler> cardsForDisplay = new();
		private List<CardUi> cardUis = new();

		private void Awake()
		{
			instance = this;
			SetupButtons();
			cardDeckUiPanel.SetActive(false);
			cardRewardsUiPanel.SetActive(false);
			ToggleDiscardCardButtons(false);

			foreach (CardHandler card in cardsForDisplay)
				cardUis.Add(card.GetComponent<CardUi>());
		}
		private void SetupButtons()
		{
			acceptCardRewardsButton.onClick.AddListener(() => AcceptRewards());
			confirmAcceptCardRewardsButton.onClick.AddListener(() => ConfirmAcceptRewards());

			cardDeckToggleButtonText = cardDeckToggleButton.GetComponentInChildren<TMP_Text>();
			cardDeckToggleButton.onClick.AddListener(() => ToggleCardDeckInfoUi());
			cardDeckToggleButtonText.text = "Show Card Deck";

			startDiscardCardsButton.onClick.AddListener(() => StartCardDiscard());
			completeDiscardCardsButton.onClick.AddListener(() => CompleteCardDiscard());
			cancelDiscardCardsButton.onClick.AddListener(() => CancelCardDiscard());
		}

		private void OnEnable()
		{
			GameManager.OnGameStateChange += OnGameStateChange;
			PlayerCardDeckHandler.OnRewardSelectionChanged += UpdateCardsSelectedForRewardsCount;
		}
		private void OnDisable()
		{
			GameManager.OnGameStateChange -= OnGameStateChange;
			PlayerCardDeckHandler.OnRewardSelectionChanged -= UpdateCardsSelectedForRewardsCount;
		}

		private void OnGameStateChange(GameManager.GameState gameState)
		{
			if (GameManager.GameState.MapView == gameState)
			{
				SwitchToCardDeckUi();
			}
			else if (GameManager.GameState.CardCombat == gameState)
			{
				SwitchToDrawPileUi();
			}
			else if (GameManager.GameState.CardCombatWin == gameState)
			{
				ShowCardRewardsUi();
			}
		}

		//multiuse card deck ui info
		private void ToggleCardDeckInfoUi()
		{
			if (GameManager.CurrentGameState == GameManager.GameState.CardCombat)
				ShowHideCardDrawPile();
			else
				ShowHideCardDeckUi();
		}
		private void ShowHideCardDeckUi()
		{
			ToggleEnableDiscardCardButtons(true);
			ToggleDiscardCardButtons(false);

			if (cardDeckUiPanel.activeInHierarchy)
			{
				cardDeckToggleButtonText.text = "Show Card Deck";
				cardDeckUiPanel.SetActive(false);
			}
			else
			{
				cardDeckToggleButtonText.text = "Hide Card Deck";
				cardDeckUiPanel.SetActive(true);
				ShowCardsInPlayerDeck();
			}
		}
		private void ShowHideCardDrawPile()
		{
			ToggleEnableDiscardCardButtons(false);

			if (cardDeckUiPanel.activeInHierarchy)
			{
				cardDeckToggleButtonText.text = "Show Card Pile";
				cardDeckUiPanel.SetActive(false);
			}
			else
			{
				cardDeckToggleButtonText.text = "Hide Card Pile";
				cardDeckUiPanel.SetActive(true);
				ShowCardsInPlayerDrawPile();
			}
		}

		private void SwitchToCardDeckUi()
		{
			uiTitleText.text = "Card Deck";
			cardDeckToggleButtonText.text = "Show Card Deck";
		}
		private void SwitchToDrawPileUi()
		{
			uiTitleText.text = "Card Draw Pile";
			cardDeckToggleButtonText.text = "Show Card Pile";
		}

		//update ui when showing cards info
		private void ShowCardsInPlayerDeck()
		{
			UpdateCardUisInDisplay(PlayerCardDeckHandler.GetPlayerCardDeck());
		}
		private void ShowCardsInPlayerDrawPile()
		{
			UpdateCardUisInDisplay(PlayerCardDeckHandler.GetPlayerDrawPile());
		}

		private void UpdateCardUisInDisplay(IReadOnlyList<AttackData> cards)
		{
			Dictionary<AttackData, int> cardDeckInfo = new();

			foreach (AttackData cardAttackData in cards)
			{
				if (!cardDeckInfo.TryAdd(cardAttackData, 1))
					cardDeckInfo[cardAttackData]++;
			}

			int index = 0;
			foreach (KeyValuePair<AttackData, int> entry in cardDeckInfo)
			{
				if (index >= cardsForDisplay.Count)
				{
					Debug.LogError("cards to set up greater than ui slots avalable to display info");
					break;
				}

				cardsForDisplay[index].SetupCard(CardInitType.Informational, null, entry.Key, false, entry.Value);
				cardsForDisplay[index].gameObject.SetActive(true);
				index++;
			}

			// Hide the rest of the card UIs 
			for (int i = index; i < cardsForDisplay.Count; i++)
				cardsForDisplay[i].gameObject.SetActive(false);
		}

		//card rewards ui
		private void ShowCardRewardsUi()
		{
			SetUpCardRewardsUi();
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
		private void SetUpCardRewardsUi()
		{
			cardRewardsSelection.Clear();
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

			SetUpCardRewards(cardChoiceCount, evenAmountOfCards);
		}
		private void SetUpCardRewards(int cardChoiceCount, bool evenAmountOfCards)
		{
			for (int i = 0; i < cardChoiceCount; i++)
			{
				CardHandler card = SpawnManager.SpawnCard();
				AttackData attackData = PlayerCardDeckHandler.GenerateCardRewardData();

				card.transform.SetParent(cardRewardsParent.transform);
				card.SetupCard(CardInitType.Reward, null, attackData, true, PlayerCardDeckHandler.CountSimilarCardsInDeck(attackData));
				SetCardPosition(card.GetComponent<RectTransform>(), i, cardChoiceCount, evenAmountOfCards);
				cardRewardsSelection.Add(card);
			}
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
		private void UpdateCardsSelectedForRewardsCount(int count)
		{
			int cardsSelectionLimit = GameManager.CurrentlyVisitedMapNode.cardRewardSelectionCount;
			selectedCardRewardsInfo.text = $"Selected Cards {count}/{cardsSelectionLimit}";
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
			GameManager.EnterMapView();

			for (int i = cardRewardsSelection.Count - 1; i >= 0; i--)
				Destroy(cardRewardsSelection[i].gameObject);
		}

		//discard card ui updates
		private void ToggleEnableDiscardCardButtons(bool enable)
		{
			startDiscardCardsButton.gameObject.SetActive(enable);
			completeDiscardCardsButton.gameObject.SetActive(enable);
			cancelDiscardCardsButton.gameObject.SetActive(enable);
		}
		private void ToggleDiscardCardButtons(bool showCompleteAndCancelButtons)
		{
			if (showCompleteAndCancelButtons)
			{
				startDiscardCardsButton.gameObject.SetActive(false);
				cancelDiscardCardsButton.gameObject.SetActive(true);
				completeDiscardCardsButton.gameObject.SetActive(true);
			}
			else
			{
				startDiscardCardsButton.gameObject.SetActive(true);
				cancelDiscardCardsButton.gameObject.SetActive(false);
				completeDiscardCardsButton.gameObject.SetActive(false);
			}
		}
		private void StartCardDiscard()
		{
			ToggleAllDiscardCardsUi(true);
			ToggleDiscardCardButtons(true);
		}
		private void CompleteCardDiscard()
		{
			ToggleAllDiscardCardsUi(false);
			PlayerCardDeckHandler.DiscardCardsFromDeck(true);

			ToggleDiscardCardButtons(false);
			ShowCardsInPlayerDeck();
		}
		private void CancelCardDiscard()
		{
			ToggleAllDiscardCardsUi(false);
			PlayerCardDeckHandler.DiscardCardsFromDeck(false);
			ToggleDiscardCardButtons(false);
		}
		private void ToggleAllDiscardCardsUi(bool active)
		{
			foreach (CardUi card in cardUis)
				card.ToggleRemoveCardUi(active);
		}
	}
}
