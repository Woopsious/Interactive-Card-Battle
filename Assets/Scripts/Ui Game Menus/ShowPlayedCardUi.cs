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

		private void Awake()
		{
			instance = this;
			PlayedCardUi.SetActive(false);
		}

		private void OnEnable()
		{
			GameManager.OnGameStateChange += OnGameStateChange;
			EntityMoves.OnEnemyMoveFound += ShowPlayedCard;
			EntityMoves.OnEnemyAttack += HidePlayedCard;
			EntityMoves.OnEnemyAttackCancel += HidePlayedCard;
		}

		private void OnDestroy()
		{
			GameManager.OnGameStateChange -= OnGameStateChange;
			EntityMoves.OnEnemyMoveFound -= ShowPlayedCard;
			EntityMoves.OnEnemyAttack -= HidePlayedCard;
			EntityMoves.OnEnemyAttackCancel -= HidePlayedCard;
		}

		private void OnGameStateChange(GameManager.GameState gameState)
		{
			if (GameManager.GameState.CardCombat == gameState || GameManager.GameState.debugCombat == gameState)
			{
				HidePlayedCard();
			}
		}

		private void ShowPlayedCard(Entity entity, AttackData data)
		{
			card.SetupCard(CardInitType.Informational, entity, data, false, 0);
			PlayedCardUi.SetActive(true);
		}
		private void HidePlayedCard()
		{
			PlayedCardUi.SetActive(false);
		}
	}
}
