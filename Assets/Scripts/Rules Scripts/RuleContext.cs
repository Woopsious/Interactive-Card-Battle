using UnityEngine;
using Woopsious;

namespace Woopsious
{
	public class RuleContext
	{
		public string RuleName { get; private set; }
		public Entity ConditionalEntity { get; private set; }
		public Entity OutcomeEntity { get; private set; }
		public DamageData DamageDataContext { get; private set; }

		public RuleContext(Entity conditionalEntity, Entity outcomeEntity)
		{
			ConditionalEntity = conditionalEntity;
			OutcomeEntity = outcomeEntity;
			DamageDataContext = null;
		}
		public RuleContext(Entity conditionalEntity, Entity outcomeEntity, DamageData damageDataContext)
		{
			ConditionalEntity = conditionalEntity;
			OutcomeEntity = outcomeEntity;
			DamageDataContext = damageDataContext;
		}
	}
}
