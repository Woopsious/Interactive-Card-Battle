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

		[Header("Ui Panels")]
		public GameObject cardDeckUiPanel;
		public GameObject cardRewardsUiPanel;

		public Button cardDeckToggleButton;
		TMP_Text cardDeckToggleButtonText;

		[Header("Card Add Rewards Ui Elements")]
		public TMP_Text cardRewardsText;
		public TMP_Text selectedCardRewardsInfo;
		public Button acceptCardRewardsButton;
		public Button warningAcceptCardRewardsButton;

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
		}
		public static void RemoveCardFromBeingAdded(AttackData cardData)
		{
			if (instance.cardsToAddToPlayerDeck.Contains(cardData))
				instance.cardsToAddToPlayerDeck.Remove(cardData);
			else
				Debug.LogError("Tried removing card that doesnt exist from queue of cards to be added");
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
		}
		void HideCardRewardsUi()
		{
			cardRewardsUiPanel.SetActive(false);
		}
		void GenerateCardRewards()
		{
			MapNode mapNode = GameManager.CurrentlyVisitedMapNode; //generate card rewards here
			int cardChoiceCount = mapNode.cardRewardChoiceCount;
			int cardRewardSelectionLimit = mapNode.cardRewardSelectionCount;

			cardRewardsText.text = $"Select {cardRewardSelectionLimit} cards to add to your deck";

			///<summery>
			///create dynamic ui that generates cards based on mapNode choice count + buttons to pick cards, limiting it based on mapNode selection count
			///<summery>
		}
		void AcceptRewards() //button call
		{
			///<summery>
			///only allow player to accept rewards once selection count matches list of chosen cards
			///<summery>

			HideCardRewardsUi();
		}
		public static bool CanSelectCardAsReward()
		{
			if (instance.cardsToAddToPlayerDeck.Count >= GameManager.CurrentlyVisitedMapNode.cardRewardSelectionCount)
				return false;
			else
				return true;
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
