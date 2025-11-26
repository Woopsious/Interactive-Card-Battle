using System.Collections.Generic;
using UnityEngine;
using Woopsious.AbilitySystem;
using static Woopsious.CardHandler;

namespace Woopsious
{
	public class PlayerCardDeckHandler : MonoBehaviour
	{
		public static PlayerCardDeckHandler Instance;

		[Header("Runtime data")]
		//player card deck data
		public List<AttackData> PlayerCardDeck = new();

		//queued cards to be added
		public List<AttackData> CardsToAddToDeck = new();

		//queued cards to be discarded
		public List<AttackData> CardsToDiscardFromDeck = new();

		//dummy cards list
		public List<StatusEffectsData> DummyCardsToForceAdd = new();

		//card spawn table
		public float TotalCardDropChance { get; private set; }
		public float TotalCardDrawChance { get; private set; }

		[Header("Debug Generate Card Rewards")]
		public int cardChoicesCount;
		public int cardSelectionCount;

		private void Awake()
		{
			Instance = this;
		}
		private void Start()
		{
			InitilizeStartingCardDeck();
		}

		//card deck initial creation
		private void InitilizeStartingCardDeck()
		{
			foreach (AttackData attackData in GameManager.PlayerClass.cards)
				PlayerCardDeck.Add(attackData);

			UpdateCollectableCardDropChances();
			UpdateCollectableCardDrawChances();
		}
		private void UpdateCollectableCardDropChances()
		{
			foreach (AttackData card in GameManager.PlayerClass.collectableCards)
				TotalCardDropChance += card.CardDropChance();
		}
		private void UpdateCollectableCardDrawChances()
		{
			foreach (AttackData card in PlayerCardDeck)
				TotalCardDrawChance += card.CardDrawChance();
		}

		//ADDING CARDS TO DECK
		public static void AddCardsToDeck(bool completeAdd)
		{
			if (completeAdd) //add before clearing 
			{
				for (int i = 0; i < Instance.CardsToAddToDeck.Count; i++)
				{
					AttackData cardData = Instance.CardsToAddToDeck[i];
					Instance.PlayerCardDeck.Add(cardData);
				}
			}

			Instance.CardsToAddToDeck.Clear();
		}
		public static void QueueCardToBeAdded(AttackData cardData)
		{
			Instance.CardsToAddToDeck.Add(cardData);
			PlayerCardDeckUi.UpdateCardsSelectedForRewardsCount(Instance.CardsToAddToDeck.Count);
		}
		public static void UnqueueCardFromBeingAdded(AttackData cardData)
		{
			if (Instance.CardsToAddToDeck.Contains(cardData))
				Instance.CardsToAddToDeck.Remove(cardData);
			else
				Debug.LogError("Tried removing card that doesnt exist from queue of cards to be added");

			PlayerCardDeckUi.UpdateCardsSelectedForRewardsCount(Instance.CardsToAddToDeck.Count);
		}

		//DISCARDING CARDS FROM DECK
		public static void DiscardCardsFromDeck(bool completeDiscard)
		{
			if (completeDiscard) //add before clearing 
			{
				for (int i = Instance.CardsToDiscardFromDeck.Count - 1; i >= 0; i--)
				{
					AttackData cardData = Instance.CardsToDiscardFromDeck[i];

					if (Instance.PlayerCardDeck.Contains(cardData))
						Instance.PlayerCardDeck.Remove(cardData);
					else
						Debug.LogError("Tried removing card that doesnt exist in player card deck");
				}
			}

			Instance.CardsToDiscardFromDeck.Clear();
		}
		public static void QueueCardToBeDiscarded(AttackData cardData)
		{
			Instance.CardsToDiscardFromDeck.Add(cardData);
		}
		public static void UnqueueCardFromBeingDiscarded(AttackData cardData)
		{
			if (Instance.CardsToDiscardFromDeck.Contains(cardData))
				Instance.CardsToDiscardFromDeck.Remove(cardData);
			else
				Debug.LogError("Tried removing card that doesnt exist from queue of cards to be discarded");
		}

		//DUMMY CARDS INJECTION
		public static void AddDummyCards(List<StatusEffectsData> dummmyCardsToAdd)
		{
			foreach (StatusEffectsData dummyCard in dummmyCardsToAdd)
				Instance.DummyCardsToForceAdd.Add(dummyCard);
		}
		public static void ResetDummyCards()
		{
			Instance.DummyCardsToForceAdd.Clear();
		}
		public static StatusEffectsData GetDummyCard(int i)
		{
			return Instance.DummyCardsToForceAdd[i];
		}
		public static int DummyCardCount()
		{
			return Instance.DummyCardsToForceAdd.Count;
		}

		//CARD GENERATION
		public static CardHandler GenerateCard(CardHandler card)
		{
			if (card == null)
				card = SpawnManager.SpawnCard();

			float totalCardDrawChance = Instance.TotalCardDrawChance;
			List<AttackData> playerCards = Instance.PlayerCardDeck;
			AttackData cardAttackData = SpawnManager.GetWeightedPlayerCardDraw(playerCards, totalCardDrawChance);

			card.SetupCard(CardInitType.InGame, TurnOrderManager.Player(), cardAttackData, true, 0);
			return card;
		}
		public static CardHandler GenerateDummyCard(CardHandler card, StatusEffectsData effectData)
		{
			if (card == null)
				card = SpawnManager.SpawnCard();

			card.SetupDummyCard(CardInitType.Dummy, effectData);
			return card;
		}
		public static List<CardHandler> GenerateCardRewards(GameObject parent)
		{
			MapNode mapNode = GameManager.CurrentlyVisitedMapNode; //generate card rewards here
			int cardChoiceCount = mapNode.cardRewardChoiceCount;

			List<CardHandler> cardRewards = new();

			for (int i = 0; i < cardChoiceCount; i++)
			{
				CardHandler card = SpawnManager.SpawnCard();
				AttackData cardAttackData = SpawnManager.GetWeightedPlayerCardReward(Instance.TotalCardDropChance);

				card.transform.SetParent(parent.transform);
				card.SetupCard(CardInitType.Reward, null, cardAttackData, true, CountSimilarCardsInDeck(cardAttackData));
				cardRewards.Add(card);
			}
			return cardRewards;
		}

		//CHECKS
		public static bool CanSelectCardAsReward()
		{
			if (Instance.CardsToAddToDeck.Count >= GameManager.CurrentlyVisitedMapNode.cardRewardSelectionCount)
				return false;
			else
				return true;
		}
		public static int CountSimilarCardsInDeck(AttackData attackData)
		{
			int similarCards = 0;

			foreach (AttackData cardInDeck in Instance.PlayerCardDeck)
			{
				if (cardInDeck == attackData)
					similarCards++;
			}
			return similarCards;
		}
	}
}
