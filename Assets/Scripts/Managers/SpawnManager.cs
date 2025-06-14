using System;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.LightTransport;

public class SpawnManager : MonoBehaviour
{
	public static SpawnManager instance;

	public Canvas canvas;

	public GameObject EntityPrefab;
	public GameObject PlayerPrefab;

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

	private void Start()
	{
		SpawnEnemies();
		SpawnPlayer();

		OnStartGame?.Invoke();
	}

	void SpawnPlayer()
	{
		PlayerEntity player = Instantiate(PlayerPrefab, canvas.transform).GetComponent<PlayerEntity>();
		OnPlayerSpawned?.Invoke(player);
	}
	void SpawnEnemies()
	{
		float spacing = (Screen.width - numberOfEnemiesToSpawn * widthOfEntities) / (numberOfEnemiesToSpawn + 1);

		for (int i = 0; i < numberOfEnemiesToSpawn; i++)
		{
			Entity spawnedEntity = Instantiate(EntityPrefab, canvas.transform).GetComponent<Entity>();
			spawnedEntity.entityData = GetRandomEnemyToSpawn();
			SetEnemyPosition(spawnedEntity.GetComponent<RectTransform>(), spacing, i + 1);
			OnEnemySpawned?.Invoke(spawnedEntity);
		}
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
}
