using UnityEngine;
using Woopsious.AbilitySystem;

namespace Woopsious
{
	// RuleCondition base class
	public abstract class RuleCondition : ScriptableObject
	{
		public abstract bool Evaluate(Entity entity);
	}
}
