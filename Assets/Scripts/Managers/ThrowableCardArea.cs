using UnityEngine;

public class ThrowableCardArea : MonoBehaviour
{
	public GameObject throwableCardAreaHighlight;
	public GameObject blockableCardAreaHighlight;

	void OnEnable()
	{
		ThrowableCard.OnCardPickUp += OnCardPickUp;
		ThrowableCard.OnEnemyThrowCard += OnEnemyCardThrown;
	}

	void OnDisable()
	{
		ThrowableCard.OnCardPickUp -= OnCardPickUp;
		ThrowableCard.OnEnemyThrowCard -= OnEnemyCardThrown;
	}

	void OnCardPickUp(bool wasPickedUp)
	{
		if (wasPickedUp)
			throwableCardAreaHighlight.SetActive(true);
		else
			throwableCardAreaHighlight.SetActive(false);
	}

	void OnEnemyCardThrown(bool wasThrown)
	{
		if (wasThrown)
			blockableCardAreaHighlight.SetActive(true);
		else
			blockableCardAreaHighlight.SetActive(false);
	}
}
