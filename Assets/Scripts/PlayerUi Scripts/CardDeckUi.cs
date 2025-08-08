using JetBrains.Annotations;
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
		public CardSlotUi[] cardSlots = new CardSlotUi[5];

		public Transform movingCardsTransform;
		bool playerPickedUpCard;

		void Awake()
		{
			instance = this;
			cardDeckRectTransform = GetComponent<RectTransform>();
			imageHighlight = GetComponent<Image>();
		}

		void OnEnable()
		{
			TurnOrderManager.OnNewTurnEvent += OnNewTurnStart;
			CardHandler.OnPlayerPickedUpCard += OnCardPicked;
		}
		void OnDestroy()
		{
			TurnOrderManager.OnNewTurnEvent -= OnNewTurnStart;
			CardHandler.OnPlayerPickedUpCard -= OnCardPicked;
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

		//return cards back to deck when failed to play
		public void ReturnCardToPlayerDeck(CardUi card)
		{
			foreach (CardSlotUi cardSlot in cardSlots)
			{
				if (cardSlot.CardInSlot == card)
					cardSlot.AddCardToSlot(card);
			}
		}

		//UI
		//show/hide deck + all cards
		void OnNewTurnStart(Entity entity)
		{
			if (entity.entityData.isPlayer)
				ShowCardDeck();
			else
				HideCardDeck();
		}
		void ShowCardDeck()
		{
			cardDeckRectTransform.anchoredPosition = Vector2.zero;
			imageHighlight.color = _ColourGrey;
		}
		void HideCardDeck()
		{
			cardDeckRectTransform.anchoredPosition = new Vector2(0, -50);
			imageHighlight.color = _ColourGrey;
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
	}
}
