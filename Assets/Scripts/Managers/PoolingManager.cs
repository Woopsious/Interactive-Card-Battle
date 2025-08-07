using System.Collections.Generic;
using UnityEngine;

namespace Woopsious
{
	public class PoolingManager : MonoBehaviour
	{
		public static PoolingManager instance;

		PlayerEntity playerEntity;

		public GameObject inactiveEntitiesParent;
		readonly int entitiesToSave = 10;
		List<Entity> inactiveEntities = new List<Entity>();

		readonly int cardsToSave = 10;
		List<CardUi> inactiveCards = new List<CardUi>();

		private void Awake()
		{
			instance = this;
		}

		void OnEnable()
		{
			Entity.OnEntityDeath += AddEntityToInactiveList;
		}
		void OnDisable()
		{
			Entity.OnEntityDeath -= AddEntityToInactiveList;
		}

		public Entity RequestEntity()
		{
			Entity entity = null;

			if (inactiveEntities.Count > 0)
			{
				entity = inactiveEntities[0];
				inactiveEntities.RemoveAt(0);
			}

			return entity;
		}
		void AddEntityToInactiveList(Entity entity)
		{
			entity.transform.SetParent(inactiveEntitiesParent.transform);

			if (inactiveEntities.Count > entitiesToSave)
			{
				Destroy(entity.gameObject);
			}
			else
			{
				if (inactiveEntities.Contains(entity)) return;
				inactiveEntities.Add(entity);
			}
		}
	}
}
