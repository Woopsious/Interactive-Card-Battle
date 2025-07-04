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

		[Header("Player Only Cards")]
		public int maxCardsUsedPerTurn;
		public int maxDamageCardsUsedPerTurn;
		public int maxNonDamageCardsUsedPerTurn;
		public int maxReplaceableCardsPerTurn;

		public List<CardData> cards = new();

		[Header("Enemy Move Set Order")]
		public List<MoveSetData> moveSetOrder = new();
	}

}