using System;
using UnityEngine;
using static Woopsious.Stat;

namespace Woopsious
{
	[Serializable]
	public class StatusEffect : MonoBehaviour
	{

		Entity entity;
		StatusEffectsHandler statusEffectsHandler;
		public StatusEffectsData StatusEffectsData { get; private set; }
		RectTransform rectTransform;

		[Header("Effect Info")]
		[SerializeField] public string effectName;
		[SerializeField] public string effectDescription;

		[Header("Effect Runtime")]
		[SerializeField] public int effectCurrentStacks;
		[SerializeField] public int effectLifetimeLeft;
		[SerializeField] public float effectValue;

		void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
		}

		public void InitilizeStatusEffect(Entity entity, StatusEffectsHandler effectsHandler, StatusEffectsData statusEffectsData)
		{
			this.entity = entity;
			statusEffectsHandler = effectsHandler;
			StatusEffectsData = statusEffectsData;

			name = statusEffectsData.effectName;
			effectName = statusEffectsData.effectName;
			effectDescription = statusEffectsData.effectDescription;

			effectCurrentStacks = statusEffectsData.effectStacks;
			effectLifetimeLeft = statusEffectsData.effectTurnLifetime;

			if (statusEffectsData.hasStacks)
				effectValue = statusEffectsData.effectValue * effectCurrentStacks;
			else
				effectValue = statusEffectsData.effectValue;
		}
		public void ReApplyStatusEffect(StatusEffectsData statusEffectsData)
		{
			effectCurrentStacks += statusEffectsData.effectStacks;
			effectLifetimeLeft = statusEffectsData.effectTurnLifetime;

			if (effectCurrentStacks > statusEffectsData.maxEffectStacks)
				effectCurrentStacks = statusEffectsData.maxEffectStacks;

			if (statusEffectsData.hasStacks)
				effectValue = statusEffectsData.effectValue * effectCurrentStacks;
			else
				effectValue = statusEffectsData.effectValue;
		}

		public void UpdateRectTransform(int index)
		{
			int movePosX;
			int movePosY;

			if (index < 6)
			{
				movePosX = (20 * index) + 4;
				movePosY = 0;
			}
			else
			{
				movePosX = (20 * (index - 6)) + 4;
				movePosY = -19;
			}

			rectTransform.anchoredPosition = new Vector2(movePosX, movePosY);
		}

		public void OnNewTurnStart()
		{
			if (StatusEffectsData.isDoT)
				ApplyDoTDamage();

			if (StatusEffectsData.hasLifetime)
			{
				effectLifetimeLeft--;

				if (effectLifetimeLeft == 0)
					statusEffectsHandler.RemoveStatusEffect(this);
			}
		}
		void ApplyDoTDamage()
		{
			entity.RecieveDamage(new(entity, false, true, (int)effectValue));
		}

		public string CreateInGameDescription()
		{
			string description = effectDescription;

			if (StatusEffectsData.hasStacks)
			{
				if (effectValue > 0)
					description += $"\n+{effectCurrentStacks} stacks of {effectName} (max:{StatusEffectsData.maxEffectStacks})";
				else
					description += $"\n{effectCurrentStacks} stacks of {effectName} (max:-{StatusEffectsData.maxEffectStacks})";
			}

			if (StatusEffectsData.effectStatModifierType != StatType.noType)
			{
				if (StatusEffectsData.effectValue > 0)
				{
					if (StatusEffectsData.effectStatModifierType == StatType.damageDealt)
						description += $"\n+{effectValue * 100}% damage dealt";

					else if (StatusEffectsData.effectStatModifierType == StatType.damageRecieved)
						description += $"\n+{effectValue * 100}% damage recieved";

					else if (StatusEffectsData.effectStatModifierType == StatType.damageBonus)
						description += $"\n+{effectValue} damage (max:{StatusEffectsData.effectValue * StatusEffectsData.maxEffectStacks})";

					else if (StatusEffectsData.effectStatModifierType == StatType.blockBonus)
						description += $"\n+{effectValue} block (max:{StatusEffectsData.effectValue * StatusEffectsData.maxEffectStacks})";
				}
				else
				{
					if (StatusEffectsData.effectStatModifierType == StatType.damageDealt)
						description += $"\n{effectValue * 100}% damage dealt";

					else if (StatusEffectsData.effectStatModifierType == StatType.damageRecieved)
						description += $"\n{effectValue * 100}% damage recieved";

					else if (StatusEffectsData.effectStatModifierType == StatType.damageBonus)
						description += $"\n{effectValue} damage (max:{StatusEffectsData.effectValue * StatusEffectsData.maxEffectStacks})";

					else if (StatusEffectsData.effectStatModifierType == StatType.blockBonus)
						description += $"\n{effectValue} block (max:{StatusEffectsData.effectValue * StatusEffectsData.maxEffectStacks})";
				}
			}

			if (StatusEffectsData.isDoT)
				description += $"\nDeals {effectValue} damage every turn";

			if (StatusEffectsData.hasLifetime)
				description += $"\nLasts for {effectLifetimeLeft} more turns";

			return description;
		}
	}
}