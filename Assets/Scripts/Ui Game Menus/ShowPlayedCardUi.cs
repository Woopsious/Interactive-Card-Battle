using System;
using System.Threading.Tasks;
using UnityEngine;
using static Woopsious.CardHandler;

namespace Woopsious
{
	public class ShowPlayedCardUi : MonoBehaviour
	{
		public static ShowPlayedCardUi instance;

		public GameObject PlayedCardUi;
		public CardHandler card;

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
			card.SetupCard(CardInitType.Informational, entity, data, false, 0);
			PlayedCardUi.SetActive(true);
		}
		void HidePlayedCard()
		{
			PlayedCardUi.SetActive(false);
		}
	}
}
