using System;
using System.Collections.Generic;
using UnityEngine;
using static Woopsious.MapNodeData;

namespace Woopsious
{
	[CreateAssetMenu(fileName = "EntityData", menuName = "ScriptableObjects/Entity")]
	public class EntityData : ScriptableObject
	{
		[Header("Entity Info")]
		public string entityName;
		public string entityDescription;
		public bool isPlayer;

		[Header("Entity Special Info")]
		public EnemyTypes enemyType;
		[Flags]
		public enum EnemyTypes : int
		{
			none = 0, slime = 1, beast = 2, humanoid = 4, construct = 8, undead = 16, Abberrations = 32
		}
		public LandTypes foundInLandTypes;
		[Range(0f, 100f)]
		public float entitySpawnChance;
		public bool eliteEnemy;

		[Header("Entity Stats")]
		public int maxHealth;
		[Range(0f, 1f)]
		[Tooltip("If health percentage drops below, entity will use heal over other moves in its move set")]
		public float minHealPercentage;

		[Header("Player Class")]
		public PlayerClass playerClass;
		public enum PlayerClass
		{
			NotPlayer, Mage, Ranger, Rogue, Warrior
		}

		//player class gimmicks
		[Range(0f, 100f)]
		public float chanceOfDoubleDamage; //Mage
		[Range(0f, 100f)]
		public float healOnKillPercentage; //Ranger
		[Range(0f, 100f)]
		public float damageReflectedPercentage; //Rogue
		public int extraBlockPerTurn; //Warrior

		[Header("Player Cards")]
		public int maxCardsUsedPerTurn;
		public int maxDamageCardsUsedPerTurn;
		public int maxNonDamageCardsUsedPerTurn;
		public int maxReplaceableCardsPerTurn;

		public List<CardData> cards = new();

		[Header("Enemy Move Set Order")] //shown as non player
		public List<MoveSetData> moveSetOrder = new();

		public int GetEntityCost()
		{
			int entityCost = (int)(maxHealth * GetEnemyTypeCostModifier(enemyType));

			if (eliteEnemy)
				entityCost = (int)(entityCost * 1.25f);

			return entityCost;
		}
		float GetEnemyTypeCostModifier(EnemyTypes enemyType)
		{
			switch (enemyType)
			{
				case EnemyTypes.none:
				Debug.LogError("enemy type not set");
				return 1f;
				case EnemyTypes.slime:
				return 1f;
				case EnemyTypes.beast:
				return 1.1f;
				case EnemyTypes.humanoid:
				return 1.2f;
				case EnemyTypes.construct:
				return 1.3f;
				case EnemyTypes.undead:
				return 1.5f;
				case EnemyTypes.Abberrations:
				return 1.75f;
				default:
				Debug.LogError("failed to match enemy type");
				return 1f;
			}
		}
	}

	[Serializable]
	public class MoveSetData
	{
		public List<AttackData> moveSetMoves = new();
	}
}