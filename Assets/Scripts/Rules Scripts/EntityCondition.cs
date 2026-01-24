using UnityEngine;
using Woopsious.AbilitySystem;

namespace Woopsious
{
	// Entity condition example
	[CreateAssetMenu(fileName = "EntityCondition", menuName = "ScriptableObjects/Rule/EntityCondition")]
	public class EntityCondition : RuleCondition
	{
		public bool conditionalCheckOnly;
		public EntityData conditionalEntityType;
		public EntityData outcomeEntityType;

		public override bool Evaluate(RuleContext ruleContext)
		{
			if (ruleContext.ConditionalEntity == ruleContext.OutcomeEntity) return false;

			if (conditionalCheckOnly && ruleContext.ConditionalEntity.EntityData == conditionalEntityType)
				return true;
			else
			{
				if (ruleContext.ConditionalEntity.EntityData == conditionalEntityType &&
						ruleContext.OutcomeEntity.EntityData == outcomeEntityType)
					return true;
				else
					return false;
			}
		}
	}
}
