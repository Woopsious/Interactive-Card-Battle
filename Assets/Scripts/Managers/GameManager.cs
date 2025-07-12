using System;
using TMPro;
using UnityEngine;

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

		public TMP_Text fpsCounter;

		int framerateCounter = 0;
		float timeCounter = 0.0f;
		float lastFramerate = 0.0f;
		public float refreshTime = 0.5f;

		public static event Action OnStartCardCombatEvent;
		public static event Action<bool> OnEndCardCombatEvent;
		public static event Action OnShowMapEvent;

		private void Awake()
		{
			instance = this;
		}
		private void Start()
		{
			ShowMap();
		}

		private void Update()
		{
			GetFps();
		}

		void OnEnable()
		{
			Entity.OnEntityDeath += EndCardCombat;
		}
		void OnDisable()
		{
			Entity.OnEntityDeath -= EndCardCombat;
		}

		void GetFps()
		{
			if (timeCounter < refreshTime)
			{
				timeCounter += Time.deltaTime;
				framerateCounter++;
			}
			else
			{
				//This code will break if you set your m_refreshTime to 0, which makes no sense.
				lastFramerate = framerateCounter / timeCounter;
				framerateCounter = 0;
				timeCounter = 0.0f;
			}
			fpsCounter.text = "FPS: " + (int)lastFramerate;
		}

		//start/end card combat
		public static void BeginCardCombat()
		{
			PauseGame(false);
			OnStartCardCombatEvent?.Invoke();
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
	}
}
