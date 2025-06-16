using TMPro;
using UnityEngine;
using static DamageData;

public class CardUi : MonoBehaviour
{
	public bool PlayerCard { get; private set; }
	public int Damage { get; private set; }
	public DamageType DamageType { get; private set; }

	public bool selectable;

	public TMP_Text cardNametext;
	public TMP_Text cardDescriptiontext;

	private void Awake()
	{
		gameObject.name = "Card" + Random.Range(1000, 9999);
	}

	public void SetupCard(CardData CardData)
	{
		if (CardData == null)
		{
			Debug.LogError("Card data null");
			return;
		}
		PlayerCard = true;

		string cardName = CardData.cardName + Random.Range(1000, 9999);
		gameObject.name = cardName;
		cardNametext.text = cardName;
		cardDescriptiontext.text = CardData.CreateDescription();
		Damage = CardData.damage;
		DamageType = CardData.damageType;
	}

	public void SetupCard(AttackData AttackData)
	{
		if (AttackData == null)
		{
			Debug.LogError("Attack data null");
			return;
		}
		PlayerCard = false;

		string cardName = AttackData.attackName + Random.Range(1000, 9999);
		gameObject.name = cardName;
		cardNametext.text = cardName;
		cardDescriptiontext.text = AttackData.CreateDescription();
		Damage = AttackData.damage;
		DamageType = AttackData.damageType;
	}
}
