using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
	public static SpawnManager instance;

	public Canvas canvas;

	public GameObject EntityPrefab;
	public GameObject PlayerPrefab;

	public List<EntityData> entityDataTypes = new();

	public int numberOfEnemiesToSpawn;

	public static event Action OnStartGame;

	public static event Action<PlayerEntity> OnPlayerSpawned;
	public static event Action<Entity> OnEnemySpawned;

	void Awake()
	{
		instance = this;
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
		float spacing = GetSpacing();

		for (int i = 0; i < numberOfEnemiesToSpawn; i++)
		{
			Entity spawnedEntity = Instantiate(EntityPrefab, canvas.transform).GetComponent<Entity>();
			spawnedEntity.entityData = GetRandomEnemyToSpawn();
			SetEnemyPosition(spawnedEntity.GetComponent<RectTransform>(), spacing, i);
			OnEnemySpawned?.Invoke(spawnedEntity);
		}
	}

	void SetEnemyPosition(RectTransform rectTransform, float spacing, int index)
	{
		rectTransform.anchoredPosition = new Vector2(spacing * index, 400);
	}

	float GetSpacing()
	{
		float widthOfEnemy = 125f;
		float remaningSpace = Screen.width - (numberOfEnemiesToSpawn * widthOfEnemy);

		Debug.LogError("remaning space: " + remaningSpace);

		float spacing = (remaningSpace + widthOfEnemy) / (numberOfEnemiesToSpawn - 1);

		Debug.LogError("spacing: " + spacing);

		return spacing;
	}

	EntityData GetRandomEnemyToSpawn()
	{
		EntityData entityData = entityDataTypes[UnityEngine.Random.Range(0, entityDataTypes.Count)];
		return entityData;
	}
}
