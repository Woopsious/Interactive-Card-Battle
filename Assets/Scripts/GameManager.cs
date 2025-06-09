using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static GameManager instance;

	public Camera uiCamera;

	private void Awake()
	{
		instance = this;
	}
}
