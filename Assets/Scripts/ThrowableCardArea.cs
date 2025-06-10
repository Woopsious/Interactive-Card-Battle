using UnityEngine;

public class ThrowableCardArea : MonoBehaviour
{
	public GameObject throwableCardAreaHighlight;

	void OnEnable()
	{
		ThrowableCard.OnCardPickUp += OnCardPickUp;
	}

	void OnDisable()
	{
		ThrowableCard.OnCardPickUp -= OnCardPickUp;
	}

	void OnCardPickUp(bool wasPickedUp)
	{
		if (wasPickedUp)
			throwableCardAreaHighlight.SetActive(true);
		else
			throwableCardAreaHighlight.SetActive(false);
	}
}
