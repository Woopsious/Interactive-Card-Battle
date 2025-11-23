using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Woopsious.AbilitySystem;
using static Woopsious.DamageData;
using static Woopsious.AttackData;

namespace Woopsious
{
	public class CardUi : MonoBehaviour
	{
		//card rarity and border colours
		Image cardBorderImage;
		Color _Amber = new(1f, 0.669654f, 0f, 1f); //rare
		Color _BrightBlue = new(0f, 0.298039f, 1f, 1f); //uncommon
		Color _Gray = new(1f, 1f, 1f, 1f); //common

		[Header("Card Ui")]
		public TMP_Text cardNametext;
		public GameObject energyBackground;
		public TMP_Text cardCostText;
		public TMP_Text cardDescriptiontext;
		public TMP_Text cardCountForCardDeckUi;

		[Header("Discard Card Ui")]
		public GameObject discardCardUiPanel;
		public TMP_Text discardCardCountText;
		public Button addCardToDiscardList;
		public Button removeCardFromDiscardList;

		[Header("Reward Card Ui")]
		public GameObject rewardCardUiPanel;
		public Button toggleSelectRewardCardButton;
		TMP_Text rewardCardSelectedText;

		//runtime card Info
		public int CardDeckCount { get; private set; }
		public int CardDiscardCount { get; private set; }
		public bool CardSelectedAsReward { get; private set; }

		//runtime
		[HideInInspector] public CardHandler cardHandler;
		public RectTransform CardRectTransform { get; private set; }
		public AttackData AttackData { get; private set; }
		public DamageData DamageData { get; private set; }
		public bool PlayerCard { get; private set; }
		public bool DummyCard { get; private set; }
		public bool Offensive { get; private set; }
		public bool Selectable { get; private set; }

		void Awake()
		{
			CardRectTransform = GetComponent<RectTransform>();
			cardHandler = GetComponent<CardHandler>();
			cardBorderImage = GetComponent<Image>();
			addCardToDiscardList.onClick.AddListener(() => AddCardToDiscardList());
			removeCardFromDiscardList.onClick.AddListener(() => RemoveCardFromDiscardList());
			toggleSelectRewardCardButton.onClick.AddListener(() => ToggleSelectCardAsReward());
			rewardCardSelectedText = toggleSelectRewardCardButton.GetComponentInChildren<TMP_Text>();
			ToggleDiscardCardUi(false);
			ToggleRewardCardUi(false);
		}

		bool AttackDataNullCheck(AttackData attackData)
		{
			if (attackData == null)
			{
				Debug.LogError("Attack data null");
				return true;
			}
			else
				return false;
		}
		void DummyCardEffectMatchCheck()
		{
			if (!AttackData.addDummyCardsForEffects) return;

			foreach (var dummyCardEffect in AttackData.effectDummyCards)
			{
				foreach (var targetEffect in AttackData.DamageData.statusEffectsForTarget)
				{
					if (dummyCardEffect == targetEffect)
						continue;
				}
			}

			Debug.LogError("Failed to find match in status effects for target for dummy effect");
		}

		//card initilization
		public void SetupUiCard(AttackData attackData, int cardDeckCount)
		{
			if (AttackDataNullCheck(attackData))
				return;

			GetComponent<BoxCollider2D>().enabled = false;
			AttackData = attackData;
			PlayerCard = false;
			DummyCard = false;
			Offensive = attackData.offensive;

			string cardName = attackData.attackName;
			gameObject.name = cardName;
			cardNametext.text = cardName;
			ChangeBorderColour(attackData.cardRarity);

			if (attackData.energyCost > 0)
				cardCostText.text = $"+{attackData.energyCost}";
			else
				cardCostText.text = $"{attackData.energyCost}";

			DamageData = new(null, attackData.DamageData);

			cardDescriptiontext.text = CreateDescription();

			if (cardDeckCount != 0)
			{
				CardDeckCount = cardDeckCount;
				cardCountForCardDeckUi.text = $"{cardDeckCount}x";
				cardCountForCardDeckUi.gameObject.SetActive(true);
			}
			else
				cardCountForCardDeckUi.gameObject.SetActive(false);
		}
		public void SetupUiCard(AttackData attackData)
		{
			if (AttackDataNullCheck(attackData))
				return;

			GetComponent<BoxCollider2D>().enabled = false;
			AttackData = attackData;
			PlayerCard = false;
		    DummyCard = false;
			Offensive = attackData.offensive;

			string cardName = attackData.attackName;
			gameObject.name = cardName;
			cardNametext.text = cardName;
			energyBackground.SetActive(false);
			ChangeBorderColour(attackData.cardRarity);

			if (attackData.energyCost > 0)
				cardCostText.text = $"+{attackData.energyCost}";
			else
				cardCostText.text = $"{attackData.energyCost}";

			DamageData = new(null, attackData.DamageData);

			cardDescriptiontext.text = CreateDescription();
			cardCountForCardDeckUi.gameObject.SetActive(false);
		}

