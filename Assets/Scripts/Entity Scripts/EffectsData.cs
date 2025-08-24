using System;
using UnityEngine;
using static Woopsious.DamageData;

namespace Woopsious
{
	[CreateAssetMenu(fileName = "EffectData", menuName = "ScriptableObjects/EffectsData")]
	[Serializable]
	public class EffectsData : ScriptableObject
	{
		[Header("Attack Info")]
		public string effectName;
		public string effectDescription;

		public string CreateDescription()
		{
			string description = effectDescription;
			description += "\n";

			return description;
		}
	}
}
