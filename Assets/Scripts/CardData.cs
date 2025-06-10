using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "ScriptableObjects/Card")]
public class CardData : ScriptableObject
{
	[Header("Card Info")]
	public string cardName;
	public string cardDescription;

	[Header("Card damage")]
	public int damage;

	public DamageType damageType;
	public enum DamageType
	{
		physical
	}

	public string CreateDescription()
	{
		string cardDescription = this.cardDescription;
		cardDescription += "\n\nDeals ";

		if (damageType == DamageType.physical)
			cardDescription += damage + " physical damage";

		return cardDescription;
	}
}
