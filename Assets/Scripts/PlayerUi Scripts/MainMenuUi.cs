using TMPro;
using UnityEngine;

public class MainMenuUi : MonoBehaviour
{
	public static MainMenuUi Instance;

	//fps counter
	public TMP_Text fpsCounter;

	int framerateCounter = 0;
	float timeCounter = 0.0f;
	float lastFramerate = 0.0f;
	float refreshTime = 0.5f;

	void Awake()
	{
		Instance = this;
	}

	void Update()
	{
		DisplayFps();
	}

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
