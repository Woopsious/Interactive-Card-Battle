using UnityEngine;
using Woopsious.ComplexStats;

namespace Woopsious
{
	// stat outcome
	[CreateAssetMenu(fileName = "StatOutcome", menuName = "ScriptableObjects/Rule/StatOutcome")]
	public class StatOutcome : RuleOutcome
	{
		public StatType statType;
		public StatModifier statModifier;

		public override void Apply(Entity entity)
		{
			entity.AddStatModifier(statType, statModifier);
		}
	}
}
