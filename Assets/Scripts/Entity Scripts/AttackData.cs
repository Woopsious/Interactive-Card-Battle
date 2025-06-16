using UnityEngine;
using static DamageData;

[CreateAssetMenu(fileName = "AttackData", menuName = "ScriptableObjects/AttackData")]
public class AttackData : ScriptableObject
{
	[Header("Attack Info")]
	public string attackName;
	public string attackDescription;

	[Header("Attack damage")]
	public int damage;

	public DamageType damageType;

	[Header("Attack use rules")]
	public int attackCooldownTurns;
	[Range(0f, 1f)]
	public float attackUseChance;

	public string CreateDescription()
	{
		string attackDescription = this.attackDescription;
		attackDescription += "\n\n";

		if (damageType == DamageType.block)
			attackDescription += "Blocks " + damage + " damage";
		else if (damageType ==DamageType.heal)
			attackDescription += "Heals " + damage + " damage";
		else if (damageType == DamageType.physical)
			attackDescription += "Deals " + damage + " physical damage";

		return attackDescription;
	}
}
