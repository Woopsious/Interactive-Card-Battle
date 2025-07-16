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

		public Canvas canvas;

		[Header("Object Prefabs")]
		public GameObject EntityPrefab;
		public GameObject PlayerPrefab;
		public GameObject cardPrefab;

		[Header("Map Node Scriptable Objects")]
		public List<MapNodeData> MapNodeDataTypes = new();
		private int totalMapNodeSpawnChance;

		[Header("Entity Scriptable Objects")]
		public List<EntityData> entityDataTypes = new();

		[Header("Enemies To Spawn")]
		public int numberOfEnemiesToSpawn;
		private float widthOfEntities;

		public static event Action<PlayerEntity> OnPlayerSpawned;
		public static event Action<Entity> OnEnemySpawned;

		void Awake()
		{
			instance = this;
			widthOfEntities = EntityPrefab.GetComponent<RectTransform>().sizeDelta.x;
			if (canvas == null)
				Debug.LogError("canvas ref null, assign it in scene");

			foreach (MapNodeData node in MapNodeDataTypes)
				totalMapNodeSpawnChance += (int)node.nodeSpawnChance;
		}

		//card battle setup
		public static async Task SpawnEntitiesForCardBattle(MapNode mapNode)
		{
			instance.RandomizeEnemySpawnAmount();

			await SpawnPlayer();
			await SpawnEnemies(mapNode);
		}
		void RandomizeEnemySpawnAmount()
		{
			numberOfEnemiesToSpawn = UnityEngine.Random.Range(2, 5);
		}
		public static async Task DebugSpawnAllEntities()
		{
			await SpawnPlayer();

			float spacing = (Screen.width - instance.entityDataTypes.Count * instance.widthOfEntities) / (instance.entityDataTypes.Count + 1);

			for (int i = 0; i < instance.entityDataTypes.Count; i++)
			{
				Entity spawnedEntity = Instantiate(instance.EntityPrefab, instance.canvas.transform).GetComponent<Entity>();
				spawnedEntity.entityData = instance.entityDataTypes[i];
				instance.SetEnemyPosition(spawnedEntity.GetComponent<RectTransform>(), spacing, i + 1);
				OnEnemySpawned?.Invoke(spawnedEntity);

				Debug.LogError(spawnedEntity.entityData.entityName + " cost: " + spawnedEntity.entityData.GetEntityCost());
			}
		}

		//entity spawning
		public static Task SpawnPlayer()
		{
			PlayerEntity player = Instantiate(instance.PlayerPrefab, instance.canvas.transform).GetComponent<PlayerEntity>();
			OnPlayerSpawned?.Invoke(player);

			return Task.CompletedTask;
		}
		public static async Task SpawnEnemies(MapNode mapNode)
		{
			List<Entity> spawnedEntites = new();

			bool spawnEnemies = true;
			while (spawnEnemies)
			{
				Entity spawnedEntity = Instantiate(instance.EntityPrefab, instance.canvas.transform).GetComponent<Entity>();
				spawnedEntity.entityData = instance.GetWeightedEntitySpawn(mapNode);

				await mapNode.BuyEnemy(spawnedEntity.entityData);
				spawnedEntites.Add(spawnedEntity);
				OnEnemySpawned?.Invoke(spawnedEntity);

				if (spawnedEntites.Count >= 5 || mapNode.entityBudget < mapNode.cheapistEnemyCost)
					spawnEnemies = false;
			}

			instance.SetEnemyPositions(spawnedEntites);
		}
		EntityData GetWeightedEntitySpawn(MapNode mapNode)
		{
			float roll = UnityEngine.Random.Range(0, mapNode.totalPossibleEntitiesSpawnChance);
			float cumulativeChance = 0;

			foreach (EntityData entity in mapNode.PossibleEntities)
			{
				cumulativeChance += entity.entitySpawnChance;

				if (roll <= cumulativeChance)
					return entity;
			}

			Debug.LogError("Failed to get weighted enemy to spawn, spawning default");
			return entityDataTypes[0];
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
			CardUi card = Instantiate(instance.cardPrefab).GetComponent<CardUi>();
			return card;
		}

		//types of cards to get
		public static CardData GetRandomCard(List<CardData> cardDataList)
		{
			CardData cardData = cardDataList[UnityEngine.Random.Range(0, cardDataList.Count)];
			return cardData;
		}
	}
}