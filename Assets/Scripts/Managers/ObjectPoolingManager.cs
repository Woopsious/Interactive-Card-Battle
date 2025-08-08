using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Woopsious
{
	public class ObjectPoolingManager : MonoBehaviour
	{
		public static ObjectPoolingManager instance;

		public GameObject inactiveEntitiesParent;
		readonly int entitiesToSave = 10;
		List<Entity> inactiveEntities = new List<Entity>();

		public GameObject inactiveCardsParent;
		readonly int cardsToSave = 10;
		List<CardUi> inactiveCards = new List<CardUi>();

		private void Awake()
		{
			instance = this;
		}

		void OnEnable()
		{
			Entity.OnEntityDeath += AddEntityToInactiveList;
			CardHandler.OnCardUsed += AddCardToInactiveList;
		}
		void OnDisable()
		{
			Entity.OnEntityDeath -= AddEntityToInactiveList;
			CardHandler.OnCardUsed -= AddCardToInactiveList;
		}

		//entity object pooling
		public static Entity RequestEntity()
		{
			Entity entity = null;

			if (instance.inactiveEntities.Count > 0)
			{
				entity = instance.inactiveEntities[0];
				instance.inactiveEntities.RemoveAt(0);
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

		//card object pooling
		public static CardUi RequestCard()
		{
			CardUi card = null;

			if (instance.inactiveCards.Count > 0)
			{
				card = instance.inactiveCards[0];
				instance.inactiveCards.RemoveAt(0);
			}

			return card;
		}
		void AddCardToInactiveList(CardUi card)
		{
			card.transform.SetParent(inactiveCardsParent.transform);

			if (inactiveCards.Count > cardsToSave)
			{
				Destroy(card.gameObject);
			}
			else
			{
				if (inactiveCards.Contains(card)) return;
				inactiveCards.Add(card);
			}
		}
	}
}
