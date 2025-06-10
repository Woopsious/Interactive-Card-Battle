using TMPro;
using UnityEngine;

public class CardUi : MonoBehaviour
{
	public CardData CardData { get; private set; }

	public bool PlayerCard { get; private set; }

	public int Damage { get; private set; }

	public TMP_Text cardNametext;
	public TMP_Text cardDescriptiontext;

	private void Awake()
	{
		gameObject.name = "Card" + Random.Range(1000, 9999);
	}

	public void SetupCard(CardData CardData, bool PlayerCard)
	{
		if (CardData == null)
		{
			Debug.LogError("Card data null");
			return;
		}
		this.PlayerCard = PlayerCard;
		this.CardData = CardData;

		string cardName = CardData.cardName + Random.Range(1000, 9999);
		gameObject.name = cardName;
		cardNametext.text = cardName;
		cardDescriptiontext.text = CardData.CreateDescription();
		Damage = CardData.damage;
	}

	//func to make damage scale with card speed (unused)
	public void UpdateDamageWithVelocity(float velocity)
	{
		float baseDamage = CardData.damage;
		float newDamage = baseDamage + (velocity / 100);

		Damage = (int)newDamage;
	}
}
