using UnityEngine;

namespace Woopsious
{
	public class CardSlotUi : MonoBehaviour
	{
		RectTransform rectTransform;

		bool damageCardsHidden;
		bool nonDamageCardsHidden;
		bool hideReplaceCardsButton;

		public int cardSlotIndex;

		public CardUi CardInSlot { get; private set; }

		void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
			cardSlotIndex = gameObject.transform.GetSiblingIndex() - 1; //-1 due to background element
		}

		private void OnEnable()
		{
			TurnOrderManager.OnNewTurnEvent += OnNewTurnStart;
			PlayerEntity.HideOffensiveCards += HideMatchingCards;
			PlayerEntity.HideReplaceCardsButton += HideReplaceCardsButton;
			CardUi.OnCardReplace += ReplaceCardInDeck;
		}
		private void OnDestroy()
		{
			TurnOrderManager.OnNewTurnEvent -= OnNewTurnStart;
			PlayerEntity.HideOffensiveCards -= HideMatchingCards;
			PlayerEntity.HideReplaceCardsButton -= HideReplaceCardsButton;
			CardUi.OnCardReplace -= ReplaceCardInDeck;
		}

		//card spawning
		void OnNewTurnStart(Entity entity)
		{
			if (entity.entityData.isPlayer)
			{
				damageCardsHidden = false;
				nonDamageCardsHidden = false;
				hideReplaceCardsButton = false;

				if (CardInSlot == null)
					SpawnNewPlayerCard();
				ShowCardInSlot();
			}
			else
				HideCardInSlot();
		}
		void SpawnNewPlayerCard()
		{
			CardUi card = SpawnManager.SpawnCard();
			card.SetupCard(SpawnManager.GetRandomCard(TurnOrderManager.Player().entityData.cards));
			AddCardToSlot(card);
		}
		public void AddCardToSlot(CardUi newCard)
		{
			CardInSlot = newCard;
			CardInSlot.transform.SetParent(gameObject.transform, false);
			CardInSlot.transform.localPosition = Vector3.zero;

			if (CardInSlot.Offensive && damageCardsHidden || !CardInSlot.Offensive && nonDamageCardsHidden)
				HideCardInSlot();
			else
				ShowCardInSlot();
		}

		//events
		void HideMatchingCards(bool hideDamageCards)
		{
			if (hideDamageCards)
				damageCardsHidden = true;
			else
				nonDamageCardsHidden = true;

			if (CardInSlot == null) return;

			if (CardInSlot.Offensive && damageCardsHidden || !CardInSlot.Offensive && nonDamageCardsHidden)
				HideCardInSlot();
		}
		void HideReplaceCardsButton()
		{
			hideReplaceCardsButton = true;

			if (CardInSlot == null) return;

			CardInSlot.replaceCardButton.gameObject.SetActive(false);
		}

		//card replacing
		void ReplaceCardInDeck(CardUi cardToReplace)
		{
			if (CardInSlot != cardToReplace) return;

			CardInSlot.SetupCard(SpawnManager.GetRandomCard(TurnOrderManager.Player().entityData.cards));
			AddCardToSlot(CardInSlot);
		}

		//show/hide cards
		void ShowCardInSlot()
		{
			if (CardInSlot == null) return;

			CardInSlot.selectable = true;
			CardInSlot.replaceCardButton.anchoredPosition = new Vector2(0, 5);

			if (hideReplaceCardsButton)
				CardInSlot.replaceCardButton.gameObject.SetActive(false);
			else
				CardInSlot.replaceCardButton.gameObject.SetActive(true);

			UpdateCardSlotPositions(false);
		}
		void HideCardInSlot()
		{
			if (CardInSlot == null) return;

			CardInSlot.selectable = false;
			CardInSlot.replaceCardButton.anchoredPosition = new Vector2(0, 230);
			CardInSlot.replaceCardButton.gameObject.SetActive(false);
			UpdateCardSlotPositions(true);
		}

		void UpdateCardSlotPositions(bool hideSlot)
		{
			if (hideSlot)
			{
				rectTransform.localRotation = Quaternion.Euler(0, 0, 0);

				switch (cardSlotIndex)
				{
					case 0:
					rectTransform.anchoredPosition = new Vector2(-300, -50);
					break;
					case 1:
					rectTransform.anchoredPosition = new Vector2(-150, -50);
					break;
					case 2:
					rectTransform.anchoredPosition = new Vector2(0, -50);
					break;
					case 3:
					rectTransform.anchoredPosition = new Vector2(150, -50);
					break;
					case 4:
					rectTransform.anchoredPosition = new Vector2(300, -50);
					break;
				}
			}
			else
			{
				switch (cardSlotIndex)
				{
					case 0:
					rectTransform.anchoredPosition = new Vector2(-300, 31);
					rectTransform.localRotation = Quaternion.Euler(0, 0, 20);
					break;
					case 1:
					rectTransform.anchoredPosition = new Vector2(-150, 66);
					rectTransform.localRotation = Quaternion.Euler(0, 0, 8);
					break;
					case 2:
					rectTransform.anchoredPosition = new Vector2(0, 75);
					rectTransform.localRotation = Quaternion.Euler(0, 0, 0);
					break;
					case 3:
					rectTransform.anchoredPosition = new Vector2(150, 66);
					rectTransform.localRotation = Quaternion.Euler(0, 0, -8);
					break;
					case 4:
					rectTransform.anchoredPosition = new Vector2(300, 31);
					rectTransform.localRotation = Quaternion.Euler(0, 0, -20);
					break;
				}
			}
		}
	}
}