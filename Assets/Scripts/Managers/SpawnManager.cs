using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

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

		[Header("Scriptable Object Data")]
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
		}

		//card battle setup
		public static async Task SpawnEntitiesForCardBattle()
		{
			instance.RandomizeEnemySpawnAmount();

			await SpawnPlayer();
			await SpawnEnemies();
		}
		void RandomizeEnemySpawnAmount()
		{
			numberOfEnemiesToSpawn = UnityEngine.Random.Range(2, 5);
		}

		//entity spawning
		public static Task SpawnPlayer()
		{
			PlayerEntity player = Instantiate(instance.PlayerPrefab, instance.canvas.transform).GetComponent<PlayerEntity>();
			OnPlayerSpawned?.Invoke(player);

			return Task.CompletedTask;
		}
		public static Task SpawnEnemies()
		{
			float spacing = (Screen.width - instance.numberOfEnemiesToSpawn * instance.widthOfEntities) / (instance.numberOfEnemiesToSpawn + 1);

			for (int i = 0; i < instance.numberOfEnemiesToSpawn; i++)
			{
				Entity spawnedEntity = Instantiate(instance.EntityPrefab, instance.canvas.transform).GetComponent<Entity>();
				spawnedEntity.entityData = instance.GetRandomEnemyToSpawn();
				instance.SetEnemyPosition(spawnedEntity.GetComponent<RectTransform>(), spacing, i + 1);
				OnEnemySpawned?.Invoke(spawnedEntity);
			}

			return Task.CompletedTask;
		}

		void SetEnemyPosition(RectTransform rectTransform, float spacing, int index)
		{
			float offset = widthOfEntities * (index - 1);
			rectTransform.anchoredPosition = new Vector2(spacing * index + offset, 400);
		}
		EntityData GetRandomEnemyToSpawn()
		{
			EntityData entityData = entityDataTypes[UnityEngine.Random.Range(0, entityDataTypes.Count)];
			return entityData;
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