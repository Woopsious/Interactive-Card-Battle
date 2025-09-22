using System;
using TMPro;
using UnityEngine;
using static Woopsious.DamageData;
using UnityEngine.UI;

namespace Woopsious
{
	public class CardUi : MonoBehaviour
	{
		[Header("Card Ui")]
		public TMP_Text cardNametext;
		public GameObject energyBackground;
		public TMP_Text cardCostText;
		public TMP_Text cardDescriptiontext;
		public TMP_Text cardCountForCardDeckUi;
		public RectTransform replaceCardButton;

		[Header("Discard Card Ui")]
		public GameObject discardCardUiPanel;
		public TMP_Text discardCardCountText;
		public Button addCardToDiscardList;
		public Button removeCardFromDiscardList;

		//runtime card counts
		public int CardDeckCount { get; private set; }
		public int CardDiscardCount { get; private set; }

		//runtime
		[HideInInspector] public CardHandler cardHandler;
		public RectTransform CardRectTransform { get; private set; }
		public AttackData AttackData { get; private set; }
		public bool PlayerCard { get; private set; }
		public bool Offensive { get; private set; }
		public DamageData DamageData { get; private set; }

		public bool selectable;

		public static event Action<CardUi> OnCardReplace;

		void Awake()
		{
			CardRectTransform = GetComponent<RectTransform>();
			cardHandler = GetComponent<CardHandler>();
			addCardToDiscardList.onClick.AddListener(() => AddCardToDiscardList());
			removeCardFromDiscardList.onClick.AddListener(() => RemoveCardFromDiscardList());
			ToggleDiscardCardUi(false);
		}

		//card initilization
		public void SetupUiCard(AttackData attackData, int cardDeckCount)
		{
			if (attackData == null)
			{
				Debug.LogError("Attack data null");
				return;
			}

			GetComponent<BoxCollider2D>().enabled = false;
			AttackData = attackData;
			Offensive = attackData.offensive;

			string cardName = attackData.attackName;
			gameObject.name = cardName;
			cardNametext.text = cardName;
			cardCostText.text = $"{attackData.energyCost}";

			DamageData = new(null, attackData.DamageData);

			cardDescriptiontext.text = CreateDescription();
			ToggleReplaceCardButton(false);

			CardDeckCount = cardDeckCount;
			cardCountForCardDeckUi.text = $"{cardDeckCount}x";
			cardCountForCardDeckUi.gameObject.SetActive(true);
		}
		public void SetupUiCard(AttackData attackData)
		{
			if (attackData == null)
			{
				Debug.LogError("Attack data null");
				return;
			}

			GetComponent<BoxCollider2D>().enabled = false;
			AttackData = attackData;
			Offensive = attackData.offensive;

			string cardName = attackData.attackName;
			gameObject.name = cardName;
			cardNametext.text = cardName;
			energyBackground.SetActive(false);
			cardCostText.text = $"{attackData.energyCost}";

			DamageData = new(null, attackData.DamageData);

			cardDescriptiontext.text = CreateDescription();
			cardCountForCardDeckUi.gameObject.SetActive(false);
			ToggleReplaceCardButton(false);
		}

		public void SetupInGameCard(Entity cardOwner, AttackData attackData, bool playerCard)
		{
			if (attackData == null)
			{
				Debug.LogError("Attack data null");
				return;
			}

			AttackData = attackData;
			PlayerCard = playerCard;
			Offensive = attackData.offensive;

			string cardName = attackData.attackName;
			gameObject.name = cardName;
			cardNametext.text = cardName;
			cardCostText.text = $"{attackData.energyCost}";

			if (!playerCard)
				energyBackground.SetActive(false);

			DamageData = new(cardOwner, attackData.DamageData);
			DamageData.DamageValue = (int)(DamageData.DamageValue + cardOwner.damageBonus.Value); //apply bonus damage
			DamageData.DamageValue = (int)(DamageData.DamageValue * cardOwner.damageDealtModifier.Value); //apply damage dealt modifier

			cardDescriptiontext.text = CreateDescription();
			cardCountForCardDeckUi.gameObject.SetActive(false);
			ToggleReplaceCardButton(false);
		}
		public void UpdateInGameCard(Entity cardOwner, bool playerCard)
		{
			PlayerCard = playerCard;

			DamageData = new(cardOwner, AttackData.DamageData);
			DamageData.DamageValue = (int)(DamageData.DamageValue + cardOwner.damageBonus.Value); //apply bonus damage
			DamageData.DamageValue = (int)(DamageData.DamageValue * cardOwner.damageDealtModifier.Value); //apply damage dealt modifier

			cardDescriptiontext.text = CreateDescription();
		}

		//description creation
		public string CreateDescription()
		{
			string description = AttackData.attackDescription;
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

				description = RichTextManager.RemoveLastComma(description);
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

				description = RichTextManager.RemoveLastComma(description);
				description += "to self";
			}

			return description;
		}

		//replace card button toggle
		public void ToggleReplaceCardButton(bool showButton)
		{
			if (showButton)
				replaceCardButton.gameObject.SetActive(true);
			else
				replaceCardButton.gameObject.SetActive(false);
		}

		//discard card funcs
		public void ToggleDiscardCardUi(bool show)
		{
			discardCardUiPanel.SetActive(show);
			discardCardCountText.text = "0";
		}
		void AddCardToDiscardList()
		{
			if (CardDiscardCount >= CardDeckCount)
			{
				CardDiscardCount = CardDeckCount;
				Debug.LogError($"card discard count already at {CardDeckCount}");
			}
			else
			{
				CardDiscardCount++;
				PlayerCardDeckUi.AddCardToBeDiscarded(AttackData);
			}

			discardCardCountText.text = $"{CardDiscardCount}";
		}
		void RemoveCardFromDiscardList()
		{
			if (CardDiscardCount <= 0)
			{
				CardDiscardCount = 0;
				Debug.LogError("card discard count already at 0");
			}
			else
			{
				CardDiscardCount--;
				PlayerCardDeckUi.RemoveCardFromBeingDiscarded(AttackData);
			}

			discardCardCountText.text = $"{CardDiscardCount}";
		}

		//button call
		public void ReplaceCard()
		{
			OnCardReplace?.Invoke(this);
		}
	}
}