		public void SetupInGameCard(Entity cardOwner, AttackData attackData, bool playerCard)
		{
			if (AttackDataNullCheck(attackData))
				return;

			AttackData = attackData;
			PlayerCard = playerCard;
			DummyCard = false;
			Offensive = attackData.offensive;

			string cardName = attackData.attackName;
			gameObject.name = cardName;
			cardNametext.text = cardName;
			ChangeBorderColour(attackData.cardRarity);

			if (attackData.energyCost > 0)
				cardCostText.text = $"+{attackData.energyCost}";
			else
				cardCostText.text = $"{attackData.energyCost}";

			if (!playerCard)
				energyBackground.SetActive(false);

			DamageData = new(cardOwner, attackData.DamageData);
			DamageData.DamageValue = (int)(DamageData.DamageValue + cardOwner.damageBonus.value); //apply bonus damage
			DamageData.DamageValue = (int)(DamageData.DamageValue * cardOwner.damageDealtModifier.value); //apply damage dealt modifier

			cardDescriptiontext.text = CreateDescription();
			cardCountForCardDeckUi.gameObject.SetActive(false);
		}
		public void UpdateInGameCard(Entity cardOwner, bool playerCard)
		{
			PlayerCard = playerCard;
			DummyCard = false;

			DamageData = new(cardOwner, AttackData.DamageData);
			DamageData.DamageValue = (int)(DamageData.DamageValue + cardOwner.damageBonus.value); //apply bonus damage
			DamageData.DamageValue = (int)(DamageData.DamageValue * cardOwner.damageDealtModifier.value); //apply damage dealt modifier

			cardDescriptiontext.text = CreateDescription();
		}

		public void SetupCardRewards(AttackData attackData, int similarCardsInDeck)
		{
			if (AttackDataNullCheck(attackData))
				return;

			GetComponent<BoxCollider2D>().enabled = false;
			AttackData = attackData;
			PlayerCard = false;
			DummyCard = false;
			Offensive = attackData.offensive;

			string cardName = attackData.attackName;
			gameObject.name = cardName;
			cardNametext.text = cardName;
			ChangeBorderColour(attackData.cardRarity);

			if (attackData.energyCost > 0)
				cardCostText.text = $"+{attackData.energyCost}";
			else
				cardCostText.text = $"{attackData.energyCost}";

			DamageData = new(null, attackData.DamageData);

			cardDescriptiontext.text = CreateDescription();

			CardDeckCount = similarCardsInDeck;
			if (similarCardsInDeck == 1)
				cardCountForCardDeckUi.text = $"{similarCardsInDeck} duplicate";
			else
				cardCountForCardDeckUi.text = $"{similarCardsInDeck} duplicates";

			cardCountForCardDeckUi.gameObject.SetActive(true);
			ToggleRewardCardUi(true);
		}

		//special dummy card initilization
		public void SetupDummyCard(StatusEffectsData dummyCardEffectData)
		{
			PlayerCard = false;
			DummyCard = true;

			string cardName = dummyCardEffectData.effectName;
			gameObject.name = cardName;
			cardNametext.text = cardName;
			cardDescriptiontext.text = "Unplayable card\nDissapears next turn";

			energyBackground.SetActive(false);
			cardCountForCardDeckUi.gameObject.SetActive(false);
		}

