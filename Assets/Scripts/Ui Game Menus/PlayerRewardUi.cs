using System.Collections.Generic;
using UnityEngine;
using Woopsious;

public class PlayerRewardUi : MonoBehaviour
{
	[Header("Ui Elements")]
	public GameObject rewardsUiPanel;

	public List<AttackData> cardsToAddToPlayerDeck = new();

	void OnEnable()
	{
		GameManager.OnEndCardCombatEvent += ShowRewardsUi;
	}
	void OnDisable()
	{
		GameManager.OnEndCardCombatEvent -= ShowRewardsUi;
	}

	void ShowRewardsUi(bool playerWins)
	{
		if (!playerWins) return; //nothing to do

		GenerateCardRewards();
		rewardsUiPanel.SetActive(true);
	}
	void HideRewardsUi()
	{
		rewardsUiPanel.SetActive(false);
	}

	void GenerateCardRewards()
	{
		MapNode mapNode = GameManager.CurrentlyVisitedMapNode; //generate card rewards here

		///<summery>
		///create dynamic ui that generates cards based on mapNode choice count + buttons to pick cards, limiting it based on mapNode selection count
		///<summery>
	}
	public void AcceptRewards() //button call
	{
		///<summery>
		///only allow player to accept rewards once selection count matches list of chosen cards
		///<summery>

		PlayerCardDeckUi.AddNewCardsToPlayerDeck(cardsToAddToPlayerDeck);

		HideRewardsUi();
	}
}
