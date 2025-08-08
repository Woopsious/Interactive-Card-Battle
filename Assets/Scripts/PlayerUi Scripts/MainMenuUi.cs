using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Woopsious
{
	public class MainMenuUi : MonoBehaviour
	{
		public static MainMenuUi Instance;

		public bool inGame;

		public GameObject mainMenuPanel;

		[Header("Main Menu Buttons")]
		public Button newGameButton;
		RectTransform newGameButtonRectTransform;

		public Button saveGameButton;
		RectTransform saveGameButtonRectTransform;

		public Button loadGameButton;
		RectTransform loadGameButtonRectTransform;

		public Button settingsButton;
		RectTransform settingsGameButtonRectTransform;

		[Header("Main Menu Buttons")]
		public Button backToMainMenuButton; //reused in sub menus (save slots/settings ui)

		public static Action ShowMainMenuUi;
		public static Action ShowSaveSlotsUi;
		public static Action ShowSettingsUi;

		//fps counter
		public TMP_Text fpsCounter;

		int framerateCounter = 0;
		float timeCounter = 0.0f;
		float lastFramerate = 0.0f;
		readonly float refreshTime = 0.5f;

		void Awake()
		{
			Instance = this;
			SetUpButtons();
		}

		void Update()
		{
			DisplayFps();
			DetectEscapeButtonPresses();
		}

		void OnEnable()
		{
			ShowMainMenuUi += ShowMainMenuUiPanel;
			ShowSaveSlotsUi += HideMainMenuUiPanel;
			ShowSettingsUi += HideMainMenuUiPanel;
		}
		void OnDestroy()
		{
			ShowMainMenuUi -= ShowMainMenuUiPanel;
			ShowSaveSlotsUi -= HideMainMenuUiPanel;
			ShowSettingsUi -= HideMainMenuUiPanel;
		}

		void SetUpButtons()
		{
			newGameButton.onClick.AddListener(delegate { StartNewGame(); });
			saveGameButton.onClick.AddListener(delegate { ShowSaveSlotsUi?.Invoke(); });
			loadGameButton.onClick.AddListener(delegate { ShowSaveSlotsUi?.Invoke(); });
			settingsButton.onClick.AddListener(delegate { ShowSettingsUi?.Invoke(); });
			backToMainMenuButton.onClick.AddListener(delegate { ShowMainMenuUi?.Invoke(); });

			newGameButtonRectTransform = newGameButton.GetComponent<RectTransform>();
			saveGameButtonRectTransform = saveGameButton.GetComponent<RectTransform>();
			loadGameButtonRectTransform = loadGameButton.GetComponent<RectTransform>();
			settingsGameButtonRectTransform = settingsButton.GetComponent<RectTransform>();
		}

		void DetectEscapeButtonPresses()
		{
			if (!inGame) return;

			if (Input.GetKeyDown(KeyCode.Escape))
			{
				if (mainMenuPanel.activeInHierarchy)
					HideMainMenuUiPanel();
				else
					ShowMainMenuUiPanel();
			}
		}

		void StartNewGame()
		{
			//start new game
			HideMainMenuUiPanel();
		}

		//change ui panels
		public void ShowMainMenuUiPanel()
		{
			if (inGame)
			{
				newGameButtonRectTransform.anchoredPosition = new Vector3(-800, 200, 0);
				saveGameButtonRectTransform.anchoredPosition = new Vector3(-800, 200, 0);
				loadGameButtonRectTransform.anchoredPosition = new Vector3(-800, 100, 0);
				settingsGameButtonRectTransform.anchoredPosition = new Vector3(-800, -100, 0);

				newGameButton.gameObject.SetActive(false);
				saveGameButton.gameObject.SetActive(true);
			}
			else
			{
				newGameButtonRectTransform.anchoredPosition = new Vector3(0, 200, 0);
				saveGameButtonRectTransform.anchoredPosition = new Vector3(0, 200, 0);
				loadGameButtonRectTransform.anchoredPosition = new Vector3(0, 100, 0);
				settingsGameButtonRectTransform.anchoredPosition = new Vector3(0, -100, 0);

				newGameButton.gameObject.SetActive(true);
				saveGameButton.gameObject.SetActive(false);
			}

			mainMenuPanel.SetActive(true);
		}
		public void HideMainMenuUiPanel()
		{
			mainMenuPanel.SetActive(false);
		}

		//fps
		void DisplayFps()
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
	}

}