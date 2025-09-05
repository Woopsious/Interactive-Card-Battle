using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Woopsious
{
	public class GameManager : MonoBehaviour
	{
		/// <summary>
		/// TODO:
		/// MAP NODES SYSTEM:
		/// create a map ui where player moves across it from left to right through nodes that are semi randomly generated
		/// player is given a choice of 1 to 3 nodes they can move to that starts a combat fight.
		/// nodes will store the next nodes they link to + there travel state (locked, canTravel, currentlyAt) to allow player to move about
		/// settings to dictate what type of enemies spawn on them, there difficulty (harder enemy variants/higher level) and a budget limit, 
		/// a node will randomly spawn enemies that match type they spawn till they run out of budget.
		/// 
		/// PLAYER CARD DECK UPGRADE SYSTEM:
		/// increase base health of player or other unique gimmicks (ignore x% of damage for x rounds/turns etc...)
		/// increase total amount of cards that can be played per turn + amount of offensive/non offensive cards types that can be played per turn
		/// increase amount of replace card actions a player has per turn, increasing amount of card options given besides the defult current 5.
		/// add a system where player can upgrade there cards (more damage/heal/block)
		/// 
		/// DIFFICULTY SCALING:
		/// difficulty scales up with the more fights player wins. leading to higher level enemies or harder/elite versions of regular enemies (or both)
		/// 
		/// keep track of a score so once player dies they can see what they scored (currently dont have an end)
		/// 
		/// </summary>

		public static GameManager instance;

		//player runtime class
		public static EntityData PlayerClass { get; private set; }

		//scene names
		public readonly string mainScene = "MainScene";
		public readonly string gameScene = "GameScene";

		//loaded scenes
		public Scene loadedMainScene;
		public Scene loadedGameScene;

		[Header("Player Scriptable Objects")]
		public List<EntityData> playerClassDataTypes = new();

		[Header("Entity Scriptable Objects")]
		public List<EntityData> entityDataTypes = new();

		[Header("Status Effects Scriptable Objects")]
		public List<StatusEffectsData> statusEffectsDataTypes = new();

		[Header("Map Node Scriptable Objects")]
		public List<MapNodeData> mapNodeDataTypes = new();

		//game events
		public static event Action<MapNode> OnStartCardCombatEvent;
		public static event Action OnStartCardCombatUiEvent;
		public static event Action<bool> OnEndCardCombatEvent;
		public static event Action OnShowMapEvent;

		private void Awake()
		{
			instance = this;
			loadedMainScene = SceneManager.GetActiveScene();
		}
		private void Start()
		{
			ShowMap();
		}

		void OnEnable()
		{
			SceneManager.sceneLoaded += OnLoadSceneFinish;
			SceneManager.sceneUnloaded += OnUnloadSceneFinish;
			Entity.OnEntityDeath += EndCardCombat;
		}
		void OnDisable()
		{
			SceneManager.sceneLoaded -= OnLoadSceneFinish;
			SceneManager.sceneUnloaded -= OnUnloadSceneFinish;
			Entity.OnEntityDeath -= EndCardCombat;
		}

		//start/end card combat
		public static void BeginCardCombat(MapNode mapNode)
		{
			PauseGame(false);
			OnStartCardCombatEvent?.Invoke(mapNode);
			OnStartCardCombatUiEvent?.Invoke();
		}
		void EndCardCombat(Entity entity)
		{
			if (TurnOrderManager.Player() == entity)
			{
				PauseGame(true);
				OnEndCardCombatEvent?.Invoke(false); //end on lose of player dies
			}

			if (TurnOrderManager.EnemyEntities().Count > 0) return;

			OnEndCardCombatEvent?.Invoke(true); //end on win if no enemies left alive
		}
		public static void ShowMap()
		{
			OnShowMapEvent?.Invoke();
		}

		public static void PauseGame(bool pause)
		{
			if (pause)
				Time.timeScale = 0f;
			else
				Time.timeScale = 1f;
		}

		//SET PLAYER CLASS
		public static void SetPlayerClass(EntityData playerClass)
		{
			PlayerClass = playerClass;
		}

		//LOAD SCENES
		public static void LoadGameScene()
		{
			instance.StartCoroutine(instance.LoadSceneAsync(instance.gameScene));
		}
		public static void ExitGameScene()
		{
			instance.StartCoroutine(instance.TryUnLoadSceneAsync(instance.gameScene));
		}
		public static void QuitGame()
		{
			Application.Quit();
		}

		//SCENE MANAGER
		//loading + event
		private IEnumerator LoadSceneAsync(string sceneToLoad)
		{
			Debug.LogError("loading scene name: " + sceneToLoad + " at: " + DateTime.Now.ToString());

			AsyncOperation asyncLoadScene = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);

			while (!asyncLoadScene.isDone)
				yield return null;
		}
		private void OnLoadSceneFinish(Scene loadedScene, LoadSceneMode mode)
		{
			Debug.LogError("loaded scene name: " + loadedScene.name + " at: " + DateTime.Now.ToString());

			if (loadedScene.name == mainScene)
				SaveManager.LoadPlayerData();
			if (loadedScene.name == gameScene)
				OnShowMapEvent?.Invoke();

			UpdateActiveSceneToMainScene(loadedScene);
			UpdateCurrentlyLoadedScene(loadedScene);
		}

		//unloading + event
		private IEnumerator TryUnLoadSceneAsync(string sceneToUnLoad)
		{
			AsyncOperation asyncUnLoadScene = SceneManager.UnloadSceneAsync(sceneToUnLoad);

			while (!asyncUnLoadScene.isDone)
				yield return null;
		}
		private void OnUnloadSceneFinish(Scene unLoadedScene)
		{
			Debug.LogError("unloaded scene: " + unLoadedScene.name + " at: " + DateTime.Now.ToString());
		}

		//scene updates
		private void UpdateActiveSceneToMainScene(Scene newLoadedScene)
		{
			Scene currentActiveScene = SceneManager.GetActiveScene();
			if (currentActiveScene.name == mainScene) return;
			SceneManager.SetActiveScene(newLoadedScene);
		}
		private void UpdateCurrentlyLoadedScene(Scene newLoadedScene)
		{
			if (newLoadedScene.name == mainScene)   //scene always stays loaded
				loadedMainScene = newLoadedScene;
			else if (newLoadedScene.name == gameScene)
				loadedGameScene = newLoadedScene;
		}

		//scene active check
		public static bool SceneIsActive(string sceneName)
		{
			List<Scene> loadedScenes = new();

			for (int i = 0; i < SceneManager.sceneCount; i++)
				loadedScenes.Add(SceneManager.GetSceneAt(i));

			foreach (Scene scene in loadedScenes)
			{
				if (scene.name == sceneName)
					return true;
				else continue;
			}

			return false;
		}
	}
}
