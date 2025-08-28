using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using static Woopsious.DamageData;

namespace Woopsious
{
	public class CardUi : MonoBehaviour
	{
		[HideInInspector] public CardHandler cardHandler;

		public TMP_Text cardNametext;
		public TMP_Text cardDescriptiontext;
		public RectTransform replaceCardButton;

		AttackData attackData;
		public bool PlayerCard { get; private set; }
		public bool Offensive { get; private set; }
		public DamageData DamageData { get; private set; }

		public bool selectable;

		public static event Action<CardUi> OnCardReplace;

		void Awake()
		{
			cardHandler = GetComponent<CardHandler>();
		}

		public void SetupCard(Entity cardOwner, AttackData AttackData, bool playerCard)
		{
			if (AttackData == null)
			{
				Debug.LogError("Attack data null");
				return;
			}

			attackData = AttackData;
			PlayerCard = playerCard;
			Offensive = AttackData.offensive;

			string cardName = AttackData.attackName;
			gameObject.name = cardName;
			cardNametext.text = cardName;

			DamageData = new(cardOwner, AttackData.DamageData);
			DamageData.DamageValue = (int)(DamageData.DamageValue + cardOwner.damageBonus.Value); //apply bonus damage
			DamageData.DamageValue = (int)(DamageData.DamageValue * cardOwner.damageDealtModifier.Value); //apply damage dealt modifier

			cardDescriptiontext.text = CreateDescription();
			replaceCardButton.gameObject.SetActive(false);
		}
		public void UpdateCard(Entity cardOwner, bool playerCard)
		{
			PlayerCard = playerCard;

			DamageData = new(cardOwner, attackData.DamageData);
			DamageData.DamageValue = (int)(DamageData.DamageValue + cardOwner.damageBonus.Value); //apply bonus damage
			DamageData.DamageValue = (int)(DamageData.DamageValue * cardOwner.damageDealtModifier.Value); //apply damage dealt modifier

			cardDescriptiontext.text = CreateDescription();
		}

		public string CreateDescription()
		{
			string description = attackData.attackDescription;
			description += "\n";

			if (DamageData.valueTypes == ValueTypes.none)
				Debug.LogError("Value type not set");

			if (DamageData.valueTypes.HasFlag(ValueTypes.dealsDamage))
			{
				if (DamageData.damageType == DamageType.physical)
					description += "\nDeals " + DamageData.DamageValue + " physical damage";

				if (DamageData.isAoeAttack)
					description += " each to a max of " + DamageData.maxAoeTargets + " targets";

				switch (DamageData.multiHitSettings)
				{
					case MultiHitAttack.No:
					break;
					case MultiHitAttack.TwoHits:
					description += " 2x (" + DamageData.DamageValue * 2 + ")";
					break;
					case MultiHitAttack.ThreeHits:
					description += " 3x (" + DamageData.DamageValue * 3 + ")";
					break;
					case MultiHitAttack.FourHits:
					description += " 4x (" + DamageData.DamageValue * 4 + ")";
					break;
				}
			}
			if (DamageData.valueTypes.HasFlag(ValueTypes.blocks))
				description += "\nBlocks " + DamageData.BlockValue + " damage";
			if (DamageData.valueTypes.HasFlag(ValueTypes.heals))
				description += "\nHeals " + DamageData.HealValue + " damage";

			return description;
		}

		//button call
		public void ReplaceCard()
		{
			OnCardReplace?.Invoke(this);
		}
	}
}