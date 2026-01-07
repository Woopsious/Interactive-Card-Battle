using UnityEngine;
using UnityEngine.UI;

namespace Woopsious
{
	public class DiscardCardUi : MonoBehaviour
	{
		Image imageHighlight;
		Color _ColourGrey = new(0.7843137f, 0.7843137f, 0.7843137f, 1);
		Color _ColourIceBlue = new(0, 1, 1, 1);
		Color _ColourYellow = new(0.7843137f, 0.6862745f, 0, 1);

		bool playerPickedUpCard;
		int cardsDiscardedThisTurn;

		void Awake()
		{
			imageHighlight = GetComponent<Image>();
			TurnOrderManager.OnStartTurn += OnPlayerTurnStart;
			CardHandler.OnPlayerPickedUpCard += OnCardPicked;
			CardHandler.OnCardDiscarded += OnCardDiscarded;
			HideUi();
		}
		void OnDestroy()
		{
			TurnOrderManager.OnStartTurn -= OnPlayerTurnStart;
			CardHandler.OnPlayerPickedUpCard -= OnCardPicked;
			CardHandler.OnCardDiscarded -= OnCardDiscarded;
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

		void OnPlayerTurnStart(Entity player)
		{
			if (!player.EntityData.isPlayer) return;
			cardsDiscardedThisTurn = 0;
		}

		void OnCardPicked(CardHandler card)
		{
			if (card != null && CanReplaceCardThisTurn())
			{
				ShowUi();
				playerPickedUpCard = true;
				imageHighlight.color = _ColourIceBlue;
			}
			else
			{
				HideUi();
				playerPickedUpCard = false;
				imageHighlight.color = _ColourGrey;
			}
		}
		bool CanReplaceCardThisTurn()
		{
			if (TurnOrderManager.Player().EntityData.maxDiscardableCardsPerTurn >= cardsDiscardedThisTurn)
				return true;
			else return false;
		}

		void OnCardDiscarded(CardHandler card)
		{
			cardsDiscardedThisTurn++;
		}

		void ShowUi()
		{
			gameObject.SetActive(true);
		}
		void HideUi()
		{
			gameObject?.SetActive(false);
		}
	}
}
