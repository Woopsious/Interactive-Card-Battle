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

	[Header("Entity Cards")]
	public List<CardData> cards = new();
}
