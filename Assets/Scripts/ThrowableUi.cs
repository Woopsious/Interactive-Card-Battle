using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class ThrowUiObject : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	public CardDeckManagerUi cardDeckManagerUi;

	CardUi card;
	Rigidbody2D rb;
	Camera uiCamera;

	bool isBeingDragged;

	Vector3 mousePos;
	Vector3 lastMousePos;

	Vector3 mouseVelocity;

	void Awake()
	{
		card = GetComponent<CardUi>();
		rb = GetComponent<Rigidbody2D>();
	}

	void Start()
	{
		uiCamera = GameManager.instance.uiCamera;
	}

	void Update()
	{
		if (!isBeingDragged) return;
		FollowMouseCursor();
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.GetComponent<CardDeckManagerUi>() != null)
			cardDeckManagerUi = other.GetComponent<CardDeckManagerUi>();
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if (other.GetComponent<CardDeckManagerUi>() != null)
			cardDeckManagerUi = null;
	}

	void FollowMouseCursor()
	{
		mousePos = Input.mousePosition;
		mouseVelocity = mousePos - lastMousePos;
		lastMousePos = mousePos;
		transform.position = new Vector3(mousePos.x, mousePos.y, 0);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		transform.SetParent(CardDeckManagerUi.instance.movingCardsTransform);
		transform.rotation = new Quaternion(0, 0, 0, 0);

		isBeingDragged = true;
		mousePos = Input.mousePosition;
		lastMousePos = Input.mousePosition;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		isBeingDragged = false;

		if (cardDeckManagerUi != null || mouseVelocity == Vector3.zero)
			CardDeckManagerUi.instance.AddCardToDeck(card);
		else //throw card
		{
			rb.MoveRotation(Quaternion.LookRotation(Vector3.forward, mouseVelocity));
			rb.AddForce(mouseVelocity * 10, ForceMode2D.Impulse);
		}
	}
}
