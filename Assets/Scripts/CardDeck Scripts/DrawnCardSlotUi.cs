using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.EventSystems;
using Woopsious.AbilitySystem;
using static Woopsious.CardHandler;
using static Woopsious.DrawnCardsUi;

namespace Woopsious
{
	public class DrawnCardSlotUi : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		RectTransform slotRectTransform;

		//slot move positions
		RectTransformData showCardRectTransform;
		RectTransformData hideCardRectTransform;
		RectTransformData mouseHoverRectTransform;

		public int cardSlotIndex;
		public CardHandler CardInSlot;
		private RectTransform CardRectTransform;

		public static event Action<DrawnCardSlotUi> OnThisSlotMouseEnter;

		void Awake()
		{
			slotRectTransform = GetComponent<RectTransform>();
			cardSlotIndex = gameObject.transform.GetSiblingIndex() - 1; //-1 due to background element

			OnThisSlotMouseEnter += OnOtherCardSlotsMouseEnters;
			PlayerEntity.OnEnergyChange += OnPlayerEnergyChanges;
			PlayerEntity.OnStatChanges += UpdateCardsOnPlayerStatChanges;
			CardHandler.OnPlayerCardUsed += OnPlayerCardUsed;
			CardHandler.OnCardReplace += ReplaceCard;

			SetSlotRectTransforms();
		}
		private void OnDestroy()
		{
			OnThisSlotMouseEnter -= OnOtherCardSlotsMouseEnters;
			PlayerEntity.OnEnergyChange -= OnPlayerEnergyChanges;
			PlayerEntity.OnStatChanges -= UpdateCardsOnPlayerStatChanges;
			CardHandler.OnPlayerCardUsed -= OnPlayerCardUsed;
			CardHandler.OnCardReplace -= ReplaceCard;
		}

		void SetSlotRectTransforms()
		{
			showCardRectTransform = new RectTransformData
			{
				anchoredPosition = slotRectTransform.anchoredPosition,
				rotation = slotRectTransform.rotation,
			};
			hideCardRectTransform = new RectTransformData
			{
				anchoredPosition = new(slotRectTransform.anchoredPosition.x, -150),
				rotation = slotRectTransform.rotation = new(0, 0, 0, 0),
			};
			mouseHoverRectTransform = new RectTransformData
			{
				anchoredPosition = new(slotRectTransform.anchoredPosition.x, 25),
				rotation = slotRectTransform.rotation,
			};

			slotRectTransform.anchoredPosition = showCardRectTransform.anchoredPosition;
			slotRectTransform.rotation = showCardRectTransform.rotation;
		}

		//card generation
		public void DrawCard()
		{
			if (CardInSlot == null)
				CardInSlot = SpawnManager.SpawnCard();

			CardInSlot.SetupCard(CardInitType.InGame, TurnOrderManager.Player(), PlayerCardDeckHandler.GenerateCardData(), true, 0);
			ReplaceCard(CardInSlot);
		}
		public void DrawDummyCard(StatusEffectsData effectData)
		{
			if (CardInSlot == null)
				CardInSlot = SpawnManager.SpawnCard();

			CardInSlot.SetupDummyCard(CardInitType.Dummy, effectData);
			ReplaceCard(CardInSlot);
		}
		void ReplaceCard(CardHandler cardToReplace)
		{
			if (CardInSlot != cardToReplace) return;
			AddCardToSlot(CardInSlot);
		}

		//move card up on mouse 'hover'
		public void OnPointerEnter(PointerEventData eventData)
		{
			if (CardInSlot == null || !CardInSlot.Selectable) return;
			MoveCardUp();
		}
		void MoveCardUp()
		{
			OnThisSlotMouseEnter?.Invoke(this);
			slotRectTransform.anchoredPosition = mouseHoverRectTransform.anchoredPosition;
			slotRectTransform.rotation = mouseHoverRectTransform.rotation;
			slotRectTransform.localScale = new Vector2(1.25f, 1.25f);
			slotRectTransform.SetSiblingIndex(slotRectTransform.parent.childCount - 1); //set as last so ui not covered
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (CardInSlot == null || !CardInSlot.Selectable) return;
			MoveCardDown();
		}
		void MoveCardDown()
		{
			slotRectTransform.anchoredPosition = showCardRectTransform.anchoredPosition;
			slotRectTransform.rotation = showCardRectTransform.rotation;
			slotRectTransform.localScale = new Vector2(1, 1);
			slotRectTransform.SetSiblingIndex(cardSlotIndex + 1); //reset to default
		}

		//card adding/removing + being used
		void OnPlayerCardUsed(CardHandler card, bool wasUsed)
		{
			if (CardInSlot != card) return;

			if (wasUsed)
				RemoveCardFromSlot(card);
			else
				AddCardToSlot(card);
		}
		void AddCardToSlot(CardHandler newCard)
		{
			CardInSlot = newCard;
			CardInSlot.gameObject.SetActive(true);
			CardRectTransform = CardInSlot.GetComponent<RectTransform>();
			CardRectTransform.SetParent(gameObject.transform, false);
			CardRectTransform.localScale = new Vector2(1, 1);
			CardRectTransform.anchoredPosition = new(0, 87.5f);

			if (CardInSlot.DummyCard)
				ShowCardInSlot();
			else
			{
				if (CardInSlot.AttackData.energyCost > TurnOrderManager.Player().energy.value)
					HideCardInSlot();
				else
					ShowCardInSlot();
			}
		}
		void RemoveCardFromSlot(CardHandler card)
		{
			if (card == CardInSlot)
				CardInSlot = null;
		}

		//events
		void OnOtherCardSlotsMouseEnters(DrawnCardSlotUi cardSlotUi)
		{
			if (cardSlotUi == this) return;
			if (CardInSlot == null) return;

			if (CardInSlot.DummyCard)
				MoveCardDown();
			else
			{
				if (CardInSlot.Selectable)
					MoveCardDown();
				else if (!CardInSlot.Selectable)
					HideCardInSlot();
			}
		}
		void UpdateCardsOnPlayerStatChanges()
		{
			if (CardInSlot == null) return;
			CardInSlot.UpdateCard(TurnOrderManager.Player(), true);
		}
		void OnPlayerEnergyChanges(int energy)
		{
			if (CardInSlot == null) return;

			if (CardInSlot.DummyCard)
				MoveCardDown();
			else
			{
				if (CardInSlot.AttackData.energyCost > energy)
					HideCardInSlot();
			}
		}

		//show/hide cards
		public void ShowCardInSlot()
		{
			if (CardInSlot == null) return;

			CardInSlot.CardSelectable();
			UpdateCardSlotPositions(true);
		}
		public void HideCardInSlot()
		{
			if (CardInSlot == null) return;

			CardInSlot.CardUnselectable();
			UpdateCardSlotPositions(false);
		}

		void UpdateCardSlotPositions(bool showSlot)
		{
			if (showSlot)
			{
				slotRectTransform.anchoredPosition = showCardRectTransform.anchoredPosition;
				slotRectTransform.localRotation = showCardRectTransform.rotation;
			}
			else
			{
				slotRectTransform.anchoredPosition = hideCardRectTransform.anchoredPosition;
				slotRectTransform.localRotation = hideCardRectTransform.rotation;
			}
		}
	}
}