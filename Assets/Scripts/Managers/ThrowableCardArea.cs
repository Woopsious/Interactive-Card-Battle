using UnityEngine;

namespace Woopsious
{
	public class ThrowableCardArea : MonoBehaviour
	{
		public GameObject throwableCardAreaHighlight;
		public GameObject blockableCardAreaHighlight;

		void OnEnable()
		{
			ThrowableCard.OnCardPickUp += OnCardPickUp;
			ThrowableCard.OnEnemyThrowCard += OnEnemyCardThrown;
		}

		void OnDestroy()
		{
			ThrowableCard.OnCardPickUp -= OnCardPickUp;
			ThrowableCard.OnEnemyThrowCard -= OnEnemyCardThrown;
		}

		void OnCardPickUp(CardUi card)
		{
			if (card != null)
			{
				if (card.Offensive)
					throwableCardAreaHighlight.SetActive(true);
				else
					throwableCardAreaHighlight.SetActive(false);
			}
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
}
