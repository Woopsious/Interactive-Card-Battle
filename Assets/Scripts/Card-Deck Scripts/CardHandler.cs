using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using Woopsious.AbilitySystem;
using static Woopsious.DamageData;

namespace Woopsious
{
	public class CardHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		[HideInInspector] public Entity cardOwner;
		public CardUi Ui { get; private set; }

		[HideInInspector] public bool isBeingDragged;
		[HideInInspector] public bool isBeingDiscarded;
		private PlayerEntity touchingPlayerRef;
		private Entity touchingEnemyRef;

		Vector3 mousePos;

		public AttackData AttackData { get; private set; }
		public DamageData DamageData { get; private set; }
		public bool PlayerCard { get; private set; }
		public bool DummyCard { get; private set; }
		public bool Offensive { get; private set; }
		public bool Selectable { get; private set; }

		public static event Action<CardHandler, bool> OnPlayerCardUsed;
		public static event Action<CardHandler> OnPlayerPickedUpCard;
		public static event Action<CardHandler> OnCardCleanUp;
		public static event Action<CardHandler> OnCardReplace;

		void Awake()
		{
			Ui = GetComponent<CardUi>();
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
			else if (other.GetComponent<DrawnCardsUi>() != null)
				DrawnCardsDeckTriggerEnter();
			else if (other.GetComponent<ReplaceCardUi>() != null)
				ReplaceCardTriggerEnter();
		}
		void OnTriggerExit2D(Collider2D other)
		{
			if (other.GetComponent<Entity>() != null)
				EntityTriggerExit(other.GetComponent<Entity>());
			else if (other.GetComponent<DrawnCardsUi>() != null)
				DrawnCardsDeckTriggerExit();
			else if (other.GetComponent<ReplaceCardUi>() != null)
				ReplaceCardTriggerExit();
		}

		bool AttackDataNullCheck(AttackData attackData)
		{
			if (attackData == null)
			{
				Debug.LogError("Attack data null");
				return true;
			}
			else
				return false;
		}

		//card initilization
		public void SetupCard(Entity cardOwner, AttackData attackData, bool playerCard, bool colliderActive)
		{
			if (AttackDataNullCheck(attackData))
				return;

			GetComponent<BoxCollider2D>().enabled = colliderActive;
			AttackData = attackData;
			PlayerCard = playerCard;
			DummyCard = false;
			Offensive = attackData.offensive;

			if (cardOwner != null) //apply entity modifiers
			{
				DamageData = new(cardOwner, attackData.DamageData);
				DamageData.DamageValue = (int)(DamageData.DamageValue + cardOwner.damageBonus.value);
				DamageData.DamageValue = (int)(DamageData.DamageValue * cardOwner.damageDealtModifier.value);
			}
			else
				DamageData = new(null, attackData.DamageData);
		}
		public void SetupCard(AttackData attackData)
		{
			if (AttackDataNullCheck(attackData))
				return;

			GetComponent<BoxCollider2D>().enabled = false;
			AttackData = attackData;
			PlayerCard = false;
			DummyCard = false;
			Offensive = attackData.offensive;

			if (cardOwner != null) //apply entity modifiers
			{
				DamageData = new(cardOwner, attackData.DamageData);
				DamageData.DamageValue = (int)(DamageData.DamageValue + cardOwner.damageBonus.value);
				DamageData.DamageValue = (int)(DamageData.DamageValue * cardOwner.damageDealtModifier.value);
			}
			else
				DamageData = new(null, attackData.DamageData);
		}
		public void UpdateCard(Entity cardOwner, bool playerCard)
		{
			PlayerCard = playerCard;
			DummyCard = false;

			DamageData = new(cardOwner, AttackData.DamageData);
			DamageData.DamageValue = (int)(DamageData.DamageValue + cardOwner.damageBonus.value); //apply bonus damage
			DamageData.DamageValue = (int)(DamageData.DamageValue * cardOwner.damageDealtModifier.value); //apply damage dealt modifier
		}

