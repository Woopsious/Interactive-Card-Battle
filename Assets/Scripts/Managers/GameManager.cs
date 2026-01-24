using NUnit.Framework.Constraints;
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
		/// SPECIAL RULE SETS:
		/// rule sets that make card combat more interesting examples:
		///		if enemy x attacks enemy y with z buff deal +5 damage,
		///		if enemy x and enemy y exist at the same time enemy x & y + 10 health etc...
		///		
		///		ARCHITECTURE:
		///			rules as scriptable objects, RuleSetManager that listenes to events for event based rules (turn start etc...) and can be called 
		///			for reactive rules like dealing extra damage if enemy x has y buff
		/// 
		/// WORLD DIFFICULTY AND MAKING IT ENDLESS:
		/// start world difficulty at 1, then add +1 to it once player defeats the boss at the end then generate new map.
		/// allow player to chose starting world difficulty (only if said world difficulty was previously beaten)
		/// 
		///		WORLD DIFFICULTY MODIFIERS:
		///		consider world modifiers that will last just for said map that influences both map nodes and card combat
		///		Map Modifier example: increase chances for desert land types to spawn etc.. or the ruins modifier to be more common etc...
		///		Card combat modifier example: enemy types do 1.5x more damage, gain perma 10 block, have 2x health etc... healing/damage 2x or /2
		///		
		/// 
		/// POSSIBLE ENEMY SPAWNING OVERHAUL:
		/// have a % spawn chance for every land type and land modifier in inspector
		/// each modifier will + - from its total spawn chance for that area negative number = can never spawn despite modifiers EG:
		///		grasslands = 0.25
		///		hills = 0.1
		///		tundra = 0, 0 will allow them to spawn with modifiers
		///		desert = -0.1, negatives will force them to never spawn here
		///		
		///		ruins = 0.2
		///		town = -0.1
		///		
		///		True spawn Rates
		///		grasslands + ruins and town = 0.35
		///		hills + ruins and town = 0.2
		///		tundra + ruins and town = 0.1 so cant spawn
		///		desert + ruins and town = -0.1 will never spawn spawn
		/// 
		/// DRAW/DISCARD/DESTROYED CARD PILES:
		/// add lists for each type of pile + a collected cards pile not used in combat (all player cards at runtime are here)
		/// overhaul replace card system to use new piles
		/// overhaul draw card system to use new piles
		/// 
		///		DRAW PILE:
		///		new card gets drawn from this draw pile. on new turn + when replacing card
		///		when draw pile doesnt have enough cards, recall all cards in used pile back into draw pile (excluding destroy pile)
		///		
		///		USED PILE:
		///		when using a card in hand, add it to this pile
		///		when replacing a card, add it to this pile
		///		
		///		DESTROYED PILE:
		///		for cards that are destroyed for what ever reason
		///		
		/// 
		/// PLAYER CARD SYSTEM:
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

		//scene names
		public readonly string mainScene = "MainScene";
		public readonly string gameScene = "GameScene";

		//loaded scenes
		public Scene loadedMainScene;
		public Scene loadedGameScene;

		[Header("Debug")]
		public bool debugGenNewMapAtColumnOne;
		public List<EntityData> debugEnemiesToFight = new();

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
		public List<MapNodeDefinition> mapNodeDataTypes = new();

		[Header("Runtime Data")]
		public static EntityData PlayerClass { get; private set; }
		public static MapNodeController CurrentlyVisitedMapNode { get; private set; }
		public static GameState CurrentGameState { get; private set; }
		public enum GameState
		{
			MainMenu, MapView, CardCombat, CardCombatWin, CardCombatLoss, debugCombat
		}

		//GAME EVENTS
		public static event Action<GameState> OnGameStateChange;
		public static event Action OnGenerateNewMap;

		//DEBUG GAME EVENTS

		private void Awake()
		{
			instance = this;
			loadedMainScene = SceneManager.GetActiveScene();
			SplitEnemyDataIntoTypes();
		}

		void OnEnable()
		{
			SceneManager.sceneLoaded += OnLoadSceneFinish;
			SceneManager.sceneUnloaded += OnUnloadSceneFinish;
		}
		void OnDisable()
		{
			SceneManager.sceneLoaded -= OnLoadSceneFinish;
			SceneManager.sceneUnloaded -= OnUnloadSceneFinish;
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

		//game state changes
		public static void EnterCardCombat(MapNodeController mapNode)
		{
			PauseGame(false);
			CurrentlyVisitedMapNode = mapNode;
			CurrentGameState = GameState.CardCombat;
			OnGameStateChange?.Invoke(CurrentGameState);
		}
		public static void EnterMapView()
		{
			PauseGame(false);

			CurrentGameState = GameState.MapView;
			OnGameStateChange?.Invoke(CurrentGameState);

			if (instance.debugGenNewMapAtColumnOne || CurrentlyVisitedMapNode != null && CurrentlyVisitedMapNode.instanceData.IsMapEndNode)
				GenerateNewMap();
		}
		public static void EnterCardCombatWin()
		{
			PauseGame(true);
			CurrentGameState = GameState.CardCombatWin;
			OnGameStateChange?.Invoke(CurrentGameState);
		}
		public static void EnterCardCombatLoss()
		{
			PauseGame(true);
			CurrentGameState = GameState.CardCombatLoss;
			OnGameStateChange?.Invoke(CurrentGameState);
		}
		public static void GenerateNewMap()
		{
			OnGenerateNewMap?.Invoke();
		}

		//debug start/end card combat
		public static void DebugStartCardCombatGameState()
		{
			PauseGame(false);
			CurrentlyVisitedMapNode = MapController.Instance.MapNodeTable[0][0]; //grab first map node in first column
			CurrentGameState = GameState.CardCombat;
			OnGameStateChange?.Invoke(GameState.debugCombat);
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
			{
				GenerateNewMap();
				EnterMapView();
			}

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
