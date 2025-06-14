using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ThrowableCard : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	Bounds bounds;
	CardDeckManagerUi cardDeckManagerUi;

	Entity cardOwner;
	CardUi card;
	Rigidbody2D rb;

	bool isBeingDragged;
	bool inThrownCardsArea;
	bool wasThrown;

	Vector3 mousePos;
	Vector3 lastMousePos;
	Vector3 mouseVelocity;

	public static event Action<bool> OnCardPickUp;
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

		//if (wasThrown) disabled atm
			//card.UpdateDamageWithVelocity(rb.linearVelocity.magnitude);
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.GetComponent<CardDeckManagerUi>() != null)
			CardDeckTriggerEnter(other.GetComponent<CardDeckManagerUi>());
		else if (other.GetComponent<ThrowableCardArea>() != null)
			ThrowableCardAreaEnter();
		else if (wasThrown && other.GetComponent<Entity>() != null)
			OnEntityHit(other.GetComponent<Entity>());
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if (other.GetComponent<CardDeckManagerUi>() != null)
			CardDeckTriggerExit();
		else if (other.GetComponent<ThrowableCardArea>() != null)
			ThrowableCardAreaExit();
	}

	//trigger funcs
	void ThrowableCardAreaEnter()
	{
		inThrownCardsArea = true;
	}
	void ThrowableCardAreaExit()
	{
		inThrownCardsArea = false;
	}

	void CardDeckTriggerEnter(CardDeckManagerUi cardDeckManagerUi)
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

	void OnEntityHit(Entity entity)
	{
		if (!CardCanHitEntity(entity)) return;

		entity.OnHit(new(card.CardData, card.PlayerCard));
		DestoryCard();
	}
	bool CardCanHitEntity(Entity entity)
	{
		if (card.CardData.damageType == CardData.DamageType.block)
		{
			if (entity.entityData.isPlayer && card.PlayerCard)
				return true;
			else if (!entity.entityData.isPlayer && !card.PlayerCard)
				return true;
			else
				return false;
		}
		else if (card.CardData.damageType == CardData.DamageType.heal)
		{
			if (entity.entityData.isPlayer && card.PlayerCard)
				return true;
			else if (!entity.entityData.isPlayer && !card.PlayerCard)
				return true;
			else
				return false;
		}
		else if (card.CardData.damageType == CardData.DamageType.physical)
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
			Debug.LogError("card damage type has no match");
			return false;
		}
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
		OnCardPickUp?.Invoke(true);
		isBeingDragged = true;

		cardOwner = TurnOrderManager.Instance.playerEntity;
		transform.SetParent(CardDeckManagerUi.instance.movingCardsTransform);
		transform.rotation = new Quaternion(0, 0, 0, 0);

		mousePos = Input.mousePosition;
		lastMousePos = Input.mousePosition;
	}
	void PlayerDeselectCard()
	{
		OnCardPickUp?.Invoke(false);
		isBeingDragged = false;

		if (!inThrownCardsArea || cardDeckManagerUi != null || mouseVelocity == Vector3.zero)
			CardDeckManagerUi.instance.AddCardToPlayerDeck(card);
		else
			PlayerThrowCard();
	}
	void PlayerThrowCard()
	{
		CardDeckManagerUi.instance.RemoveCardFromPlayerDeck(card);
		transform.rotation = Quaternion.LookRotation(Vector3.forward, mouseVelocity);
		rb.AddForce(mouseVelocity.normalized * 500, ForceMode2D.Impulse);
		wasThrown = true;
	}
	//func to throw card harder or slower based on mouse velocity to increase damage (unused)
	void PlayerThrowCardBasedOnMouseMovement()
	{
		rb.MoveRotation(Quaternion.LookRotation(Vector3.forward, mouseVelocity));
		rb.AddForce(mouseVelocity * 20, ForceMode2D.Impulse);
		wasThrown = true;

		CardDeckManagerUi.instance.RemoveCardFromPlayerDeck(card);
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
		transform.SetParent(CardDeckManagerUi.instance.movingCardsTransform);
		transform.position = entity.transform.position;
		transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

		//reuse to throw card at player pos
		mousePos = playerPosition;
		lastMousePos = entity.transform.localPosition;
		mouseVelocity = mousePos - lastMousePos;

		//throw card
		transform.rotation = Quaternion.LookRotation(Vector3.forward, mouseVelocity);
		rb.AddForce(mouseVelocity.normalized * 500, ForceMode2D.Impulse);
		wasThrown = true;
	}

	//card removal
	void CheckIfCardOutOfBounds()
	{
		if (!bounds.Contains(transform.position))
			DestoryCard();
	}
	void DestoryCard()
	{
		if (!card.PlayerCard)
			OnEnemyThrowCard?.Invoke(false);

		cardOwner.EndTurn();
		Destroy(gameObject);
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(bounds.center, bounds.size);
	}
}
