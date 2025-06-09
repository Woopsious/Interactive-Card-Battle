using UnityEngine;

public class CardUi : MonoBehaviour
{
	private void Awake()
	{
		gameObject.name = "Card" + Random.Range(1000, 9999);
	}
}
