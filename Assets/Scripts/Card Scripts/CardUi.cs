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
		public GameObject removeCardUiPanel;
		public TMP_Text removeCardCountText;
		public Button addCardToRemoveList;
		public Button removeCardFromRemoveList;

		[Header("Reward Card Ui")]
		public GameObject rewardCardUiPanel;
		public Button toggleSelectRewardCardButton;
		TMP_Text rewardCardSelectedText;

		[Header("Draw Card Ui")]
		public GameObject drawCardUiPanel;
		public TMP_Text drawCardText;

		[Header("Drop Card Ui")]
		public GameObject dropCardUiPanel;
		public TMP_Text dropCardText;

		//runtime card Info
		public int CardDeckCount { get; private set; }
		public int CardRemoveCount { get; private set; }
		public bool CardSelectedAsReward { get; private set; }

		//runtime
		[HideInInspector] public CardHandler cardHandler;
		public RectTransform RectTransform { get; private set; }

		private void Awake()
		{
			RectTransform = GetComponent<RectTransform>();
			cardHandler = GetComponent<CardHandler>();
			cardBorderImage = GetComponent<Image>();
			addCardToRemoveList.onClick.AddListener(() => AddCardToRemoveList());
			removeCardFromRemoveList.onClick.AddListener(() => RemoveCardFromRemoveList());
			toggleSelectRewardCardButton.onClick.AddListener(() => ToggleSelectCardAsReward());
			rewardCardSelectedText = toggleSelectRewardCardButton.GetComponentInChildren<TMP_Text>();
			ToggleRemoveCardUi(false);
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
			ToggleDropChanceUi(true);
			ToggleDrawChanceUi(true);
			UpdateCardDescriptionUi(cardInitType);
		}
		private void InitilizeCardUi(CardInitType cardInitType, bool playerCard, int cardDeckCount)
		{
			UpdateCardName(cardHandler.AttackData.attackName);
			ChangeBorderColour(cardHandler.AttackData.cardRarity);

			UpdateCardEnergyUi(playerCard);
			UpdateCardCountUi(false, false, cardDeckCount);
			ToggleRewardCardUi(false);
			ToggleDropChanceUi(false);
			ToggleDrawChanceUi(false);
			UpdateCardDescriptionUi(cardInitType);
		}
		private void InitilizeDummyCardUi(CardInitType cardInitType, StatusEffectsData statusEffectsData)
		{
			UpdateCardName(statusEffectsData.effectName);
			ChangeBorderColour(CardRarity.Common);

			UpdateCardEnergyUi(false);
			UpdateCardCountUi(false, false, 0);
			ToggleRewardCardUi(false);
			ToggleDropChanceUi(false);
			ToggleDrawChanceUi(false);
			UpdateCardDescriptionUi(cardInitType);
		}
		private void InitilizeRewardCardUi(CardInitType cardInitType, int cardDeckCount)
		{
			UpdateCardName(cardHandler.AttackData.attackName);
			ChangeBorderColour(cardHandler.AttackData.cardRarity);

			UpdateCardEnergyUi(false);
			UpdateCardCountUi(true, false, cardDeckCount);
			ToggleRewardCardUi(true);
			ToggleDropChanceUi(true);
			ToggleDrawChanceUi(true);
			UpdateCardDescriptionUi(cardInitType);
		}

		//sub ui funcs
		private void UpdateCardName(string cardName)
		{
			gameObject.name = cardName;
			cardNametext.text = cardName;
		}
		private void UpdateCardDescriptionUi(CardInitType cardInitType)
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
		private string CreateDescription()
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
				description += $"\nGain {RichTextManager.AddValueTypeColour($"{damageData.BlockValue} block", ValueTypes.blocks)}";

			if (damageData.valueTypes.HasFlag(ValueTypes.heals))
				description += $"\nRestore {RichTextManager.AddValueTypeColour($"{damageData.HealValue} health", ValueTypes.heals)}";

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
					description += $"\nDeals {RichTextManager.AddValueTypeColour(damageString, ValueTypes.damages)}" +
						$" to {damageData.multiHitCount} different enemies";
				}
				else
					description += $"\nDeals {RichTextManager.AddValueTypeColour(damageString, ValueTypes.damages)} {damageData.multiHitCount}x times";
			}
			else
				description += $"\nDeals {RichTextManager.AddValueTypeColour($"{damageData.DamageValue} damage", ValueTypes.damages)}";

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
						description += RichTextManager.AddTextLink($"{entry.Key.effectName}", RichTextManager.GlobalColours.blue);
					else
						description += RichTextManager.AddTextLink($"{entry.Value}x {entry.Key.effectName}", RichTextManager.GlobalColours.blue);

					description += ", ";
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

		//remove card funcs
		public void ToggleRemoveCardUi(bool show)
		{
			removeCardUiPanel.SetActive(show);
			removeCardCountText.text = "0";
		}
		private void AddCardToRemoveList()
		{
			if (CardRemoveCount >= CardDeckCount)
			{
				CardRemoveCount = CardDeckCount;
				Debug.LogError($"card remove count already at {CardDeckCount}");
			}
			else
			{
				CardRemoveCount++;
				PlayerCardDeckHandler.QueueCardToBeRemoved(cardHandler.AttackData);
			}

			removeCardCountText.text = $"{CardRemoveCount}";
		}
		private void RemoveCardFromRemoveList()
		{
			if (CardRemoveCount <= 0)
			{
				CardRemoveCount = 0;
				Debug.LogError("card remove count already at 0");
			}
			else
			{
				CardRemoveCount--;
				PlayerCardDeckHandler.UnqueueCardFromBeingRemoved(cardHandler.AttackData);
			}

			removeCardCountText.text = $"{CardRemoveCount}";
		}

		//reward card funcs
		private void ToggleRewardCardUi(bool show)
		{
			rewardCardUiPanel.SetActive(show);
			CardSelectedAsReward = false;
			rewardCardSelectedText.text = $"Select";
			rewardCardSelectedText.color = new(0f, 0.5882353f, 0f);
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

		//drop/draw chance funcs
		private void ToggleDropChanceUi(bool show)
		{
			if (!cardHandler.AttackData.isPlayerAttack || cardHandler.DummyCard)
			{
				dropCardUiPanel.SetActive(false);
				return;
			}

			dropCardUiPanel.SetActive(show);
			dropCardText.text = $"Drop Chance: {cardHandler.AttackData.CardDropChance() * 100}%";
		}
		private void ToggleDrawChanceUi(bool show)
		{
			if (!cardHandler.AttackData.isPlayerAttack || cardHandler.DummyCard)
			{
				drawCardUiPanel.SetActive(false);
				return;
			}

			drawCardUiPanel.SetActive(show);
			drawCardText.text = $"Draw Chance: {cardHandler.AttackData.CardDrawChance() * 100}%";
		}
	}
}