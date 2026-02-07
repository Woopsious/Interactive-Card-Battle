using System.Collections.Generic;
using UnityEngine;

namespace Woopsious
{
	// Entity condition example
	[CreateAssetMenu(fileName = "EntityCondition", menuName = "ScriptableObjects/Rule/EntityCondition")]
	public class EntityCondition : RuleCondition
	{
		public bool useListVersion;
		public bool sourceCheckOnly;

		public EntityData sourceEntityType;
		public EntityData targetEntityType;

		public List<EntityData> sourceEntityTypes = new();
		public List<EntityData> targetEntityTypes = new();

		public override bool Evaluate(RuleContext ruleContext)
		{
			if (!useListVersion)
				return EvaluateSingle(ruleContext);
			else
				return EvaluateList(ruleContext);
		}

		private bool EvaluateSingle(RuleContext ruleContext)
		{
			if (ruleContext.SourceEntity == ruleContext.TargetEntity) return false;

			if (sourceCheckOnly && ruleContext.SourceEntity.EntityData == sourceEntityType)
				return true;
			else
			{
				if (ruleContext.SourceEntity.EntityData == sourceEntityType &&
						ruleContext.TargetEntity.EntityData == targetEntityType)
					return true;
				else
					return false;
			}
		}
		private bool EvaluateList(RuleContext ruleContext)
		{
			if (ruleContext.SourceEntity == ruleContext.TargetEntity) return false;

			if (sourceCheckOnly)
			{
				foreach (EntityData sourceEntityData in sourceEntityTypes)
				{
					if (ruleContext.SourceEntity.EntityData == sourceEntityData)
						return true;
				}

				return false;
			}
			else
			{
				foreach (EntityData sourceEntityData in sourceEntityTypes)
				{
					foreach (EntityData targetEntityData in targetEntityTypes)
					{
						if (ruleContext.SourceEntity.EntityData == sourceEntityData && 
							ruleContext.TargetEntity.EntityData == targetEntityData)
							return true;
					}
				}

				return false;
			}
		}
	}
}
