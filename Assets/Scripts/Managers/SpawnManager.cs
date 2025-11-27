using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

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
		public static async Task DebugSpawnEntities(List<EntityData> entitiesToSpawn)
		{
			await SpawnPlayer();

			List<Entity> spawnedEntites = new();

			foreach (EntityData entityData in entitiesToSpawn)
			{
				Entity entity = ObjectPoolingManager.RequestEntity();

				if (entity == null)
					entity = Instantiate(instance.EntityPrefab, CardCombatUi.instance.spawnedEntitiesTransform).GetComponent<Entity>();

				entity.InitializeEntity(entityData);
				entity.transform.SetParent(CardCombatUi.instance.spawnedEntitiesTransform);
				entity.gameObject.SetActive(true);

				spawnedEntites.Add(entity);
				OnEnemySpawned?.Invoke(entity);
			}

			instance.SetEnemyPositions(spawnedEntites);
		}

		//entity spawning
		public static Task SpawnPlayer()
		{
			PlayerEntity player = Instantiate(instance.PlayerPrefab, CardCombatUi.instance.spawnedEntitiesTransform).GetComponent<PlayerEntity>();
			EntityData playerClass = GameManager.PlayerClass;

			player.InitializeEntity(playerClass);
			player.transform.SetAsFirstSibling();
			OnPlayerSpawned?.Invoke(player);

			return Task.CompletedTask;
		}
		public static async Task SpawnEnemies(MapNode mapNode)
		{
			List<Entity> spawnedEntites = new();

			bool spawnEnemies = true;
			while (spawnEnemies)
			{
				if (spawnedEntites.Count >= 5 || mapNode.entityBudget < mapNode.cheapistEnemyCost)
					break;

				Entity entity = ObjectPoolingManager.RequestEntity();

				if (entity == null)
					entity = Instantiate(instance.EntityPrefab, CardCombatUi.instance.spawnedEntitiesTransform).GetComponent<Entity>();

				entity.InitializeEntity(instance.GetWeightedEntitySpawn(mapNode));
				entity.transform.SetParent(CardCombatUi.instance.spawnedEntitiesTransform);
				entity.gameObject.SetActive(true);

				await mapNode.BuyEnemyAndUpdatePossibleEntities(entity.EntityData);
				spawnedEntites.Add(entity);
				OnEnemySpawned?.Invoke(entity);
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

			Debug.LogError("Failed to get weighted enemy to spawn");
			return null;
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
		public static CardHandler SpawnCard()
		{
			CardHandler card = ObjectPoolingManager.RequestCard();

			if (card == null)
				card = Instantiate(instance.cardPrefab).GetComponent<CardHandler>();

			return card;
		}

		//types of cards to get
		public static AttackData GetWeightedPlayerCardReward(float totalCardDropChance)
		{
			float roll = (float)(instance.systemRandom.NextDouble() * totalCardDropChance);
			float cumulativeChance = 0;

			foreach (AttackData card in GameManager.PlayerClass.collectableCards)
			{
				cumulativeChance += card.CardDropChance();

				if (roll <= cumulativeChance)
					return card;
			}

			Debug.LogError("Failed to get weighted card reward");
			return null;
		}
		public static AttackData GetWeightedPlayerCardDraw(List<AttackData> playerCardDeck, float totalCardDrawChance)
		{
			float roll = (float)(instance.systemRandom.NextDouble() * totalCardDrawChance);
			float cumulativeChance = 0;

			foreach (AttackData card in playerCardDeck)
			{
				cumulativeChance += card.CardDrawChance();

				if (roll <= cumulativeChance)
					return card;
			}

			Debug.LogError("Failed to get weighted card draw");
			return null;
		}
	}
}