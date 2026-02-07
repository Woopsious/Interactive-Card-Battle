using System.Collections.Generic;
using UnityEngine;

namespace Woopsious
{
	[CreateAssetMenu(fileName = "PlayerCondition", menuName = "ScriptableObjects/Rule/PlayerCondition")]
	public class PlayerCondition : RuleCondition
	{
		public List<EntityData> sourceEntityTypes;

		public override bool Evaluate(RuleContext ruleContext)
		{
			if (!ruleContext.SourceEntity.EntityData.isPlayer) 
				return false;

			foreach (EntityData entityData in sourceEntityTypes)
			{
				if (ruleContext.SourceEntity.EntityData == entityData)
					return true;
			}

			return false;
		}
	}
}
