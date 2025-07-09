using System;
using System.Collections.Generic;
using UnityEngine;

namespace Woopsious
{
	[CreateAssetMenu(fileName = "EntityData", menuName = "ScriptableObjects/Entity")]
	public class EntityData : ScriptableObject
	{
		[Header("Entity Info")]
		public string entityName;
		public string entityDescription;
		public bool isPlayer;

		[Header("Entity Health")]
		public int maxHealth;
		[Range(0f, 1f)]
		[Tooltip("If health percentage drops below, entity will use heal over other moves in its move set")]
		public float minHealPercentage;

		[Header("Player Cards")] //shown as player
		[HideInInspector] public int maxCardsUsedPerTurn;
		[HideInInspector] public int maxDamageCardsUsedPerTurn;
		[HideInInspector] public int maxNonDamageCardsUsedPerTurn;
		[HideInInspector] public int maxReplaceableCardsPerTurn;

		public List<CardData> cards = new();

		[Header("Enemy Move Set Order")] //shown as non player
		public List<MoveSetData> moveSetOrder = new();
	}

	[Serializable]
	public class MoveSetData
	{
		public List<AttackData> moveSetMoves = new();
	}
}