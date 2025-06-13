using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	/// <summary>
	/// TODO:
	/// on players turn push forward card deck ui and player.
	/// on enemies turn push back card deck ui and player to give more room.
	/// 
	/// DISPALY OPPENENT PLAYED CARD TO PLAYER:
	/// one enemies turn pick card to use then show ui and display card chosen to player for x seconds (5s)
	/// after x seconds is up hide ui and then get enemy to throw the actual card (use Tasks and await for this)
	/// 
	/// possibly turn cards like block/heal into instant use cards instead of throwing them at urself.
	/// 
	/// add a simple brain to enemies to work out what type of card they should play.
	/// 
	/// add support to play multiple cards in a turn, eg: 1 offensive card 1 defensive card etc...
	/// 
	/// CARD DECK MANAGER REFACTOR:
	/// </summary>

	public static GameManager instance;

	public GameObject cardPrefab;

	private void Awake()
	{
		instance = this;
	}

	//card spawning
	public CardUi SpawnCard()
	{
		CardUi card = Instantiate(cardPrefab).GetComponent<CardUi>();
		return card;
	}

	//types of cards to get
	public CardData GetDamageCard(List<CardData> cardDataList)
	{
		List<CardData> offensiveCards = new();

		foreach(CardData card in cardDataList)
		{
			if (card.damageType == CardData.DamageType.physical)
				offensiveCards.Add(card);
			else
				continue;
		}

		CardData cardData = offensiveCards[Random.Range(0, offensiveCards.Count)];
		return cardData;
	}
	public CardData GetRandomCard(List<CardData> cardDataList)
	{
		CardData cardData = cardDataList[Random.Range(0, cardDataList.Count)];
		return cardData;
	}
}
