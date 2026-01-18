using UnityEngine;

namespace Woopsious
{
	// RuleOutcome base class
	public abstract class RuleOutcome : ScriptableObject
	{
		public abstract void Apply(Entity entity);
	}
}
