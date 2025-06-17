using System;
using TMPro;
using UnityEngine;
using static DamageData;

public class CardUi : MonoBehaviour
{
	public TMP_Text cardNametext;
	public TMP_Text cardDescriptiontext;
	public GameObject replaceCardButtonObj;

	public bool PlayerCard { get; private set; }
	public bool Offensive { get; private set; }
	public int Damage { get; private set; }
	public DamageType DamageType { get; private set; }

	public bool selectable;

	public static event Action<CardUi> OnCardReplace;

	public void SetupCard(CardData CardData)
	{
		if (CardData == null)
		{
			Debug.LogError("Card data null");
			return;
		}

		PlayerCard = true;
		Offensive = CardData.offensive;

		string cardName = CardData.cardName + UnityEngine.Random.Range(1000, 9999);
		gameObject.name = cardName;
		cardNametext.text = cardName;
		cardDescriptiontext.text = CardData.CreateDescription();
		Damage = CardData.damage;
		DamageType = CardData.damageType;

		replaceCardButtonObj.SetActive(true);
	}

	public void SetupCard(AttackData AttackData)
	{
		if (AttackData == null)
		{
			Debug.LogError("Attack data null");
			return;
		}

		PlayerCard = false;
		Offensive = AttackData.offensive;

		string cardName = AttackData.attackName + UnityEngine.Random.Range(1000, 9999);
		gameObject.name = cardName;
		cardNametext.text = cardName;
		cardDescriptiontext.text = AttackData.CreateDescription();
		Damage = AttackData.damage;
		DamageType = AttackData.damageType;

		replaceCardButtonObj.SetActive(false);
	}

	public void ReplaceCard()
	{
		OnCardReplace?.Invoke(this);
	}
}
