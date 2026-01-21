using UnityEngine;
using Woopsious.AbilitySystem;

namespace Woopsious
{
	// Entity condition example
	[CreateAssetMenu(fileName = "EntityCondition", menuName = "ScriptableObjects/Rule/EntityCondition")]
	public class EntityCondition : RuleCondition
	{
		public EntityData targetEntity;

		public override bool Evaluate(RuleContext ruleContext)
		{
			return ruleContext.ConditionalEntity.EntityData == targetEntity;
		}
	}
}
