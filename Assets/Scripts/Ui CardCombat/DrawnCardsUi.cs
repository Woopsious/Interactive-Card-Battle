using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Woopsious
{
	public class DrawnCardsUi : MonoBehaviour
	{
		public static DrawnCardsUi instance;

		RectTransform drawnCardsTransform;
		RectTransform drawnCardsBackgroundTransform;
		Image imageHighlight;
		Color _ColourGrey = new(0.3921569f, 0.3921569f, 0.3921569f, 1);
		Color _ColourIceBlue = new(0, 1, 1, 1);
		Color _ColourYellow = new(0.7843137f, 0.6862745f, 0, 1);

		public DrawnCardSlotUi[] cardSlots = new DrawnCardSlotUi[9];
		public List<DrawnCardSlotUi> activeCardSlots = new();

		public Transform movingCardsTransform;
		bool playerPickedUpCard;

		[Header("audio")]
		AudioHandler audioHandler;
		public AudioClip cardAudioSfx;

		void Awake()
		{
			instance = this;
			drawnCardsTransform = GetComponent<RectTransform>();
			drawnCardsBackgroundTransform = drawnCardsTransform.GetChild(0).GetComponentInChildren<RectTransform>();
			imageHighlight = GetComponent<Image>();
			audioHandler = GetComponent<AudioHandler>();
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

		//UI
		//show/hide deck + all cards
		void OnNewTurnStart(Entity entity)
		{
			if (entity.EntityData.isPlayer)
				ShowCardDeck(entity as PlayerEntity);
			else
				HideCardDeck();
		}
		void ShowCardDeck(PlayerEntity playerEntity)
		{
			imageHighlight.color = _ColourGrey;
			audioHandler.PlayAudio(cardAudioSfx, true);

			if (activeCardSlots.Count == playerEntity.cardDrawAmount.Value) return;

			SetupDynamicCardSlots((int)playerEntity.cardDrawAmount.Value);
		}
		void SetupDynamicCardSlots(int slotsToSetup)
		{
			activeCardSlots.Clear();

			float spacingX = 150;
			drawnCardsTransform.sizeDelta = new Vector2((spacingX * slotsToSetup) + 50, 125);
			drawnCardsBackgroundTransform.sizeDelta = new Vector2((spacingX * slotsToSetup) + 40, 120);

			for (int i = 0; i < cardSlots.Length; i++)
			{
				DrawnCardSlotUi cardSlotUi = cardSlots[i];
				if (i < slotsToSetup)
				{
					RectTransformData data;

					if (i == 0)
						data = new RectTransformData(SetInitialSlotPosition(spacingX, i, -138), new Vector3(0, 0, 15));
					else if (i == 1)
						data = new RectTransformData(SetInitialSlotPosition(spacingX, i, -96), new Vector3(0, 0, 7.5f));
					else if (i == slotsToSetup - 2)
						data = new RectTransformData(SetInitialSlotPosition(spacingX, i, -75), new Vector3(0, 0, -7.5f));
					else if (i == slotsToSetup - 1)
						data = new RectTransformData(SetInitialSlotPosition(spacingX, i, -96), new Vector3(0, 0, -15));
					else
						data = new RectTransformData(SetInitialSlotPosition(spacingX, i, -75), new Vector3(0, 0, 0));

					activeCardSlots.Add(cardSlotUi);
					cardSlotUi.SetSlotRectTransforms(data);
					cardSlotUi.gameObject.SetActive(true);
				}
				else
					cardSlotUi.gameObject.SetActive(false);
			}
		}
		//adjust map node positions
		Vector2 SetInitialSlotPosition(float spacingX, int i, int posY)
		{
			return  new Vector2(spacingX * i, posY);
		}
		void HideCardDeck()
		{
			imageHighlight.color = _ColourGrey;
		}

		//update image border highlight
		void OnCardPicked(CardUi card)
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