		//special dummy card initilization
		public void SetupDummyCard(StatusEffectsData dummyCardEffectData)
		{
			PlayerCard = false;
			DummyCard = true;

			string cardName = dummyCardEffectData.effectName;
			gameObject.name = cardName;
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

		void DrawnCardsDeckTriggerEnter()
		{
			transform.localScale = new Vector2(1, 1);
		}
		void DrawnCardsDeckTriggerExit()
		{
			transform.localScale = new Vector2(0.5f, 0.5f);
		}

		void ReplaceCardTriggerEnter()
		{
			isBeingDiscarded = true;
		}
		void ReplaceCardTriggerExit()
		{
			isBeingDiscarded = false;
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
			if (!Selectable || eventData.button != PointerEventData.InputButton.Left) return;
			PlayerSelectCard();
		}
		public void OnPointerUp(PointerEventData eventData)
		{
			if (!Selectable || eventData.button != PointerEventData.InputButton.Left) return;
			PlayerDeselectCard();
		}

		void PlayerSelectCard()
		{
			OnPlayerPickedUpCard?.Invoke(this);
			isBeingDragged = true;
			touchingPlayerRef = null;
			touchingEnemyRef = null;

			cardOwner = TurnOrderManager.Player();
			transform.SetParent(CardCombatUi.instance.DraggedCardsTransform);
			transform.rotation = new Quaternion(0, 0, 0, 0);
			mousePos = Input.mousePosition;
		}
		void PlayerDeselectCard()
		{
			if (isBeingDiscarded)
				OnCardReplace?.Invoke(this);
			else if (touchingPlayerRef != null && !Offensive)
				UseCardOnTarget(cardOwner);
			else if (touchingEnemyRef != null && Offensive)
				UseCardOnTarget(touchingEnemyRef);
			else
				OnPlayerCardUsed?.Invoke(this, false);

			OnPlayerPickedUpCard?.Invoke(null);
			isBeingDragged = false;
			isBeingDiscarded = false;
			touchingPlayerRef = null;
			touchingEnemyRef = null;
		}

		//card selectable by player
		public void CardSelectable()
		{
			if (!PlayerCard)
				Selectable = false;
			else
				Selectable = true;
		}
		public void CardUnselectable()
		{
			Selectable = false;
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
			if (DamageData.valueTypes == ValueTypes.none)
				Debug.LogError("Value type not set");

			ApplyDamageAndEffectsToTarget(target);
			ApplyBlockHealAndEffectsToCardOwner();
			AddDummyCardsIfExists();

			CleanUpCard();
		}

		//handle applying value types and status effects to correct entities
		void ApplyDamageAndEffectsToTarget(Entity target)
		{
			if (DamageData.valueTypes.HasFlag(ValueTypes.damages))
			{
				if (DamageData.isMultiHitAttack &&DamageData.HitsDifferentTargets)
				{
					DamageData.DamageValue = DamageData.DamageValue / DamageData.multiHitCount; //split damage for multiple targets

					foreach (Entity aoeTarget in GetAoeTargets(target))
					{
						aoeTarget.RecieveDamage(DamageData);
						aoeTarget.StatusEffectsHandler.AddStatusEffects(DamageData.statusEffectsForTarget);
					}
				}
				else
				{
					target.RecieveDamage(DamageData);
					target.StatusEffectsHandler.AddStatusEffects(DamageData.statusEffectsForTarget);
				}
			}
		}
		List<Entity> GetAoeTargets(Entity initialTarget)
		{
			List<Entity> targets = new() { initialTarget };
			int targetsToFind = DamageData.multiHitCount - 1;

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
			if (DamageData.valueTypes.HasFlag(ValueTypes.blocks))
				cardOwner.RecieveBlock(DamageData);

			if (DamageData.valueTypes.HasFlag(ValueTypes.heals))
				cardOwner.RecieveHealing(DamageData);

			cardOwner.StatusEffectsHandler.AddStatusEffects(DamageData.statusEffectsForSelf);
		}
		void AddDummyCardsIfExists()
		{
			if (!AttackData.addDummyCardsForEffects) return;
			PlayerCardDeckHandler.AddDummyCards(AttackData.effectDummyCards);
		}

		//clean up card
		void CleanUpCard()
		{
			if (PlayerCard)
				OnPlayerCardUsed?.Invoke(this, true);
			else
				cardOwner.EndTurn();

			gameObject.SetActive(false);
			OnCardCleanUp?.Invoke(this);
		}
	}
}