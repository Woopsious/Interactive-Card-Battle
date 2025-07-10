using System;
using TMPro;
using UnityEngine;

namespace Woopsious
{
	public class GameManager : MonoBehaviour
	{
		/// <summary>
		/// TODO:
		/// create a map ui where player moves across it from left to right through nodes that are semi randomly generated
		/// player is given a choice of 1 to 3 nodes they can move to that starts a combat fight.
		/// 
		/// add a system where player can upgrade there cards (more damage/heal/block) from a PlayerCardDeckManager
		/// 
		/// add difficulty scale, enemies get harder the more fights player does till they die and get a final score etc...
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

		private void Awake()
		{
			instance = this;
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
			Entity.OnEntityDeath += EndCardCombat;
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

		public static void PauseGame(bool pause)
		{
			if (pause)
				Time.timeScale = 0f;
			else
				Time.timeScale = 1f;
		}
	}
}
