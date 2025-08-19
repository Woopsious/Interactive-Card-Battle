using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;
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
		public static event Action<CardUi, bool> OnPlayerCardUsed;
		public static event Action<CardUi> OnPlayerPickedUpCard;

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
			transform.SetParent(CardCombatUi.instance.DraggedCardsTransform);
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
				OnPlayerCardUsed?.Invoke(card, false);

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
			if (card.DamageInfo.isAoeAttack)
				HandleCardAoeDamage(target);
			else
				HandleCardDamage(target);

			HandleCardBlock();
			HandleCardHeal();

			CleanUpCard();
		}

		//handle applying damage info values to correct entities
		void HandleCardAoeDamage(Entity initialTarget)
		{
			if (card.DamageInfo.DamageValue == 0) return;

			initialTarget.RecieveDamage(new(cardOwner, false, card.DamageInfo)); //damage initial target
			int targetsToFind = card.DamageInfo.maxAoeTargets - 1;

			foreach (Entity entity in TurnOrderManager.Instance.turnOrder) //find and damage others in turn order (excluding initial + player)
			{
				if (entity.EntityData.isPlayer || entity == initialTarget) continue;
				if (targetsToFind <= 0) break;

				targetsToFind--;
				entity.RecieveDamage(new(cardOwner, false, card.DamageInfo));
			}
		}
		void HandleCardDamage(Entity target)
		{
			if (card.DamageInfo.DamageValue == 0) return;
			target.RecieveDamage(new(cardOwner, false, card.DamageInfo));
		}
		void HandleCardBlock()
		{
			if (card.DamageInfo.BlockValue == 0) return;
			cardOwner.RecieveBlock(new(cardOwner, false, card.DamageInfo));
		}
		void HandleCardHeal()
		{
			if (card.DamageInfo.HealValue == 0) return;
			cardOwner.RecieveHealing(new(cardOwner, false, card.DamageInfo));
		}

		//clean up card
		void CleanUpCard()
		{
			if (card.PlayerCard)
				OnPlayerCardUsed?.Invoke(card, true);
			else
				cardOwner.EndTurn();

			gameObject.SetActive(false);
			OnCardUsed?.Invoke(card);
		}
	}
}