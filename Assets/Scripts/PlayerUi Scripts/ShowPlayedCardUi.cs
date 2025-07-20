using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Woopsious
{
	public class ShowPlayedCardUi : MonoBehaviour
	{
		public static ShowPlayedCardUi instance;

		public GameObject PlayedCardUi;
		public CardUi cardUi;

		void Awake()
		{
			instance = this;
			PlayedCardUi.SetActive(false);
		}

		void OnEnable()
		{
			GameManager.OnStartCardCombatUiEvent += HidePlayedCard;
			Entity.OnEnemyMoveFound += ShowPlayedCard;
			Entity.OnEnemyAttack += HidePlayedCard;
			Entity.OnEnemyAttackCancel += HidePlayedCard;
		}

		void OnDestroy()
		{
			GameManager.OnStartCardCombatUiEvent -= HidePlayedCard;
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
}
