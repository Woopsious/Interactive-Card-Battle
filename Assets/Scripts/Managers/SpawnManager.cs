using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
	public static SpawnManager instance;

	public Canvas canvas;

	public GameObject EntityPrefab;
	public GameObject PlayerPrefab;
	public GameObject cardPrefab;

	public List<EntityData> entityDataTypes = new();

	public int numberOfEnemiesToSpawn;
	private float widthOfEntities;

	public static event Action OnStartGame;

	public static event Action<PlayerEntity> OnPlayerSpawned;
	public static event Action<Entity> OnEnemySpawned;

	void Awake()
	{
		instance = this;
		widthOfEntities = EntityPrefab.GetComponent<RectTransform>().sizeDelta.x;
		if (canvas == null)
			Debug.LogError("canvas ref null, assign it in scene");
	}

	private async void Start()
	{
		await SpawnEnemies();
		await SpawnPlayer();

		OnStartGame?.Invoke();
	}

	//entity spawning
	Task SpawnPlayer()
	{
		PlayerEntity player = Instantiate(PlayerPrefab, canvas.transform).GetComponent<PlayerEntity>();
		OnPlayerSpawned?.Invoke(player);

		return Task.CompletedTask;
	}
	Task SpawnEnemies()
	{
		float spacing = (Screen.width - numberOfEnemiesToSpawn * widthOfEntities) / (numberOfEnemiesToSpawn + 1);

		for (int i = 0; i < numberOfEnemiesToSpawn; i++)
		{
			Entity spawnedEntity = Instantiate(EntityPrefab, canvas.transform).GetComponent<Entity>();
			spawnedEntity.entityData = GetRandomEnemyToSpawn();
			SetEnemyPosition(spawnedEntity.GetComponent<RectTransform>(), spacing, i + 1);
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
