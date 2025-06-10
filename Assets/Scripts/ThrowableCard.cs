using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ThrowableCard : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	CardDeckManagerUi cardDeckManagerUi;

	CardUi card;
	Rigidbody2D rb;

	bool isBeingDragged;
	bool inThrownCardsArea;
	bool wasThrown;

	Vector3 mousePos;
	Vector3 lastMousePos;
	Vector3 mouseVelocity;

	public static event Action<bool> OnCardPickUp;

	void Awake()
	{
		card = GetComponent<CardUi>();
		rb = GetComponent<Rigidbody2D>();
		isBeingDragged = false;
		inThrownCardsArea = true;
		wasThrown = false;
	}

	void Update()
	{
		if (isBeingDragged)
			FollowMouseCursor();

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
			ApplyDamage(other.GetComponent<Entity>());
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

	void ApplyDamage(Entity entity)
	{
		entity.RecieveDamage(card.Damage);
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
		if (card.PlayerCard && wasThrown)
			return;
		else if (card.PlayerCard && !wasThrown)
			PlayerSelectCard();
		else if (!card.PlayerCard && wasThrown)
			BlockEnemyThrownCard();
	}
	public void OnPointerUp(PointerEventData eventData)
	{
		PlayerDeselectCard();
	}

	//player card actions
	void PlayerSelectCard()
	{
		OnCardPickUp?.Invoke(true);

		transform.SetParent(CardDeckManagerUi.instance.movingCardsTransform);
		transform.rotation = new Quaternion(0, 0, 0, 0);

		isBeingDragged = true;
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
		rb.MoveRotation(Quaternion.LookRotation(Vector3.forward, mouseVelocity));
		rb.AddForce(mouseVelocity.normalized * 500, ForceMode2D.Impulse);
		wasThrown = true;

		CardDeckManagerUi.instance.RemoveCardFromPlayerDeck(card);
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

	}

	//enemy card actions
	public void EnemyThrowCard()
	{
		inThrownCardsArea = false; //doesnt start off in area so reset to false
	}
}
