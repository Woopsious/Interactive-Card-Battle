using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace Woopsious
{
	public class SpawnManager : MonoBehaviour
	{
		public static SpawnManager instance;

		[Header("Object Prefabs")]
		public GameObject EntityPrefab;
		public GameObject PlayerPrefab;
		public GameObject cardPrefab;

		[Header("Enemies To Spawn")]
		private float widthOfEntities;

		private readonly System.Random systemRandom = new();
		public static event Action<PlayerEntity> OnPlayerSpawned;
		public static event Action<Entity> OnEnemySpawned;

		void Awake()
		{
			instance = this;
			widthOfEntities = EntityPrefab.GetComponent<RectTransform>().sizeDelta.x;
		}

		//card battle setup
		public static async Task SpawnEntitiesForCardBattle(MapNode mapNode)
		{
			await SpawnPlayer();
			await SpawnEnemies(mapNode);
		}
		public static async Task DebugSpawnAllEntities()
		{
			await SpawnPlayer();

			float spacing = (Screen.width - 
				GameManager.instance.enemyDataTypes.Count * instance.widthOfEntities) / (GameManager.instance.enemyDataTypes.Count + 1);

			for (int i = 0; i < GameManager.instance.enemyDataTypes.Count; i++)
			{
				Entity spawnedEntity = Instantiate(instance.EntityPrefab, CardCombatUi.instance.spawnedEntitiesTransform).GetComponent<Entity>();

				spawnedEntity.InitilizeEntity(GameManager.instance.enemyDataTypes[i]);
				instance.SetEnemyPosition(spawnedEntity.GetComponent<RectTransform>(), spacing, i + 1);
				OnEnemySpawned?.Invoke(spawnedEntity);

				Debug.LogError(spawnedEntity.EntityData.entityName + " cost: " + spawnedEntity.EntityData.GetEntityCost());
			}
		}

		//entity spawning
		public static Task SpawnPlayer()
		{
			PlayerEntity player = Instantiate(instance.PlayerPrefab, CardCombatUi.instance.spawnedEntitiesTransform).GetComponent<PlayerEntity>();
			EntityData playerClass = GameManager.PlayerClass;

			player.InitilizeEntity(playerClass);
			OnPlayerSpawned?.Invoke(player);

			return Task.CompletedTask;
		}
		public static async Task SpawnEnemies(MapNode mapNode)
		{
			List<Entity> spawnedEntites = new();

			bool spawnEnemies = true;
			while (spawnEnemies)
			{
				Entity entity = ObjectPoolingManager.RequestEntity();

				if (entity == null)
					entity = Instantiate(instance.EntityPrefab, CardCombatUi.instance.spawnedEntitiesTransform).GetComponent<Entity>();

				entity.InitilizeEntity(instance.GetWeightedEntitySpawn(mapNode));
				entity.transform.SetParent(CardCombatUi.instance.spawnedEntitiesTransform);
				entity.gameObject.SetActive(true);

				await mapNode.BuyEnemy(entity.EntityData);
				spawnedEntites.Add(entity);
				OnEnemySpawned?.Invoke(entity);

				if (spawnedEntites.Count >= 5 || mapNode.entityBudget < mapNode.cheapistEnemyCost)
					spawnEnemies = false;
			}

			instance.SetEnemyPositions(spawnedEntites);
		}
		EntityData GetWeightedEntitySpawn(MapNode mapNode)
		{
			float roll = (float)(systemRandom.NextDouble() * mapNode.totalPossibleEntitiesSpawnChance);
			float cumulativeChance = 0;

			foreach (EntityData entity in mapNode.PossibleEntities)
			{
				cumulativeChance += entity.entitySpawnChance;

				if (roll <= cumulativeChance)
					return entity;
			}

			Debug.LogError("Failed to get weighted enemy to spawn, spawning default");
			return GameManager.instance.enemyDataTypes[0];
		}
		void SetEnemyPositions(List<Entity> entities)
		{
			float spacing = (Screen.width - entities.Count * instance.widthOfEntities) / (entities.Count + 1);

			for (int i = 0; i < entities.Count; i++)
				instance.SetEnemyPosition(entities[i].GetComponent<RectTransform>(), spacing, i + 1);
		}
		void SetEnemyPosition(RectTransform rectTransform, float spacing, int index)
		{
			float offset = widthOfEntities * (index - 1);
			rectTransform.anchoredPosition = new Vector2(spacing * index + offset, 400);
		}

		//card spawning
		public static CardUi SpawnCard()
		{
			CardUi card = ObjectPoolingManager.RequestCard();

			if (card == null)
				card = Instantiate(instance.cardPrefab).GetComponent<CardUi>();

			return card;
		}

		//types of cards to get
		public static AttackData GetRandomCard(List<AttackData> cardDataList)
		{
			AttackData cardData = cardDataList[instance.systemRandom.Next(cardDataList.Count)];
			return cardData;
		}
	}
}