using UnityEngine;
using Woopsious;

namespace Woopsious
{
	public class RuleContext
	{
		public Entity SourceEntity { get; private set; }
		public Entity TargetEntity { get; private set; }
		public DamageData DamageDataContext { get; private set; }

		public RuleContext(Entity sourceEntity, Entity targetEntity)
		{
			SourceEntity = sourceEntity;
			TargetEntity = targetEntity;
			DamageDataContext = null;
		}
		public RuleContext(Entity sourceEntity, Entity targetEntity, DamageData damageDataContext)
		{
			SourceEntity = sourceEntity;
			TargetEntity = targetEntity;
			DamageDataContext = damageDataContext;
		}
	}
}
