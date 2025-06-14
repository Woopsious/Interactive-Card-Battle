using UnityEngine;

public class PlayerEntity : Entity
{
	RectTransform rectTransform;

	void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
	}

	void OnEnable()
	{
		TurnOrderManager.OnNewTurnEvent += OnNewTurnStart;
	}

	void OnDisable()
	{
		TurnOrderManager.OnNewTurnEvent -= OnNewTurnStart;
	}

	void OnNewTurnStart(Entity entity)
	{
		if (entity == this)
			rectTransform.anchoredPosition = new Vector2(0, -200);
		else
			rectTransform.anchoredPosition = new Vector2(0, -325);
	}
}
