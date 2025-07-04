using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Woopsious
{
	public class GameManager : MonoBehaviour
	{
		/// <summary>
		/// TODO:
		/// possibly turn cards like block/heal into instant use cards instead of throwing them at urself.
		/// 
		/// add a simple brain to enemies to work out what type of card they should play.
		/// 
		/// CARD DECK MANAGER REFACTOR:
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
