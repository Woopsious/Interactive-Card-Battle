using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Woopsious.DamageData;

namespace Woopsious
{
	[CreateAssetMenu(fileName = "AttackData", menuName = "ScriptableObjects/AttackData")]
	[Serializable]
	public class AttackData : ScriptableObject
	{
		[Header("Attack Info")]
		public string attackName;
		public string attackDescription;

		[Header("Other Data")]
		public bool offensive;
		public int energyCost;

		[Header("Damage Data")]
		public DamageData DamageData;

		[Header("Attack use rules")]
		[Range(0f, 1f)]
		public float attackUseChance;
	}
}
