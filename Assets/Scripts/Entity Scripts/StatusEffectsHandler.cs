using System.Collections.Generic;
using UnityEngine;
using static Woopsious.Stat;

namespace Woopsious
{
	public class StatusEffectsHandler : MonoBehaviour
	{
		Entity entity;
		public List<StatusEffect> currentStatusEffects = new();

		void Awake()
		{
			entity = GetComponent<Entity>();
		}

		void OnEnable()
		{
			TurnOrderManager.OnNewTurnEvent += OnNewTurnStart;
		}
		void OnDisable()
		{
			TurnOrderManager.OnNewTurnEvent += OnNewTurnStart;
		}

		//adding/removing effects
		public void ClearStatusEffects()
		{
			for (int i = currentStatusEffects.Count - 1; i >= 0; i--)
				RemoveStatusEffect(currentStatusEffects[i]);
		}
		public void AddStatusEffects(List<StatusEffectsData> statusEffectsData)
		{
			List<StatusEffectsData> effectsToApply = new(statusEffectsData);

			for (int i = currentStatusEffects.Count - 1;  i >= 0; i--) //re apply matching status effects
			{
				for (int j = effectsToApply.Count - 1; j >= 0; j--)
				{
					if (currentStatusEffects[i].StatusEffectsData == effectsToApply[j])
					{
						StatType statType = currentStatusEffects[i].StatusEffectsData.effectStatModifierType;
						entity.RemoveStatModifier(currentStatusEffects[i].effectValue, statType);

						currentStatusEffects[i].ReApplyStatusEffect(effectsToApply[j]);
						entity.AddStatModifier(currentStatusEffects[i].effectValue, statType);
						effectsToApply.RemoveAt(j);
					}
				}
			}

			foreach (StatusEffectsData effectToApply in effectsToApply) //add new status effects
			{
				StatusEffect newStatusEffect = new(entity, this, effectToApply);
				currentStatusEffects.Add(newStatusEffect);
				entity.AddStatModifier(newStatusEffect.effectValue, effectToApply.effectStatModifierType);
			}

			//if (currentStatusEffects.Count == 0) //no effect to check so add
				//currentStatusEffects.Add(newEffect);
		}
		public void RemoveStatusEffect(StatusEffect effectToRemove)
		{
			if (currentStatusEffects.Contains(effectToRemove))
			{
				StatusEffectsData statusEffectData = effectToRemove.StatusEffectsData;
				entity.RemoveStatModifier(statusEffectData.effectValue, statusEffectData.effectStatModifierType);
				currentStatusEffects.Remove(effectToRemove);
			}
			else
				Debug.LogError("tried to remove status effect that doesnt exist");
		}

		//apply effect modifiers

		//event listeners
		void OnNewTurnStart(Entity entity)
		{
			if (this.entity != entity) return;

			for (int i = currentStatusEffects.Count - 1; i >= 0; i--)
				currentStatusEffects[i].OnNewTurnStart();
		}
	}
}