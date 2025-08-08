using System;
using UnityEngine;
using UnityEngine.EventSystems;
using static Woopsious.DamageData;

namespace Woopsious
{
	public class CardHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		[HideInInspector] public Entity cardOwner;
		CardUi card;

		[HideInInspector] public bool isBeingDragged;
		private PlayerEntity touchingPlayerRef;
		private Entity touchingEnemyRef;

		Vector3 mousePos;

		public static event Action<CardUi> OnCardUsed;
		public static event Action<CardUi> OnPlayerPickedUpCard;
		public static event Action<bool> OnPlayerUseCard;

		void Awake()
		{
			card = GetComponent<CardUi>();
			isBeingDragged = false;
			touchingPlayerRef = null;
			touchingEnemyRef = null;
		}
		void Update()
		{
			if (isBeingDragged)
				FollowMouseCursor();
		}

		void OnTriggerEnter2D(Collider2D other)
		{
			if (other.GetComponent<Entity>() != null)
				EntityTriggerEnter(other.GetComponent<Entity>());
			else if (other.GetComponent<CardDeckUi>() != null)
				CardDeckTriggerEnter();
		}
		void OnTriggerExit2D(Collider2D other)
		{
			if (other.GetComponent<Entity>() != null)
				EntityTriggerExit(other.GetComponent<Entity>());
			else if (other.GetComponent<CardDeckUi>() != null)
				CardDeckTriggerExit();
		}

		//trigger enter/exit funcs
		void EntityTriggerEnter(Entity entity)
		{
			if (entity.EntityData.isPlayer)
				touchingPlayerRef = (PlayerEntity)entity;
			else
				touchingEnemyRef = entity;
		}
		void EntityTriggerExit(Entity entity)
		{
			if (entity.EntityData.isPlayer)
				touchingPlayerRef = null;
			else
				touchingEnemyRef = null;
		}

		void CardDeckTriggerEnter()
		{
			transform.localScale = new Vector2(1, 1);
		}
		void CardDeckTriggerExit()
		{
			transform.localScale = new Vector2(0.5f, 0.5f);
		}

		//player mouse events
		void FollowMouseCursor()
		{
			mousePos = Input.mousePosition;
			transform.position = new Vector3(mousePos.x, mousePos.y, 0);
		}

		//player card actions
		public void OnPointerDown(PointerEventData eventData)
		{
			if (!card.selectable) return;
			PlayerSelectCard();
		}
		public void OnPointerUp(PointerEventData eventData)
		{
			if (!card.selectable) return;
			PlayerDeselectCard();
		}

		void PlayerSelectCard()
		{
			OnPlayerPickedUpCard?.Invoke(card);
			isBeingDragged = true;
			touchingPlayerRef = null;
			touchingEnemyRef = null;

			cardOwner = TurnOrderManager.Player();
			transform.SetParent(CardCombatUi.instance.DraggedCardsArea);
			transform.rotation = new Quaternion(0, 0, 0, 0);
			mousePos = Input.mousePosition;

			card.replaceCardButton.gameObject.SetActive(false);
		}
		void PlayerDeselectCard()
		{
			OnPlayerPickedUpCard?.Invoke(null);
			isBeingDragged = false;

			if (touchingPlayerRef != null)
				UseCardOnTarget(cardOwner);
			else if (touchingEnemyRef != null)
				UseCardOnTarget(touchingEnemyRef);
			else
				CardDeckUi.instance.ReturnCardToPlayerDeck(card);

			touchingPlayerRef = null;
			touchingEnemyRef = null;
		}

		//enemy card actions
		public void EnemyUseCard(Entity entity, Entity target)
		{
			cardOwner = entity;
			UseCardOnTarget(target);
		}

		//shared actions
		void UseCardOnTarget(Entity target)
		{
			target.OnHit(new(cardOwner, false, card.Damage, card.DamageType));

			if (card.AlsoHeals)
				cardOwner.OnHit(new(cardOwner, false, card.Damage, DamageType.heal));

			CleanUpCard();
		}
		void CleanUpCard()
		{
			if (card.PlayerCard)
			{
				PlayerEntity player = cardOwner as PlayerEntity;
				OnPlayerUseCard?.Invoke(card.Offensive);
			}
			else
				cardOwner.EndTurn();

			gameObject.SetActive(false);
			OnCardUsed?.Invoke(card);
		}
	}
}