		//description creation
		public string CreateDescription()
		{
			string description = AttackData.attackDescription;
			description += "\n";

			if (AttackData.extraCardsToDraw != 0)
			{
				if (AttackData.extraCardsToDraw == 1)
					description += $"+{AttackData.extraCardsToDraw} card next turn\n(Max: 9)\n";
				else
					description += $"+{AttackData.extraCardsToDraw} cards next turn\n(Max: 9)\n";
			}

			if (DamageData.valueTypes.HasFlag(ValueTypes.damages))
				description = CreateDamageDescription(description);

			description = CreateStatusEffectDescriptions(description, DamageData.statusEffectsForTarget, true);

			if (DamageData.valueTypes.HasFlag(ValueTypes.blocks))
				description += $"\nGain {RichTextManager.AddColour($"{DamageData.BlockValue} block", RichTextManager.steelBlue)}";

			if (DamageData.valueTypes.HasFlag(ValueTypes.heals))
				description += $"\nRestore {RichTextManager.AddColour($"{DamageData.HealValue} health", RichTextManager.darkGreen)}";

			description = CreateStatusEffectDescriptions(description, DamageData.statusEffectsForSelf, false);

			return description;
		}

		string CreateDamageDescription(string description)
		{
			if (DamageData.isMultiHitAttack)
			{
				int splitDamage = DamageData.DamageValue / DamageData.multiHitCount;
				string damageString = $"{splitDamage} damage";

				if (DamageData.HitsDifferentTargets)
				{
					description += $"\nDeals {RichTextManager.AddColour(damageString, RichTextManager.crimsonRed)} " +
						$"to {DamageData.multiHitCount} different enemies";
				}
				else
					description += $"\nDeals {RichTextManager.AddColour(damageString, RichTextManager.crimsonRed)} {DamageData.multiHitCount}x times";
			}
			else
				description += $"\nDeals {RichTextManager.AddColour($"{DamageData.DamageValue} damage", RichTextManager.crimsonRed)}";

			return description;
		}	
		string CreateStatusEffectDescriptions(string description, List<StatusEffectsData> statusEffects, bool enemyEffects)
		{
			if (statusEffects.Count != 0)
			{
				description += "\nApplies ";
				Dictionary<StatusEffectsData, int> statusEffectsCount = new();

				foreach (StatusEffectsData statusEffect in statusEffects)
				{
					if (statusEffectsCount.ContainsKey(statusEffect))
						statusEffectsCount[statusEffect]++;
					else
						statusEffectsCount.Add(statusEffect, 1);
				}

				foreach (var entry in statusEffectsCount)
				{
					if (entry.Value == 1)
						description += "<link=Test><color=blue>" + entry.Key.effectName + "</color></link>, ";
					else
						description += "<link=Test><color=blue>" + entry.Value + "x " + entry.Key.effectName + "</color></link>, ";
				}

				description = RichTextManager.RemoveLastComma(description);

				if (enemyEffects)
					description += "to enemies";
				else
					description += "to self";
			}

			return description;
		}
		void ChangeBorderColour(CardRarity cardRarity)
		{
			if (cardRarity == CardRarity.Rare)
				cardBorderImage.color = _Amber;
			else if (cardRarity == CardRarity.Uncommon)
				cardBorderImage.color = _BrightBlue;
			else
				cardBorderImage.color = _Gray;
		}

		public void EnableCard()
		{
			if (!PlayerCard)
				Selectable = false;
			else
				Selectable = true;
		}
		public void DisableCard()
		{
			Selectable = false;
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

		//reward card funcs
		public void ToggleRewardCardUi(bool show)
		{
			rewardCardUiPanel.SetActive(show);
			rewardCardSelectedText.text = $"Unselected";
		}
		void ToggleSelectCardAsReward()
		{
			if (PlayerCardDeckUi.CanSelectCardAsReward() && !CardSelectedAsReward)
			{
				CardSelectedAsReward = true;
				PlayerCardDeckUi.AddCardToBeAdded(AttackData);
				rewardCardSelectedText.text = $"Unselect";
				rewardCardSelectedText.color = new(1f, 0.2941177f, 0f);
			}
			else if (CardSelectedAsReward)
			{
				CardSelectedAsReward = false;
				PlayerCardDeckUi.RemoveCardFromBeingAdded(AttackData);
				rewardCardSelectedText.text = $"Select";
				rewardCardSelectedText.color = new(0f, 0.5882353f, 0f);
			}
		}
	}
}