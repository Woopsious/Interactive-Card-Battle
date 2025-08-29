using System;
using System.Collections.Generic;
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

		public static event Action<CardUi, bool> OnPlayerCardUsed;
		public static event Action<CardUi> OnPlayerPickedUpCard;
		public static event Action<CardUi> OnCardCleanUp;

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
			if (card.DamageData.valueTypes == ValueTypes.none)
				Debug.LogError("Value type not set");

			ApplyDamageAndEffectsToTarget(target);
			ApplyBlockHealAndEffectsToCardOwner();

			CleanUpCard();
		}

		//handle applying value types and status effects to correct entities
		void ApplyDamageAndEffectsToTarget(Entity target)
		{
			if (card.DamageData.valueTypes.HasFlag(ValueTypes.dealsDamage))
			{
				if (card.DamageData.isMultiHitAttack && card.DamageData.HitsDifferentTargets)
				{
					card.DamageData.DamageValue = card.DamageData.DamageValue / card.DamageData.multiHitCount; //split damage for multiple targets

					foreach (Entity aoeTarget in GetAoeTargets(target))
					{
						aoeTarget.RecieveDamage(card.DamageData);
						aoeTarget.statusEffectsHandler.AddStatusEffects(card.DamageData.statusEffectsForTarget);
					}
				}
				else
				{
					target.RecieveDamage(card.DamageData);
					target.statusEffectsHandler.AddStatusEffects(card.DamageData.statusEffectsForTarget);
				}
			}
		}
		List<Entity> GetAoeTargets(Entity initialTarget)
		{
			List<Entity> targets = new() { initialTarget };
			int targetsToFind = card.DamageData.multiHitCount - 1;

			foreach (Entity entity in TurnOrderManager.Instance.turnOrder) //find and damage others in turn order (excluding initial + player)
			{
				if (entity.EntityData.isPlayer || targets.Contains(entity)) continue;
				if (targetsToFind <= 0) break;

				targetsToFind--;
				targets.Add(entity);
			}

			return targets;
		}
		void ApplyBlockHealAndEffectsToCardOwner()
		{
			if (card.DamageData.valueTypes.HasFlag(DamageData.ValueTypes.blocks))
				cardOwner.RecieveBlock(card.DamageData);

			if (card.DamageData.valueTypes.HasFlag(DamageData.ValueTypes.heals))
				cardOwner.RecieveHealing(card.DamageData);

			cardOwner.statusEffectsHandler.AddStatusEffects(card.DamageData.statusEffectsForSelf);
		}

		//clean up card
		void CleanUpCard()
		{
			if (card.PlayerCard)
				OnPlayerCardUsed?.Invoke(card, true);
			else
				cardOwner.EndTurn();

			gameObject.SetActive(false);
			OnCardCleanUp?.Invoke(card);
		}
	}
}