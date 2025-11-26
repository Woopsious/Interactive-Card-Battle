using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Woopsious.AbilitySystem;
using static Woopsious.DamageData;
using static Woopsious.AttackData;
using static Woopsious.CardHandler;

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

		public UiType uiType;
		public enum UiType
		{
			baseCard, databaseCard, RewardCard
		}

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
		public RectTransform RectTransform { get; private set; }

		void Awake()
		{
			RectTransform = GetComponent<RectTransform>();
			cardHandler = GetComponent<CardHandler>();
			cardBorderImage = GetComponent<Image>();
			addCardToDiscardList.onClick.AddListener(() => AddCardToDiscardList());
			removeCardFromDiscardList.onClick.AddListener(() => RemoveCardFromDiscardList());
			toggleSelectRewardCardButton.onClick.AddListener(() => ToggleSelectCardAsReward());
			rewardCardSelectedText = toggleSelectRewardCardButton.GetComponentInChildren<TMP_Text>();
			ToggleDiscardCardUi(false);
			ToggleRewardCardUi(false);

			cardHandler.InitilzeInformationalCardUi += InitilizeInformationalCard;
			cardHandler.InitilzeCardUi += InitilizeCardUi;
			cardHandler.InitilzeDummyCardUi += InitilizeDummyCardUi;
			cardHandler.InitilzeRewardCardUi += InitilizeRewardCardUi;
			cardHandler.UpdateCardDescriptionUi += UpdateCardDescriptionUi;
		}
		private void OnDestroy()
		{
			cardHandler.InitilzeInformationalCardUi -= InitilizeInformationalCard;
			cardHandler.InitilzeCardUi -= InitilizeCardUi;
			cardHandler.InitilzeDummyCardUi -= InitilizeDummyCardUi;
			cardHandler.InitilzeRewardCardUi -= InitilizeRewardCardUi;
			cardHandler.UpdateCardDescriptionUi -= UpdateCardDescriptionUi;
		}

		//card Ui initilization
		private void InitilizeInformationalCard(CardInitType cardInitType, int cardDeckCount)
		{
			UpdateCardName(cardHandler.AttackData.attackName);
			ChangeBorderColour(cardHandler.AttackData.cardRarity);

			UpdateCardEnergyUi(false);
			UpdateCardCountUi(true, true, cardDeckCount);
			ToggleRewardCardUi(false);
			UpdateCardDescriptionUi(cardInitType);
		}
		private void InitilizeCardUi(CardInitType cardInitType, bool playerCard, int cardDeckCount)
		{
			UpdateCardName(cardHandler.AttackData.attackName);
			ChangeBorderColour(cardHandler.AttackData.cardRarity);

			UpdateCardEnergyUi(playerCard);
			UpdateCardCountUi(false, false, cardDeckCount);
			ToggleRewardCardUi(false);
			UpdateCardDescriptionUi(cardInitType);
		}
		private void InitilizeDummyCardUi(CardInitType cardInitType, StatusEffectsData statusEffectsData)
		{
			UpdateCardName(statusEffectsData.effectName);
			ChangeBorderColour(CardRarity.Common);

			UpdateCardEnergyUi(false);
			UpdateCardCountUi(false, false, 0);
			ToggleRewardCardUi(false);
			UpdateCardDescriptionUi(cardInitType);
		}
		private void InitilizeRewardCardUi(CardInitType cardInitType, int cardDeckCount)
		{
			UpdateCardName(cardHandler.AttackData.attackName);
			ChangeBorderColour(cardHandler.AttackData.cardRarity);

			UpdateCardEnergyUi(false);
			UpdateCardCountUi(true, false, cardDeckCount);
			ToggleRewardCardUi(true);
			UpdateCardDescriptionUi(cardInitType);
		}

		//sub ui funcs
		private void UpdateCardName(string cardName)
		{
			gameObject.name = cardName;
			cardNametext.text = cardName;
		}
		public void UpdateCardDescriptionUi(CardInitType cardInitType)
		{
			if (cardInitType == CardInitType.Dummy)
				cardDescriptiontext.text = "Unplayable card\nDissapears next turn";
			else
				cardDescriptiontext.text = CreateDescription();
		}
		private void UpdateCardEnergyUi(bool showUi)
		{
			if (showUi)
			{
				if (cardHandler.AttackData.energyCost > 0)
					cardCostText.text = $"+{cardHandler.AttackData.energyCost}";
				else
					cardCostText.text = $"{cardHandler.AttackData.energyCost}";
			}
			else
				energyBackground.SetActive(false);
		}
		private void UpdateCardCountUi(bool showUi, bool ignoreZeroCount, int cardCount)
		{
			if (showUi)
			{
				CardDeckCount = cardCount;
				cardCountForCardDeckUi.text = $"{cardCount}x";
				cardCountForCardDeckUi.gameObject.SetActive(true);

				if (ignoreZeroCount && cardCount == 0)
					cardCountForCardDeckUi.gameObject.SetActive(false);
			}
			else
				cardCountForCardDeckUi.gameObject.SetActive(false);
		}

		//description creation
		public string CreateDescription()
		{
			AttackData attackData = cardHandler.AttackData;
			DamageData damageData = cardHandler.DamageData;

			string description = attackData.attackDescription;
			description += "\n";

			if (attackData.extraCardsToDraw != 0)
			{
				if (attackData.extraCardsToDraw == 1)
					description += $"+{attackData.extraCardsToDraw} card next turn\n(Max: 9)\n";
				else
					description += $"+{attackData.extraCardsToDraw} cards next turn\n(Max: 9)\n";
			}

			if (damageData.valueTypes.HasFlag(ValueTypes.damages))
				description = CreateDamageDescription(damageData, description);

			description = CreateStatusEffectDescriptions(description, damageData.statusEffectsForTarget, true);

			if (damageData.valueTypes.HasFlag(ValueTypes.blocks))
				description += $"\nGain {RichTextManager.AddColour($"{damageData.BlockValue} block", RichTextManager.steelBlue)}";

			if (damageData.valueTypes.HasFlag(ValueTypes.heals))
				description += $"\nRestore {RichTextManager.AddColour($"{damageData.HealValue} health", RichTextManager.darkGreen)}";

			description = CreateStatusEffectDescriptions(description, damageData.statusEffectsForSelf, false);

			return description;
		}
		private string CreateDamageDescription(DamageData damageData, string description)
		{
			if (damageData.isMultiHitAttack)
			{
				int splitDamage = damageData.DamageValue / damageData.multiHitCount;
				string damageString = $"{splitDamage} damage";

				if (damageData.HitsDifferentTargets)
				{
					description += $"\nDeals {RichTextManager.AddColour(damageString, RichTextManager.crimsonRed)} " +
						$"to {damageData.multiHitCount} different enemies";
				}
				else
					description += $"\nDeals {RichTextManager.AddColour(damageString, RichTextManager.crimsonRed)} {damageData.multiHitCount}x times";
			}
			else
				description += $"\nDeals {RichTextManager.AddColour($"{damageData.DamageValue} damage", RichTextManager.crimsonRed)}";

			return description;
		}
		private string CreateStatusEffectDescriptions(string description, List<StatusEffectsData> statusEffects, bool enemyEffects)
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
		private void ChangeBorderColour(CardRarity cardRarity)
		{
			if (cardRarity == CardRarity.Rare)
				cardBorderImage.color = _Amber;
			else if (cardRarity == CardRarity.Uncommon)
				cardBorderImage.color = _BrightBlue;
			else
				cardBorderImage.color = _Gray;
		}

		//discard card funcs
		public void ToggleDiscardCardUi(bool show)
		{
			discardCardUiPanel.SetActive(show);
			discardCardCountText.text = "0";
		}
		private void AddCardToDiscardList()
		{
			if (CardDiscardCount >= CardDeckCount)
			{
				CardDiscardCount = CardDeckCount;
				Debug.LogError($"card discard count already at {CardDeckCount}");
			}
			else
			{
				CardDiscardCount++;
				PlayerCardDeckHandler.QueueCardToBeDiscarded(cardHandler.AttackData);
			}

			discardCardCountText.text = $"{CardDiscardCount}";
		}
		private void RemoveCardFromDiscardList()
		{
			if (CardDiscardCount <= 0)
			{
				CardDiscardCount = 0;
				Debug.LogError("card discard count already at 0");
			}
			else
			{
				CardDiscardCount--;
				PlayerCardDeckHandler.UnqueueCardFromBeingDiscarded(cardHandler.AttackData);
			}

			discardCardCountText.text = $"{CardDiscardCount}";
		}

		//reward card funcs
		public void ToggleRewardCardUi(bool show)
		{
			rewardCardUiPanel.SetActive(show);
			rewardCardSelectedText.text = $"Unselected";
		}
		private void ToggleSelectCardAsReward()
		{
			if (PlayerCardDeckHandler.CanSelectCardAsReward() && !CardSelectedAsReward)
			{
				CardSelectedAsReward = true;
				PlayerCardDeckHandler.QueueCardToBeAdded(cardHandler.AttackData);
				rewardCardSelectedText.text = $"Unselect";
				rewardCardSelectedText.color = new(1f, 0.2941177f, 0f);
			}
			else if (CardSelectedAsReward)
			{
				CardSelectedAsReward = false;
				PlayerCardDeckHandler.UnqueueCardFromBeingAdded(cardHandler.AttackData);
				rewardCardSelectedText.text = $"Select";
				rewardCardSelectedText.color = new(0f, 0.5882353f, 0f);
			}
		}
	}
}