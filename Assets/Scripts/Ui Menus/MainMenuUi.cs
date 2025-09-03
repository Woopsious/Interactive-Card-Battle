using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Woopsious
{
	public class MainMenuUi : MonoBehaviour
	{
		public static MainMenuUi Instance;

		public bool inGame;

		public GameObject mainMenuPanel;
		Image mainMenuBackground;

		[Header("Main Menu Buttons")]
		public Button newGameButton;
		RectTransform newGameButtonRectTransform;

		public Button saveGameButton;
		RectTransform saveGameButtonRectTransform;

		public Button loadGameButton;
		RectTransform loadGameButtonRectTransform;

		public Button settingsButton;
		RectTransform settingsButtonRectTransform;

		public Button databaseButton;
		RectTransform databaseButtonRectTransform;

		public Button exitGameButton;
		RectTransform exitGameButtonRectTransform;

		public Button quitGameButton;
		RectTransform QuitGameButtonRectTransform;

		[Header("Back Buttons")]
		public Button backButton; //reused in sub menus (save slots/settings ui etc...)
		TMP_Text backButtonText;

		public static Action ShowMainMenuUi;
		public static Action ShowNewGameUi;
		public static Action ShowSaveSlotsUi;
		public static Action ShowSettingsUi;
		public static Action ShowDatabaseUi;

		//fps counter
		public TMP_Text fpsCounter;

		int framerateCounter = 0;
		float timeCounter = 0.0f;
		float lastFramerate = 0.0f;
		readonly float refreshTime = 0.5f;

		Color _ColourDarkGray = new(0.2941177f, 0.2941177f, 0.2941177f, 1);

		void Awake()
		{
			Instance = this;
			mainMenuBackground = mainMenuPanel.GetComponent<Image>();
			backButtonText = backButton.GetComponentInChildren<TMP_Text>();
			SetUpButtons();
		}

		void Update()
		{
			DisplayFps();
			DetectEscapeButtonPressesInGame();
		}

		void OnEnable()
		{
			SceneManager.sceneLoaded += OnSceneLoadedEvent;
			SceneManager.sceneUnloaded += OnSceneUnLoadedEvent;

			ShowMainMenuUi += ShowMainMenuUiPanel;
			ShowNewGameUi += HideMainMenuUiPanel;
			ShowSaveSlotsUi += HideMainMenuUiPanel;
			ShowDatabaseUi += HideMainMenuUiPanel;
			ShowSettingsUi += HideMainMenuUiPanel;
		}
		void OnDestroy()
		{
			SceneManager.sceneLoaded -= OnSceneLoadedEvent;
			SceneManager.sceneUnloaded -= OnSceneUnLoadedEvent;

			ShowMainMenuUi -= ShowMainMenuUiPanel;
			ShowNewGameUi -= HideMainMenuUiPanel;
			ShowSaveSlotsUi -= HideMainMenuUiPanel;
			ShowDatabaseUi -= HideMainMenuUiPanel;
			ShowSettingsUi -= HideMainMenuUiPanel;
		}

		void SetUpButtons()
		{
			newGameButton.onClick.AddListener(() => ShowNewGameUi?.Invoke());
			saveGameButton.onClick.AddListener(() => ShowSaveSlotsUi?.Invoke());
			loadGameButton.onClick.AddListener(() => ShowSaveSlotsUi?.Invoke());
			databaseButton.onClick.AddListener(() => ShowDatabaseUi?.Invoke());
			settingsButton.onClick.AddListener(() => ShowSettingsUi?.Invoke());
			exitGameButton.onClick.AddListener(() => GameManager.ExitGameScene());
			quitGameButton.onClick.AddListener(() => GameManager.QuitGame());

			backButton.onClick.AddListener(() => BackButtonClickEvents());

			newGameButtonRectTransform = newGameButton.GetComponent<RectTransform>();
			saveGameButtonRectTransform = saveGameButton.GetComponent<RectTransform>();
			loadGameButtonRectTransform = loadGameButton.GetComponent<RectTransform>();
			databaseButtonRectTransform = databaseButton.GetComponent<RectTransform>();
			settingsButtonRectTransform = settingsButton.GetComponent<RectTransform>();
			exitGameButtonRectTransform = exitGameButton.GetComponent<RectTransform>();
			QuitGameButtonRectTransform = quitGameButton.GetComponent<RectTransform>();
		}

		void BackButtonClickEvents()
		{
			if (mainMenuPanel.activeInHierarchy)
				HideMainMenuUiPanel();
			else
				ShowMainMenuUi?.Invoke();
		}

		void DetectEscapeButtonPressesInGame()
		{
			//if (!inGame) return;

			if (Input.GetKeyDown(KeyCode.Escape))
			{
				if (mainMenuPanel.activeInHierarchy)
					HideMainMenuUiPanel();
				else
					ShowMainMenuUi?.Invoke();
			}
		}

		//scene change events
		void OnSceneLoadedEvent(Scene loadedScene, LoadSceneMode mode)
		{
			if (loadedScene.name == GameManager.instance.mainScene)
			{
				ShowMainMenuUi?.Invoke();
			}
			else if (loadedScene.name == GameManager.instance.gameScene)
			{
				inGame = true;
				HideMainMenuUiPanel();
			}
		}
		void OnSceneUnLoadedEvent(Scene unloadedScene)
		{
			if (unloadedScene.name == GameManager.instance.gameScene)
			{
				inGame = false;
				ShowMainMenuUi?.Invoke();
			}
		}

		//change ui panels
		public void ShowMainMenuUiPanel()
		{
			if (inGame)
			{
				newGameButtonRectTransform.anchoredPosition = new Vector2(-800, 250);
				saveGameButtonRectTransform.anchoredPosition = new Vector2(-800, 250);
				loadGameButtonRectTransform.anchoredPosition = new Vector2(-800, 150);
				databaseButtonRectTransform.anchoredPosition = new Vector2(-800, 50);
				settingsButtonRectTransform.anchoredPosition = new Vector2(-800, -100);
				exitGameButtonRectTransform.anchoredPosition = new Vector2(-800, -200);
				QuitGameButtonRectTransform.anchoredPosition = new Vector2(-800, -200);

				mainMenuBackground.color = _ColourDarkGray - new Color(0, 0, 0, 0.25f); //make transparent

				newGameButton.gameObject.SetActive(false);
				saveGameButton.gameObject.SetActive(true);

				exitGameButton.gameObject.SetActive(true);
				quitGameButton.gameObject.SetActive(false);
			}
			else
			{
				newGameButtonRectTransform.anchoredPosition = new Vector2(0, 250);
				saveGameButtonRectTransform.anchoredPosition = new Vector2(0, 250);
				loadGameButtonRectTransform.anchoredPosition = new Vector2(0, 150);
				databaseButtonRectTransform.anchoredPosition = new Vector2(0, 50);
				settingsButtonRectTransform.anchoredPosition = new Vector2(0, -100);
				exitGameButtonRectTransform.anchoredPosition = new Vector2(0, -200);
				QuitGameButtonRectTransform.anchoredPosition = new Vector2(0, -200);

				mainMenuBackground.color = _ColourDarkGray;

				newGameButton.gameObject.SetActive(true);
				saveGameButton.gameObject.SetActive(false);

				exitGameButton.gameObject.SetActive(false);
				quitGameButton.gameObject.SetActive(true);
			}

			backButton.gameObject.transform.SetParent(mainMenuPanel.transform);
			backButtonText.text = "Close Menu";
			mainMenuPanel.SetActive(true);
		}
		public void HideMainMenuUiPanel()
		{
			mainMenuPanel.SetActive(false);
			backButtonText.text = "Back";
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