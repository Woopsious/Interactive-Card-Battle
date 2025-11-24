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

		[Header("Debug Generate Card Rewards")]
		public int cardChoicesCount;
		public int cardSelectionCount;

		[Header("Card Add Rewards Ui Elements")]
		public TMP_Text cardRewardsText;
		public TMP_Text selectedCardRewardsInfo;
		public GameObject cardRewardsParent;
		public Button acceptCardRewardsButton;
		public Button confirmAcceptCardRewardsButton;

		public List<CardUi> cardRewardsSelection = new();

		//card spawn table
		public float TotalCardDropChance { get; private set; }
		public float TotalCardDrawChance { get; private set; }

		[Header("Card Discard Ui Elements")]
		public Button startDiscardCardsButton;
		public Button CompleteDiscardCardsButton;
		public Button CancelDiscardCardsButton;

		public List<CardUi> cardUisForDisplay = new();

		[Header("Runtime data")]
		//player card deck data
		public List<AttackData> playerCardDeck = new();

		//queued cards to be added
		public List<AttackData> cardsToAddToPlayerDeck = new();

		//queued cards to be discarded
		public List<AttackData> cardsToRemoveFromPlayerDeck = new();

		//dummy cards list
		[SerializeField] private List<StatusEffectsData> dummyCardsToForceAdd = new();

		private void Awake()
		{
			instance = this;
			SetupButtons();
			cardDeckUiPanel.SetActive(false);
			cardRewardsUiPanel.SetActive(false);
			ToggleDiscardCardButtons(false);

			foreach (AttackData card in GameManager.PlayerClass.collectableCards)
			{
				TotalCardDropChance += card.CardDropChance();
				TotalCardDrawChance += card.CardDrawChance();
			}
		}
		private void Start()
		{
			AddNewCardsToPlayerDeck(GameManager.PlayerClass.cards);
		}
		void SetupButtons()
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
		public static void AddNewCardsToPlayerDeck(List<AttackData> cardDatas)
		{
			for (int i = 0; i < cardDatas.Count; i++)
			{
				AttackData cardData = cardDatas[i];
				instance.playerCardDeck.Add(cardData);
			}
		}
		public static void RemoveCardsFromPlayerDeck(List<AttackData> cardDatas)
		{
			for (int i = cardDatas.Count - 1;  i >= 0; i--)
			{
				AttackData cardData = cardDatas[i];

				if (instance.playerCardDeck.Contains(cardData))
					instance.playerCardDeck.Remove(cardData);
				else
					Debug.LogError("Tried removing card that doesnt exist in player card deck");
			}
		}

		//queue cards to be added
		public static void AddCardToBeAdded(AttackData cardData)
		{
			instance.cardsToAddToPlayerDeck.Add(cardData);
			instance.UpdateCardsSelectedForRewardsCount();
		}
		public static void RemoveCardFromBeingAdded(AttackData cardData)
		{
			if (instance.cardsToAddToPlayerDeck.Contains(cardData))
				instance.cardsToAddToPlayerDeck.Remove(cardData);
			else
				Debug.LogError("Tried removing card that doesnt exist from queue of cards to be added");

			instance.UpdateCardsSelectedForRewardsCount();
		}
		public static void CompleteCardAdd()
		{
			foreach (AttackData attackData in instance.cardsToAddToPlayerDeck)
				instance.playerCardDeck.Add(attackData);
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
				Debug.LogError("Tried removing card that doesnt exist from queue of cards to be discarded");
		}

		//card rewards ui
		void ShowCardRewardsUi(bool playerWins)
		{
			if (!playerWins) return; //nothing to do

			GenerateCardRewards();
			cardRewardsUiPanel.SetActive(true);
			confirmAcceptCardRewardsButton.gameObject.SetActive(false);
			UpdateCardsSelectedForRewardsCount();
		}
		void HideCardRewardsUi()
		{
			cardRewardsUiPanel.SetActive(false);
			confirmAcceptCardRewardsButton.gameObject.SetActive(false);
		}

		//card rewards generation
		void GenerateCardRewards()
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

			for (int i = 0; i < cardChoiceCount; i++)
				GenerateNewCardReward(i, cardChoiceCount, evenAmountOfCards);

			///<summery>
			///create dynamic ui that generates cards based on mapNode choice count + buttons to pick cards, limiting it based on mapNode selection count
			///<summery>
		}
		void GenerateNewCardReward(int index, int cardCount, bool evenAmountOfCards)
		{
			CardUi cardUi = Instantiate(cardUiPrefab).GetComponent<CardUi>();
			cardUi.transform.SetParent(cardRewardsParent.transform);
			AttackData cardAttackData = SpawnManager.GetWeightedPlayerCardReward(TotalCardDropChance);

			cardUi.SetupCardRewards(cardAttackData, CountSimilarCardsInDeck(cardAttackData));
			SetCardPosition(cardUi.GetComponent<RectTransform>(), index, cardCount,  evenAmountOfCards);
			cardRewardsSelection.Add(cardUi);
		}
		int CountSimilarCardsInDeck(AttackData attackData)
		{
			int similarCards = 0;

			foreach (AttackData cardInDeck in playerCardDeck)
			{
				if (cardInDeck == attackData)
					similarCards++;
			}
			return similarCards;
		}
		void SetCardPosition(RectTransform cardTransform, int index, int cardCount, bool evenAmountOfCards)
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
		void UpdateCardsSelectedForRewardsCount()
		{
			int cardsSelectionLimit = GameManager.CurrentlyVisitedMapNode.cardRewardSelectionCount;
			selectedCardRewardsInfo.text = $"Selected Cards {cardsToAddToPlayerDeck.Count}/{cardsSelectionLimit}";
		}

		//accept card rewards
		void AcceptRewards()
		{
			if (CanStillSelectCardRewards())
				confirmAcceptCardRewardsButton.gameObject.SetActive(true);
			else
				ConfirmAcceptRewards();
		}
		void ConfirmAcceptRewards()
		{
			HideCardRewardsUi();
			AddNewCardsToPlayerDeck(cardsToAddToPlayerDeck);
			ClearCardRewardsSelectionList();
			GameManager.ShowMap();
		}
		void ClearCardRewardsSelectionList()
		{
			for (int i = cardRewardsSelection.Count - 1; i >= 0; i--)
				Destroy(cardRewardsSelection[i].gameObject);

			cardsToAddToPlayerDeck.Clear();
			cardRewardsSelection.Clear();
		}
		public static bool CanSelectCardAsReward()
		{
			if (instance.cardsToAddToPlayerDeck.Count >= GameManager.CurrentlyVisitedMapNode.cardRewardSelectionCount)
				return false;
			else
				return true;
		}
		bool CanStillSelectCardRewards()
		{
			if (instance.cardsToAddToPlayerDeck.Count < GameManager.CurrentlyVisitedMapNode.cardRewardSelectionCount)
				return true;
			else
				return false;
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

		//cards to force add on new turns;
		public static void DebugAddDummyCard()
		{
			List<StatusEffectsData> debugDummyCards = new()
			{
				GameManager.instance.statusEffectsDataTypes[UnityEngine.Random.Range(0, GameManager.instance.statusEffectsDataTypes.Count)]
			};

			AddDummyCards(debugDummyCards);
		}
		public static void AddDummyCards(List<StatusEffectsData> dummmyCardsToAdd)
		{
			foreach (StatusEffectsData dummyCard in dummmyCardsToAdd)
				instance.dummyCardsToForceAdd.Add(dummyCard);
		}
		public static void ResetDummyCards()
		{
			instance.dummyCardsToForceAdd.Clear();
		}
		public static StatusEffectsData GetDummyCard(int i)
		{
			return instance.dummyCardsToForceAdd[i];
		}
		public static int DummyCardCount()
		{
			return instance.dummyCardsToForceAdd.Count;
		}
	}
}
