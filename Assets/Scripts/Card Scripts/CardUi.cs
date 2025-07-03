using System;
using TMPro;
using UnityEngine;
using static DamageData;

public class CardUi : MonoBehaviour
{
	[HideInInspector] public ThrowableCard throwableCard;

	public TMP_Text cardNametext;
	public TMP_Text cardDescriptiontext;
	public RectTransform replaceCardButton;

	public bool PlayerCard { get; private set; }
	public bool Offensive { get; private set; }
	public bool AlsoHeals { get; private set; }
	public int Damage { get; private set; }
	public DamageType DamageType { get; private set; }

	public bool selectable;

	public static event Action<CardUi> OnCardReplace;

	void Awake()
	{
		throwableCard = GetComponent<ThrowableCard>();
	}

	public void SetupCard(CardData CardData)
	{
		if (CardData == null)
		{
			Debug.LogError("Card data null");
			return;
		}

		PlayerCard = true;
		AlsoHeals = CardData.alsoHeals;
		Offensive = CardData.offensive;

		string cardName = CardData.cardName + UnityEngine.Random.Range(1000, 9999);
		gameObject.name = cardName;
		cardNametext.text = cardName;
		cardDescriptiontext.text = CardData.CreateDescription();
		Damage = CardData.damage;
		DamageType = CardData.damageType;

		replaceCardButton.gameObject.SetActive(true);
	}

	public void SetupCard(AttackData AttackData)
	{
		if (AttackData == null)
		{
			Debug.LogError("Attack data null");
			return;
		}

		PlayerCard = false;
		AlsoHeals = AttackData.alsoHeals;
		Offensive = AttackData.offensive;

		string cardName = AttackData.attackName;
		gameObject.name = cardName;
		cardNametext.text = cardName;
		cardDescriptiontext.text = AttackData.CreateDescription();
		Damage = AttackData.damage;
		DamageType = AttackData.damageType;

		replaceCardButton.gameObject.SetActive(false);
	}

	//button call
	public void ReplaceCard()
	{
		OnCardReplace?.Invoke(this);
	}
}
