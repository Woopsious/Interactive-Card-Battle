using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static GameManager instance;

	public Camera uiCamera;

	public GameObject PlayerLocation;

	private void Awake()
	{
		instance = this;
	}
}
