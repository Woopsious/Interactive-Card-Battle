using System;
using System.Collections.Generic;
using UnityEngine;
using static Woopsious.EntityData;
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
			none = 0, slime = 1, beast = 2, humanoid = 4, construct = 8, undead = 16, Abberration = 32
		}
		public LandTypes foundInLandTypes;
		public LandModifiers foundWithLandModifiers;
		[Range(0f, 1f)]
		public float entitySpawnChance;
		public bool eliteEnemy;

		[Header("Entity Stats")]
		public int maxHealth;
		public int baseBlock;
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

		[Header("Player Only Stats")]
		public int baseEnergy;
		public int maxReplaceableCardsPerTurn;
		public int initialCardDrawAmount;

		[Header("Player Deck Info")]
		[Header("Player Starting Deck")]
		public List<AttackData> cards = new();

		[Header("Collectable Cards")]
		public List<AttackData> collectableCards = new();

		[Header("Enemy Move Set Order")] //shown as non player
		public List<MoveSetData> moveSetOrder = new();

		[Header("Entity Audio")]
		public AudioClip hitSfx;

		//set info for enemy entities
		public string CreateEntityInfo()
		{
			string entityInfo = "";

			entityInfo += entityName + "\n\n";

			entityInfo += $"Health: {maxHealth}\nBase Block: {baseBlock}\n\n";

			entityInfo += CreateFoundLandTypesAndModifiersInfo();

			entityInfo += $"\n\nSpawn Chance: {entitySpawnChance * 100}";

			return entityInfo;
		}
		string CreateFoundLandTypesAndModifiersInfo()
		{
			string entityInfo = "";
			entityInfo += $"Found in following land types:\n";
			entityInfo += RichTextManager.GetLandTypesTextColour(foundInLandTypes);

			entityInfo += "\nFound with following land modifiers:\n";
			entityInfo += RichTextManager.GetLandModifiersTextColour(foundWithLandModifiers);

			return entityInfo;
		}

		//set info for player classes
		public string CreatePlayerClassInfo()
		{
			string classInfo = "";

			classInfo += entityName + "\n\n";

			classInfo += $"Health: {maxHealth}\n";
			classInfo += $"Block: {baseBlock}\n";
			classInfo += $"Energy: {baseEnergy}\n\n";

			classInfo += "STARTING CARDS\n";
			classInfo += SetPlayerClassCardsInfo() + "\n\n";

			classInfo += "Class specialty ";
			classInfo += ExplainPlayerClassGimmick();

			return classInfo;
		}
		string SetPlayerClassCardsInfo()
		{
			int multiCardsCount = 0;
			int attackCardsCount = 0;
			int blockCardsCount = 0;
			int healCardsCount = 0;

			foreach (AttackData attackData in cards)
			{
				DamageData damageData = attackData.DamageData;
				if (damageData.DamageValue != 0 && damageData.BlockValue != 0 || damageData.DamageValue != 0 && damageData.HealValue != 0)
					multiCardsCount++;
				else if (damageData.DamageValue != 0)
					attackCardsCount++;
				else if (damageData.BlockValue != 0)
					blockCardsCount++;
				else if (damageData.HealValue != 0)
					healCardsCount++;
			}

			string cardsInfo = "";
			cardsInfo += $"Multi Cards: {multiCardsCount}\nAttack Cards: {attackCardsCount}\nBlock Cards: {blockCardsCount}\nHeal Cards: {healCardsCount}";
			return cardsInfo;
			//at some point also add the dispalying of each type of starting cards details
		}
		string ExplainPlayerClassGimmick()
		{
			string classGimmickInfo = "";

			if (playerClass == PlayerClass.Mage)
			{
				classGimmickInfo += $"\"Potent Magic\"\n Has a {chanceOfDoubleDamage}% to deal double the damage of a card";
			}
			else if (playerClass == PlayerClass.Ranger)
			{
				classGimmickInfo += $"\"Resourceful\"\n Heals for {healOnKillPercentage}% of health on killing an enemy";
			}
			else if (playerClass == PlayerClass.Rogue)
			{
				classGimmickInfo += $"\"Trickster\"\n Reflects {damageReflectedPercentage}% of damage recieved onto attacker";
			}
			else if (playerClass == PlayerClass.Warrior)
			{
				classGimmickInfo += $"\"Stalwart\"\n Has a permanent {baseBlock} block";
			}

			return classGimmickInfo;
		}

		//calc entity cost at runtime
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
				case EnemyTypes.Abberration:
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