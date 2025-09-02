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
			EntityMoves.OnEnemyMoveFound += ShowPlayedCard;
			EntityMoves.OnEnemyAttack += HidePlayedCard;
			EntityMoves.OnEnemyAttackCancel += HidePlayedCard;
		}

		void OnDestroy()
		{
			GameManager.OnStartCardCombatUiEvent -= HidePlayedCard;
			EntityMoves.OnEnemyMoveFound -= ShowPlayedCard;
			EntityMoves.OnEnemyAttack -= HidePlayedCard;
			EntityMoves.OnEnemyAttackCancel -= HidePlayedCard;
		}

		void ShowPlayedCard(Entity entity, AttackData data)
		{
			cardUi.SetupCard(entity, data, false);
			PlayedCardUi.SetActive(true);
		}
		void HidePlayedCard()
		{
			PlayedCardUi.SetActive(false);
		}
	}
}
