using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Woopsious.AbilitySystem;
using static Woopsious.EntityData;

namespace Woopsious
{
	public class GameManager : MonoBehaviour
	{
		/// <summary>
		/// TODO:
		/// 
		/// DRAW/DISCARD/DESTROYED CARD PILES:
		/// add lists for each type of pile + a collected cards pile not used in combat (all player cards at runtime are here)
		/// overhaul replace card system to use new piles
		/// overhaul draw card system to use new piles
		/// 
		///		DRAW PILE:
		///		new card gets drawn from this draw pile. on new turn + when replacing card
		///		when draw pile doesnt have enough cards, recall all cards in discard pile back into draw pile (excluding destroy pile)
		///		
		///		DISCARD PILE:
		///		when replacing card, card gets added to discard pile,
		///		
		///		DESTROYED PILE:
		///		for cards that are destroyed for what ever reason
		///		
		/// 
		/// PLAYER CARD SYSTEM:
		/// add rarities to cards, higher rarity = generally bettor or more unique 
		/// EG: one time use cards per battle, 'ultimate' moves based on player class etc... 
		/// card upgrade system, increasing effects of cards in some way, 1 card 
		/// EG: (base, base+, base++) (dmg:10, dmg:12, dmg:15) (strength stacks: 2,2,3)
		/// 
		/// CUSTOM EDITOR FOR PLAYER CARD SYSTEM:
		/// custom editor for AttackData since player cards and enemy attack moves both use this to make creating new ones easier
		/// + including extra info that let me customize how cards get upgraded using a new class that contains data for each level upgrade
		/// giving more control over a flat +20% each card level as it wouldnt work with above example as easily
		/// 
		/// </summary>

		public static GameManager instance;

		//player runtime class
		public static EntityData PlayerClass { get; private set; }

		//current map node
		public static MapNode CurrentlyVisitedMapNode { get; private set; }

		//scene names
		public readonly string mainScene = "MainScene";
		public readonly string gameScene = "GameScene";

		//loaded scenes
		public Scene loadedMainScene;
		public Scene loadedGameScene;

		[Header("Player Scriptable Objects")]
		public List<EntityData> playerClassDataTypes = new();

		[Header("Enemy Scriptable Objects")]
		public List<EntityData> enemyDataTypes = new();

		public List<EntityData> AbberationsEnemyData { get; private set; } = new();
		public List<EntityData> BeastsEnemyData { get; private set; } = new();
		public List<EntityData> ConstructsEnemyData { get; private set; } = new();
		public List<EntityData> HumanoidsEnemyData { get; private set; } = new();
		public List<EntityData> SlimesEnemyData { get; private set; } = new();
		public List<EntityData> UndeadEnemyData { get; private set; } = new();

		[Header("Status Effects Scriptable Objects")]
		public List<StatusEffectsData> statusEffectsDataTypes = new();

		[Header("Map Node Scriptable Objects")]
		public List<MapNodeData> mapNodeDataTypes = new();

		//game events
		public static event Action<MapNode> OnStartCardCombatEvent;
		public static event Action<List<EntityData>> OnDebugStartCardCombatEvent;
		public static event Action OnStartCardCombatUiEvent;
		public static event Action<bool> OnEndCardCombatEvent;
		public static event Action OnShowMapEvent;

		private void Awake()
		{
			instance = this;
			loadedMainScene = SceneManager.GetActiveScene();
			SplitEnemyDataIntoTypes();
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

		void SplitEnemyDataIntoTypes()
		{
			foreach (EntityData entityData in instance.enemyDataTypes)
			{
				switch (entityData.enemyType)
				{
					case EnemyTypes.Abberration: AbberationsEnemyData.Add(entityData); break;

					case EnemyTypes.beast: BeastsEnemyData.Add(entityData); break;

					case EnemyTypes.construct: ConstructsEnemyData.Add(entityData); break;

					case EnemyTypes.humanoid: HumanoidsEnemyData.Add(entityData); break;

					case EnemyTypes.slime: SlimesEnemyData.Add(entityData); break;

					case EnemyTypes.undead: UndeadEnemyData.Add(entityData); break;
				}
			}
		}

		//start/end card combat
		public static void BeginCardCombat(MapNode mapNode)
		{
			PauseGame(false);
			OnStartCardCombatEvent?.Invoke(mapNode);
			CurrentlyVisitedMapNode = mapNode;
			OnStartCardCombatUiEvent?.Invoke();
		}
		public static void DebugBeginCardCombat(List<EntityData> entityDatas)
		{
			PauseGame(false);
			OnDebugStartCardCombatEvent?.Invoke(entityDatas);
			CurrentlyVisitedMapNode = InteractiveMapHandler.Instance.MapNodeTable[0][0]; //grab first map node in first column
			OnStartCardCombatUiEvent?.Invoke();
		}
		public static void DebugEndCardCombat()
		{
			instance.EndCardCombat(TurnOrderManager.Player());
		}
		void EndCardCombat(Entity entity)
		{
			if (TurnOrderManager.Player() == entity) //end on loss if player dies
			{
				PauseGame(true);
				OnEndCardCombatEvent?.Invoke(false); //loss
			}

			int enemiesDead = 0;

			foreach (Entity enemy in TurnOrderManager.EnemyEntities())
			{
				if (enemy.health <= 0)
					enemiesDead++;
			}

			if (enemiesDead < TurnOrderManager.EnemyEntities().Count) return; //end on win if no enemies left
			OnEndCardCombatEvent?.Invoke(true); //win
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
