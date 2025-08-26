using System.Collections.Generic;
using UnityEngine;

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
		public void AddStatusEffect(StatusEffectsData statusEffectData)
		{
			StatusEffect newEffect = new(entity, this, statusEffectData);

			//if (currentStatusEffects.Count == 0) //no effect to check so add
				//currentStatusEffects.Add(newEffect);

			foreach(StatusEffect effect in currentStatusEffects) //re apply match effects
			{
				if (effect.StatusEffectsData == newEffect.StatusEffectsData)
				{
					effect.ReApplyStatusEffect(newEffect.StatusEffectsData);
					return;
				}
			}

			entity.AddStatModifier(new(statusEffectData.effectValue, statusEffectData.effectStatModifierType));
			currentStatusEffects.Add(newEffect);
		}
		public void RemoveStatusEffect(StatusEffect effectToRemove)
		{
			if (currentStatusEffects.Contains(effectToRemove))
			{
				StatusEffectsData statusEffectData = effectToRemove.StatusEffectsData;
				entity.RemoveStatModifier(new(statusEffectData.effectValue, statusEffectData.effectStatModifierType));
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