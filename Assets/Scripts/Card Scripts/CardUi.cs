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
				description = CreateDamageDescription(description);

			description = CreateTargetStatusEffectDescription(description);

			if (DamageData.valueTypes.HasFlag(ValueTypes.blocks))
				description += "\nGain " + DamageData.BlockValue + " block";

			if (DamageData.valueTypes.HasFlag(ValueTypes.heals))
				description += "\nHeal self for " + DamageData.HealValue;

			description = CreateSelfStatusEffectDescription(description);

			return description;
		}

		string CreateDamageDescription(string description)
		{
			if (DamageData.isMultiHitAttack)
			{
				int splitDamage = DamageData.DamageValue / DamageData.multiHitCount;
				if (DamageData.HitsDifferentTargets)
				{
					description += "\nDeal " + splitDamage + " damage to " + DamageData.multiHitCount + " enemies";
				}
				else
				{
					description += "\nDeal " + splitDamage + " (" + DamageData.DamageValue + ") " + DamageData.multiHitCount + "x to enemy";
				}
			}
			else
				description += "\nDeal " + DamageData.DamageValue + " damage to enemy";

			return description;
		}	
		string CreateTargetStatusEffectDescription(string description)
		{
			if (DamageData.statusEffectsForTarget.Count != 0)
			{
				description += "\nApplies ";

				foreach (StatusEffectsData statusEffect in DamageData.statusEffectsForTarget)
					description += "<link=\"Test\"><color=\"blue\">" + statusEffect.effectName + "</color></link>, ";

				description = RemoveLastComma(description);
				description += "to enemy";
			}

			return description;
		}
		string CreateSelfStatusEffectDescription(string description)
		{
			if (DamageData.statusEffectsForSelf.Count != 0)
			{
				description += "\nApplies ";

				foreach (StatusEffectsData statusEffect in DamageData.statusEffectsForSelf)
					description += "<link=\"Test\"><color=\"blue\">" + statusEffect.effectName + "</color></link>, ";

				description = RemoveLastComma(description);
				description += "to self";
			}

			return description;
		}

		string RemoveLastComma(string input)
		{
			int lastCommaIndex = input.LastIndexOf(',');

			if (lastCommaIndex >= 0)
				return input.Remove(lastCommaIndex, 1);

			return input;
		}

		//button call
		public void ReplaceCard()
		{
			OnCardReplace?.Invoke(this);
		}
	}
}