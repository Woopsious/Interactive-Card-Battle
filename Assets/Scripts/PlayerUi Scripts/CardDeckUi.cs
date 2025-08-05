using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Woopsious
{
	public class CardDeckUi : MonoBehaviour
	{
		public static CardDeckUi instance;

		Image imageHighlight;
		Color _ColourGrey = new(0.3921569f, 0.3921569f, 0.3921569f, 1);
		Color _ColourIceBlue = new(0, 1, 1, 1);
		Color _ColourYellow = new(0.7843137f, 0.6862745f, 0, 1);

		private RectTransform cardDeckRectTransform;
		public RectTransform[] cardSlotRectTransforms = new RectTransform[5];

		public Transform movingCardsTransform;

		public List<CardUi> cards;

		bool damageCardsHidden;
		bool nonDamageCardsHidden;
		bool hideReplaceCardsButton;
		bool playerPickedUpCard;

		void Awake()
		{
			instance = this;
			cards = new List<CardUi>();
			cardDeckRectTransform = GetComponent<RectTransform>();
			imageHighlight = GetComponent<Image>();
		}

		void OnEnable()
		{
			TurnOrderManager.OnNewTurnEvent += OnNewTurnStart;
			PlayerEntity.HideOffensiveCards += HideMatchingCards;
			PlayerEntity.HideReplaceCardsButton += HideReplaceCardsButton;
			CardUi.OnCardReplace += ReplaceCardInDeck;
			CardHandler.OnCardPickUp += OnCardPicked;
		}
		void OnDestroy()
		{
			TurnOrderManager.OnNewTurnEvent -= OnNewTurnStart;
			PlayerEntity.HideOffensiveCards -= HideMatchingCards;
			PlayerEntity.HideReplaceCardsButton -= HideReplaceCardsButton;
			CardUi.OnCardReplace -= ReplaceCardInDeck;
			CardHandler.OnCardPickUp -= OnCardPicked;
		}

		void OnTriggerEnter2D(Collider2D other)
		{
			if (other.GetComponent<CardUi>() != null)
				ThrowableCardEnter();
		}
		void OnTriggerExit2D(Collider2D other)
		{
			if (other.GetComponent<CardUi>() != null)
				ThrowableCardExit();
		}

		//spawn card in player deck
		void SpawnNewPlayerCard()
		{
			CardUi card = SpawnManager.SpawnCard();
			card.SetupCard(SpawnManager.GetRandomCard(TurnOrderManager.Player().entityData.cards));
			AddCardToPlayerDeck(card);
			card.selectable = true;
		}

		//replace card
		public void ReplaceCardInDeck(CardUi cardToReplace)
		{
			cardToReplace.SetupCard(SpawnManager.GetRandomCard(TurnOrderManager.Player().entityData.cards));

			for (int i = 0; i < cards.Count; i++)
			{
				if (cards[i] != cardToReplace) continue;

				if (TurnOrderManager.CurrentEntitiesTurn() != TurnOrderManager.Player())
					HideCard(i);
				else if (damageCardsHidden && cardToReplace.Offensive)
					HideCard(i);
				else if (nonDamageCardsHidden && !cardToReplace.Offensive)
					HideCard(i);
				else
					ShowCard(i);
			}
		}

		//add/remove cards from player deck
		public void RemoveNullCardsFromPlayerDeck()
		{
			for (int i = cards.Count - 1; i >= 0; i--)
			{
				if (cards[i] == null)
					cards.RemoveAt(i);
			}
		}
		public void AddCardToPlayerDeck(CardUi card)
		{
			card.replaceCardButton.gameObject.SetActive(true);

			if (!cards.Contains(card))
				cards.Add(card);

			for (int i = 0;i < cards.Count; i++)
			{
				if (cardSlotRectTransforms[i].transform.childCount == 1) continue;
				else if (cardSlotRectTransforms[i].transform.childCount == 0)
				{
					if (card.Offensive && damageCardsHidden || !card.Offensive && nonDamageCardsHidden)
					{
						Transform cardSlotTransform = card.transform.parent;
						HideCard(cardSlotTransform.GetSiblingIndex());
					}
					else
						ShowCard(i);
					
					card.transform.SetParent(cardSlotRectTransforms[i].transform, false);
					card.transform.localPosition = Vector3.zero;
					break;
				}
				else
					Debug.LogError("child count of card slots incorrect: " + cardSlotRectTransforms[i].transform.childCount);
			}
		}

		//UI
		//show/hide deck + cards event listeners
		void OnNewTurnStart(Entity entity)
		{
			if (entity.entityData.isPlayer)
			{
				damageCardsHidden = false;
				nonDamageCardsHidden = false;
				hideReplaceCardsButton = false;

				RemoveNullCardsFromPlayerDeck();
				while (cards.Count < 5)
					SpawnNewPlayerCard();
				ShowCardDeck();
			}
			else
				HideCardDeck();
		}
		void HideMatchingCards(bool hideDamageCards)
		{
			if (hideDamageCards)
				damageCardsHidden = true;
			else
				nonDamageCardsHidden = true;

			foreach (CardUi card in cards)
			{
				if (card == null) continue;

				if (card.Offensive == hideDamageCards)
					HideCard(card.transform.parent.GetSiblingIndex() - 1);
			}
		}
		void HideReplaceCardsButton()
		{
			hideReplaceCardsButton = true;

			for (int i = 0; i < cards.Count; i++)
			{
				if (cards[i] == null) continue;
				cards[i].replaceCardButton.gameObject.SetActive(false);
			}
		}

		//update image border highlight
		void OnCardPicked(CardUi card)
		{
			if (card != null)
			{
				playerPickedUpCard = true;
				imageHighlight.color = _ColourYellow;
			}
			else
			{
				playerPickedUpCard = false;
				imageHighlight.color = _ColourGrey;
			}
		}
		void ThrowableCardEnter()
		{
			if (!playerPickedUpCard) return;
			imageHighlight.color = _ColourYellow;
		}
		void ThrowableCardExit()
		{
			if (!playerPickedUpCard) return;
			imageHighlight.color = _ColourIceBlue;
		}

		//show/hide deck + all cards
		void ShowCardDeck()
		{
			cardDeckRectTransform.anchoredPosition = Vector2.zero;
			imageHighlight.color = _ColourGrey;

			for (int i = 0; i < cardSlotRectTransforms.Length; i++)
				ShowCard(i);
		}
		void HideCardDeck()
		{
			cardDeckRectTransform.anchoredPosition = new Vector2(0, -50);
			imageHighlight.color = _ColourGrey;

			for (int i = 0; i < cardSlotRectTransforms.Length; i++)
				HideCard(i);
		}

		//show/hide individual cards
		void ShowCard(int i)
		{
			if (cards[i] != null)
			{
				cards[i].selectable = true;
				cards[i].replaceCardButton.anchoredPosition = new Vector2(0, 5);

				if (hideReplaceCardsButton)
					cards[i].replaceCardButton.gameObject.SetActive(false);
				else
					cards[i].replaceCardButton.gameObject.SetActive(true);
			}

			switch (i)
			{
				case 0:
				cardSlotRectTransforms[i].anchoredPosition = new Vector2(-300, 31);
				cardSlotRectTransforms[i].transform.localRotation = Quaternion.Euler(0, 0, 20);
				break;
				case 1:
				cardSlotRectTransforms[i].anchoredPosition = new Vector2(-150, 66);
				cardSlotRectTransforms[i].transform.localRotation = Quaternion.Euler(0, 0, 8);
				break;
				case 2:
				cardSlotRectTransforms[i].anchoredPosition = new Vector2(0, 75);
				cardSlotRectTransforms[i].transform.localRotation = Quaternion.Euler(0, 0, 0);
				break;
				case 3:
				cardSlotRectTransforms[i].anchoredPosition = new Vector2(150, 66);
				cardSlotRectTransforms[i].transform.localRotation = Quaternion.Euler(0, 0, -8);
				break;
				case 4:
				cardSlotRectTransforms[i].anchoredPosition = new Vector2(300, 31);
				cardSlotRectTransforms[i].transform.localRotation = Quaternion.Euler(0, 0, -20);
				break;
			}
		}
		void HideCard(int i)
		{
			if (cards[i] != null)
			{
				cards[i].selectable = false;
				cards[i].replaceCardButton.anchoredPosition = new Vector2(0, 230);

				if (hideReplaceCardsButton)
					cards[i].replaceCardButton.gameObject.SetActive(false);
				else
					cards[i].replaceCardButton.gameObject.SetActive(true);
			}

			cardSlotRectTransforms[i].transform.localRotation = Quaternion.Euler(0, 0, 0);

			switch (i)
			{
				case 0:
				cardSlotRectTransforms[i].anchoredPosition = new Vector2(-300, -50);
				break;
				case 1:
				cardSlotRectTransforms[i].anchoredPosition = new Vector2(-150, -50);
				break;
				case 2:
				cardSlotRectTransforms[i].anchoredPosition = new Vector2(0, -50);
				break;
				case 3:
				cardSlotRectTransforms[i].anchoredPosition = new Vector2(150, -50);
				break;
				case 4:
				cardSlotRectTransforms[i].anchoredPosition = new Vector2(300, -50);
				break;
			}
		}
	}
}
