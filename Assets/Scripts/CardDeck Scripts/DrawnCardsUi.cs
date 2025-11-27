using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Woopsious.AbilitySystem;
using Woopsious.AudioSystem;

namespace Woopsious
{
	public class DrawnCardsUi : MonoBehaviour
	{
		public static DrawnCardsUi instance;

		BoxCollider2D boxCollider2D;

		//highlights
		Image imageHighlight;
		Color _ColourGrey = new(0.3921569f, 0.3921569f, 0.3921569f, 1);
		Color _ColourIceBlue = new(0, 1, 1, 1);
		Color _ColourYellow = new(0.7843137f, 0.6862745f, 0, 1);

		//card slots + transforms
		public DrawnCardSlotUi[] cardSlots = new DrawnCardSlotUi[9];
		public List<DrawnCardSlotUi> activeCardSlots = new();

		RectTransform drawnCardsTransform;
		RectTransform drawnCardsBackgroundTransform;

		public Transform movingCardsTransform;
		bool playerPickedUpCard;

		[Header("audio")]
		AudioHandler audioHandler;
		public AudioClip cardAudioSfx;

		void Awake()
		{
			instance = this;
			boxCollider2D = GetComponent<BoxCollider2D>();
			drawnCardsTransform = GetComponent<RectTransform>();
			drawnCardsBackgroundTransform = drawnCardsTransform.GetChild(0).GetComponentInChildren<RectTransform>();
			imageHighlight = GetComponent<Image>();
			audioHandler = GetComponent<AudioHandler>();

			TurnOrderManager.OnStartTurn += UpdateDrawnCardsDeck;
			CardHandler.OnPlayerPickedUpCard += OnCardPicked;
		}
		void OnDestroy()
		{
			TurnOrderManager.OnStartTurn -= UpdateDrawnCardsDeck;
			CardHandler.OnPlayerPickedUpCard -= OnCardPicked;
		}

		void OnTriggerEnter2D(Collider2D other)
		{
			if (other.GetComponent<CardUi>() != null)
				DraggedCardEnter();
		}
		void OnTriggerExit2D(Collider2D other)
		{
			if (other.GetComponent<CardUi>() != null)
				DraggedCardExit();
		}

		//UI
		//show/hide deck + all cards
		void UpdateDrawnCardsDeck(Entity entity)
		{
			if (entity.EntityData.isPlayer)
			{
				ShowCardDeck(entity as PlayerEntity);
				DrawNewPlayerCards();
			}
			else
				HideCardDeck();
		}
		void ShowCardDeck(PlayerEntity playerEntity)
		{
			imageHighlight.color = _ColourGrey;
			audioHandler.PlayAudio(cardAudioSfx, true);

			if (activeCardSlots.Count != playerEntity.cardDrawAmount.value)
				SetupDynamicCardSlots((int)playerEntity.cardDrawAmount.value);
		}
		void SetupDynamicCardSlots(int slotsToSetup)
		{
			activeCardSlots.Clear();
			int drawnCardsWidth;

			if (slotsToSetup % 2 == 0) //if odd increase by 2 cards anyway
				drawnCardsWidth = (150 * slotsToSetup) + 150;
			else
				drawnCardsWidth = (150 * (slotsToSetup + 1)) + 150;

			boxCollider2D.size = new Vector2(drawnCardsWidth, 150);
			drawnCardsTransform.sizeDelta = new Vector2(drawnCardsWidth, 125);
			drawnCardsBackgroundTransform.sizeDelta = new Vector2(drawnCardsWidth - 10, 120);

			for (int i = 0; i < cardSlots.Length; i++)
			{
				DrawnCardSlotUi cardSlotUi = cardSlots[i];
				if (i < slotsToSetup)
				{
					activeCardSlots.Add(cardSlotUi);
					cardSlotUi.gameObject.SetActive(true);
				}
				else
					cardSlotUi.gameObject.SetActive(false);
			}
		}
		void DrawNewPlayerCards()
		{
			int dummyCardsAdded = 0;

			foreach (DrawnCardSlotUi cardSlot in activeCardSlots)
			{
				if (dummyCardsAdded < PlayerCardDeckHandler.DummyCardCount())
				{
					cardSlot.DrawDummyCard(PlayerCardDeckHandler.GetDummyCard(dummyCardsAdded));
					dummyCardsAdded++;
				}
				else
					cardSlot.DrawCard();

				cardSlot.ShowCardInSlot();
			}

			PlayerCardDeckHandler.ResetDummyCards();
		}
		void HideCardDeck()
		{
			imageHighlight.color = _ColourGrey;

			foreach (DrawnCardSlotUi cardSlot in activeCardSlots)
			{
				cardSlot.HideCardInSlot();
			}
		}

		//update image border highlight
		void OnCardPicked(CardHandler card)
		{
			audioHandler.PlayAudio(cardAudioSfx, true);

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
		void DraggedCardEnter()
		{
			if (!playerPickedUpCard) return;
			imageHighlight.color = _ColourYellow;
		}
		void DraggedCardExit()
		{
			if (!playerPickedUpCard) return;
			imageHighlight.color = _ColourIceBlue;
		}

		public struct RectTransformData
		{
			public Vector2 anchoredPosition;
			public Quaternion rotation;

			public RectTransformData(Vector2 anchoredPosition, Vector3 rotation)
			{
				this.anchoredPosition = anchoredPosition;
				this.rotation = Quaternion.Euler(rotation);
			}
		}
	}
}
