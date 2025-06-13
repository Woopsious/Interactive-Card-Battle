using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static GameManager instance;

	public GameObject cardPrefab;

	private void Awake()
	{
		instance = this;
	}

	//card creation
	public CardUi SpawnDamageCard(Entity entity)
	{
		CardUi card = Instantiate(cardPrefab).GetComponent<CardUi>();
		card.SetupCard(GetDamageCard(entity.entityData.cards), entity.entityData.isPlayer);
		return card;
	}
	CardData GetDamageCard(List<CardData> cardDataList)
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

	public CardUi SpawnRandomCard(Entity entity)
	{
		CardUi card = Instantiate(cardPrefab).GetComponent<CardUi>();
		card.SetupCard(GetRandomCardData(entity.entityData.cards), entity.entityData.isPlayer);
		return card;
	}
	CardData GetRandomCardData(List<CardData> cardDataList)
	{
		CardData cardData = cardDataList[Random.Range(0, cardDataList.Count)];
		return cardData;
	}
}
