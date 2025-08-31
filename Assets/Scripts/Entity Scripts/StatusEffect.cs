using System;
using UnityEngine;

namespace Woopsious
{
	[Serializable]
	public class StatusEffect
	{
		readonly Entity entity;
		readonly StatusEffectsHandler statusEffectsHandler;
		public StatusEffectsData StatusEffectsData { get; private set; }

		[Header("Effect Info")]
		[SerializeField] public string effectName;
		[SerializeField] public string effectDescription;

		[Header("Effect Runtime")]
		[SerializeField] public int effectCurrentStacks;
		[SerializeField] public int effectLifetimeLeft;
		[SerializeField] public float effectValue;

		public StatusEffect(Entity entity, StatusEffectsHandler effectsHandler, StatusEffectsData statusEffectsData)
		{
			this.entity = entity;
			statusEffectsHandler = effectsHandler;
			StatusEffectsData = statusEffectsData;

			effectName = statusEffectsData.effectName;
			effectDescription = statusEffectsData.CreateInGameDescription();

			effectCurrentStacks = statusEffectsData.effectStacks;
			effectLifetimeLeft = statusEffectsData.effectTurnLifetime;

			if (statusEffectsData.hasStacks)
				effectValue = statusEffectsData.effectValue * effectCurrentStacks;
			else
				effectValue = (int)statusEffectsData.effectValue;
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
				effectValue = (int)statusEffectsData.effectValue;
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
	}
}