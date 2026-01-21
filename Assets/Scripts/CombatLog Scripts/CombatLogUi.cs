using System.Collections.Generic;
using UnityEngine;
using static Woopsious.CardHandler;

namespace Woopsious
{
	public class CombatLogUi : MonoBehaviour
	{
		public static CombatLogUi instance;

		public GameObject debugButton;
		public GameObject combatLogWindowUi;

		public GameObject contentWindow;

		public GameObject logMessageTemplate;

		public List<GameObject> combatLogsList = new();

		private void Awake()
		{
			instance = this;
		}

		private void OnEnable()
		{
			GameManager.OnGameStateChange += OnGameStateChange;
		}

		private void OnDestroy()
		{
			GameManager.OnGameStateChange -= OnGameStateChange;
		}

		private void OnGameStateChange(GameManager.GameState gameState)
		{
			if (GameManager.GameState.CardCombat == gameState)
				ShowCombatLog();
			else
				HideCombatLog();
		}

		private void ShowCombatLog()
		{
			debugButton.SetActive(true);
			combatLogWindowUi.SetActive(true);
		}
		private void HideCombatLog()
		{
			debugButton.SetActive(false);
			combatLogWindowUi.SetActive(false);
			ClearCombatLog();
		}

		public static void CreateNewCombatLog(CombatLogContext combatLogContext)
		{
			GameObject go = Instantiate(instance.logMessageTemplate, instance.contentWindow.transform);
			CombatLogMessageUi combatLogMessage = go.GetComponent<CombatLogMessageUi>();
			combatLogMessage.CreateLogMessage(combatLogContext);
			instance.combatLogsList.Add(go);
		}

		private void ClearCombatLog()
		{
			for (int i = combatLogsList.Count - 1; i >= 0; i--)
				Destroy(combatLogsList[i]);
		}

		public void DebugCreateCombatLog()
		{
			CreateNewCombatLog(new(CombatLogContext.CombatLogEntry.debug));
		}
	}
}
