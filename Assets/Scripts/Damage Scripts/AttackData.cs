using System;
using System.Collections.Generic;
using UnityEngine;
using Woopsious.AbilitySystem;
using Woopsious.ComplexStats;

namespace Woopsious
{
	[CreateAssetMenu(fileName = "AttackData", menuName = "ScriptableObjects/AttackData")]
	[Serializable]
	public class AttackData : ScriptableObject
	{
		[Header("Attack Info")]
		public string attackName;
		public string attackDescription;

		[Header("Special Settings")]
		public bool isPlayerAttack;
		public bool offensive;
		public int energyCost;
		[Range(0f, 1f)]
		public float attackUseChance;

		[Header("Special Player Card Drawing")]
		public bool canDrawExtraCards;
		public StatData drawExtraCardData;

		[Header("Special Player Card Playing")]
		public bool canPlayExtraCards;
		public StatData playExtraCardData;

		[Header("Dummy Card Settings")]
		[Tooltip("set to true to make this attack force add unplayable dummy cards to players next hand")]
		public bool addDummyCardsForEffects;
		[Tooltip("status effect dummy card will be based on, using its name and description")]
		public List<StatusEffectsData> effectDummyCards;

		[Header("Damage Data")]
		public DamageData DamageData;
	}
}
