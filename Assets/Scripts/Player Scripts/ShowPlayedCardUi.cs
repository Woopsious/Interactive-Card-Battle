using System;
using System.Threading.Tasks;
using UnityEngine;

public class ShowPlayedCardUi : MonoBehaviour
{
	public static ShowPlayedCardUi instance;

	public GameObject PlayedCardUi;
	public CardUi cardUi;

	void Awake()
	{
		instance = this;
	}

	void OnEnable()
	{
		Entity.OnEnemyMoveFound += ShowPlayedCard;
		Entity.OnEnemyAttack += HidePlayedCard;
		Entity.OnEnemyAttackCancel += HidePlayedCard;
	}

	void OnDisable()
	{
		Entity.OnEnemyMoveFound -= ShowPlayedCard;
		Entity.OnEnemyAttack -= HidePlayedCard;
		Entity.OnEnemyAttackCancel -= HidePlayedCard;
	}

	void ShowPlayedCard(AttackData data)
	{
		cardUi.SetupCard(data);
		PlayedCardUi.SetActive(true);
	}

	void HidePlayedCard()
	{
		PlayedCardUi.SetActive(false);
	}
}
