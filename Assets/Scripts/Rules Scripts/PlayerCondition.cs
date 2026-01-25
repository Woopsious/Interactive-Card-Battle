using UnityEngine;

namespace Woopsious
{
	[CreateAssetMenu(fileName = "PlayerCondition", menuName = "ScriptableObjects/Rule/PlayerCondition")]
	public class PlayerCondition : RuleCondition
	{
		public EntityData conditionalEntityType;

		public override bool Evaluate(RuleContext ruleContext)
		{
			if (ruleContext.OutcomeEntity.EntityData.isPlayer && ruleContext.ConditionalEntity.EntityData == conditionalEntityType)
				return true;
			else 
				return false;
		}
	}
}
