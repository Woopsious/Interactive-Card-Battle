using UnityEngine;
using static Woopsious.DamageData;

namespace Woopsious
{
	[CreateAssetMenu(fileName = "CardData", menuName = "ScriptableObjects/Card")]
	public class CardData : ScriptableObject
	{
		[Header("Card Info")]
		public string cardName;
		public string cardDescription;

		public bool offensive;
		public bool alsoHeals;

		[Header("Card damage")]
		public int damage;

		public DamageType damageType;

		public string CreateDescription()
		{
			string cardDescription = this.cardDescription;
			cardDescription += "\n\n";

			if (damageType == DamageType.block)
				cardDescription += "Blocks " + damage + " damage";
			else if (damageType == DamageType.heal)
				cardDescription += "Heals " + damage + " damage";
			else if (damageType == DamageType.physical)
				cardDescription += "Deals " + damage + " physical damage";

			return cardDescription;
		}
	}
}
