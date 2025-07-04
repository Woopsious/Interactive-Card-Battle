using System;
using UnityEngine;
using UnityEngine.EventSystems;
using static Woopsious.DamageData;

namespace Woopsious
{
	public class ThrowableCard : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		Bounds bounds;
		CardDeckUi cardDeckManagerUi;

		Entity cardOwner;
		CardUi card;
		Rigidbody2D rb;

		public PlayerEntity PlayerRef { get; private set; }
		bool isBeingDragged;
		bool inThrownCardsArea;
		bool wasThrown;

		Vector3 mousePos;
		Vector3 lastMousePos;
		Vector3 mouseVelocity;

		public static event Action<CardUi> OnCardPickUp;
		public static event Action<bool> OnEnemyThrowCard;

		void Awake()
		{
			card = GetComponent<CardUi>();
			rb = GetComponent<Rigidbody2D>();

			isBeingDragged = false;
			inThrownCardsArea = true;
			wasThrown = false;

			bounds = new()
			{
				center = new Vector2(Screen.width / 2, Screen.height / 2),
				extents = new Vector3((Screen.width / 2) + 100, (Screen.height / 2) + 100, 0)
			};
		}

		void Update()
		{
			if (isBeingDragged)
				FollowMouseCursor();

			if (wasThrown)
				CheckIfCardOutOfBounds();
		}

		void OnTriggerEnter2D(Collider2D other)
		{
			if (!wasThrown && other.GetComponent<PlayerEntity>() != null)
				PlayerEntityTriggerEnter(other.GetComponent<PlayerEntity>());
			else if (other.GetComponent<CardDeckUi>() != null)
				CardDeckTriggerEnter(other.GetComponent<CardDeckUi>());
			else if (other.GetComponent<ThrowableCardArea>() != null)
				ThrowableCardAreaEnter();
			else if (wasThrown && other.GetComponent<Entity>() != null)
				UseCard(other.GetComponent<Entity>());
		}
		void OnTriggerExit2D(Collider2D other)
		{
			if (!wasThrown && other.GetComponent<PlayerEntity>() != null)
				PlayerEntityTriggerExit();
			else if (other.GetComponent<CardDeckUi>() != null)
				CardDeckTriggerExit();
			else if (other.GetComponent<ThrowableCardArea>() != null)
				ThrowableCardAreaExit();
		}

		//trigger enter/exit funcs
		void PlayerEntityTriggerEnter(PlayerEntity entity)
		{
			PlayerRef = entity;
		}
		void PlayerEntityTriggerExit()
		{
			PlayerRef = null;
		}

		void CardDeckTriggerEnter(CardDeckUi cardDeckManagerUi)
		{
			if (wasThrown) return;
			this.cardDeckManagerUi = cardDeckManagerUi;
			transform.localScale = new Vector2(1, 1);
		}
		void CardDeckTriggerExit()
		{
			if (wasThrown) return;
			cardDeckManagerUi = null;
			transform.localScale = new Vector2(0.5f, 0.5f);
		}

		void ThrowableCardAreaEnter()
		{
			inThrownCardsArea = true;
		}
		void ThrowableCardAreaExit()
		{
			inThrownCardsArea = false;
		}

		//player mouse events
		void FollowMouseCursor()
		{
			mousePos = Input.mousePosition;
			mouseVelocity = mousePos - lastMousePos;
			lastMousePos = mousePos;
			transform.position = new Vector3(mousePos.x, mousePos.y, 0);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (!card.selectable) return;

			if (card.PlayerCard && wasThrown)
				return;
			else if (card.PlayerCard && !wasThrown)
				PlayerSelectCard();
			else if (!card.PlayerCard && wasThrown && inThrownCardsArea)
				BlockEnemyThrownCard();
		}
		public void OnPointerUp(PointerEventData eventData)
		{
			if (!card.selectable) return;

			PlayerDeselectCard();
		}

		//player card actions
		void PlayerSelectCard()
		{
			OnCardPickUp?.Invoke(card);
			isBeingDragged = true;

			cardOwner = TurnOrderManager.Player();
			transform.SetParent(CardDeckUi.instance.movingCardsTransform);
			transform.rotation = new Quaternion(0, 0, 0, 0);

			mousePos = Input.mousePosition;
			lastMousePos = Input.mousePosition;

			card.replaceCardButton.gameObject.SetActive(false);
		}
		void PlayerDeselectCard()
		{
			OnCardPickUp?.Invoke(null);
			isBeingDragged = false;

			if (card.Offensive)
			{
				if (inThrownCardsArea && cardDeckManagerUi == null && mouseVelocity != Vector3.zero)
					ThrowCard();
				else
					CardDeckUi.instance.AddCardToPlayerDeck(card);
			}
			else
			{
				if (PlayerRef != null)
					UseCard(PlayerRef);
				else
					CardDeckUi.instance.AddCardToPlayerDeck(card);
			}
		}
		void BlockEnemyThrownCard()
		{
			DestoryCard();
		}

		//enemy card actions
		public void EnemyThrowCard(Entity entity, Vector3 playerPosition)
		{
			OnEnemyThrowCard?.Invoke(true);
			card.selectable = true;
			inThrownCardsArea = false; //doesnt start off in area so reset to false

			//set transform data
			cardOwner = entity;
			transform.SetParent(CardDeckUi.instance.movingCardsTransform);
			transform.position = entity.transform.position;
			transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

			//reuse to throw card at player pos
			mousePos = playerPosition;
			lastMousePos = entity.transform.localPosition;
			mouseVelocity = mousePos - lastMousePos;

			ThrowCard();
		}
		public void EnemyUseCard(Entity entity)
		{
			cardOwner = entity;
			UseCard(entity);
		}

		//shared actions
		void ThrowCard()
		{
			transform.rotation = Quaternion.LookRotation(Vector3.forward, mouseVelocity);
			rb.AddForce(mouseVelocity.normalized * 500, ForceMode2D.Impulse);
			wasThrown = true;
		}
		void UseCard(Entity entity)
		{
			if (!CardCanHitEntity(entity)) return;

			if (card.AlsoHeals)
				cardOwner.OnHit(new(card.Damage, DamageType.heal));

			entity.OnHit(new(card.Damage, card.DamageType));
			DestoryCard();
		}
		bool CardCanHitEntity(Entity entity)
		{
			if (card.Offensive)
			{
				if (entity.entityData.isPlayer && card.PlayerCard)
					return false;
				else if (!entity.entityData.isPlayer && !card.PlayerCard)
					return false;
				else
					return true;
			}
			else
			{
				if (entity.entityData.isPlayer && card.PlayerCard)
					return true;
				else if (!entity.entityData.isPlayer && !card.PlayerCard)
					return true;
				else
					return false;
			}
		}

		//card removal
		void CheckIfCardOutOfBounds()
		{
			if (!bounds.Contains(transform.position))
				DestoryCard();
		}
		void DestoryCard()
		{
			if (card.PlayerCard)
			{
				PlayerEntity player = cardOwner as PlayerEntity;
				player.UpdateCardsUsed(card.Offensive);
			}
			else
			{
				cardOwner.EndTurn();
				OnEnemyThrowCard?.Invoke(false);
			}

			Destroy(gameObject);
		}

		void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(bounds.center, bounds.size);
		}
	}
}