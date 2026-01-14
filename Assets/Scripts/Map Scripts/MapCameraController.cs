using UnityEngine;
using UnityEngine.EventSystems;
using Woopsious;

public class MapCameraController : MonoBehaviour, IDragHandler
{
	public Camera mapCamera;

	//camera zoom size
	readonly float cameraBaseOrthoSize = 540;
	readonly float maxCameraOrthoSize = 540 * 2.5f;
	readonly float minCameraOrthoSize = 540 / 2.5f;
	readonly float zoomSpeed = 500;

	//camera move limits
	private Vector2 maxCameraPosition = new(Screen.width, Screen.height);
	private Vector2 minCameraPosition = new(-Screen.width, -Screen.height);

	private void Start()
	{
		SetCameraViewConstraints();
	}

	private void OnEnable()
	{
		mapCamera.gameObject.SetActive(true);
	}
	private void OnDisable()
	{
		mapCamera.gameObject.SetActive(false);
	}

	private void Update()
	{
		MapZoom();
	}

	private void SetCameraViewConstraints()
	{
		mapCamera.orthographicSize = cameraBaseOrthoSize;

		int viewWidth = (int)(Screen.width * 1.25f);
		int viewHeight = (int)(Screen.width * 1.25f);

		int widthOffset = Screen.width / 2;
		int heightOffset = Screen.height / 2;

		maxCameraPosition = new(viewWidth + widthOffset, viewHeight + heightOffset);
		minCameraPosition = new(-viewWidth + widthOffset, -viewHeight + heightOffset);
	}

	//map drag and zoom
	private void MapZoom()
	{
		mapCamera.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
		mapCamera.orthographicSize = Mathf.Clamp(mapCamera.orthographicSize, minCameraOrthoSize, maxCameraOrthoSize);
	}
	public void OnDrag(PointerEventData eventData)
	{
		MapDrag(eventData);
	}
	private void MapDrag(PointerEventData eventData)
	{
		Vector3 movement = new Vector3(eventData.delta.x, eventData.delta.y, 0) * 2;
		Vector3 cameraMovePos = mapCamera.transform.position - movement;

		cameraMovePos.x = Mathf.Clamp(cameraMovePos.x, minCameraPosition.x, maxCameraPosition.x);
		cameraMovePos.y = Mathf.Clamp(cameraMovePos.y, minCameraPosition.y, maxCameraPosition.y);

		mapCamera.transform.position = cameraMovePos;
	}
}
