using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static DamageData;

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

	private void Awake()
	{
		instance = this;
	}

	private void Update()
	{
		GetFps();
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
}
