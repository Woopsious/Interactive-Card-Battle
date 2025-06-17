using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EntityData", menuName = "ScriptableObjects/Entity")]
public class EntityData : ScriptableObject
{
	[Header("Entity Info")]
	public string entityName;
	public string entityDescription;

	public bool isPlayer;

	[Header("Entity Health")]
	public int maxHealth;

	[Header("Player Only Cards")]
	public int maxCardsUsedPerTurn;
	public int maxDamageCardsUsedPerTurn;
	public int maxNonDamageCardsUsedPerTurn;
	public int maxReplaceableCardsPerTurn;

	public List<CardData> cards = new();

	[Header("Enemy Attacks + Order")]
	public List<AttackData> attacks = new();
}
