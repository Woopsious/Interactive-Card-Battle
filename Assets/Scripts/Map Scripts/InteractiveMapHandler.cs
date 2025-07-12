using UnityEngine;
using UnityEngine.EventSystems;
using Woopsious;

public class InteractiveMapHandler : MonoBehaviour, IDragHandler
{
	public Camera mapCamera;

	readonly Vector2 maxCameraPosition = new(Screen.width * 2.5f, Screen.height * 2.5f);
	readonly Vector2 minCameraPosition = new(-Screen.width * 2.5f, -Screen.height * 2.5f);

	readonly float cameraBaseOrthoSize = 540;
	readonly float maxCameraOrthoSize = 540 * 4;
	readonly float minCameraOrthoSize = 540 / 4;

	readonly float zoomSpeed = 500;

	void Start()
	{
		mapCamera.gameObject.SetActive(true);
		mapCamera.orthographicSize = cameraBaseOrthoSize;
	}

	void OnEnable()
	{
		mapCamera.gameObject.SetActive(true);
	}
	void OnDisable()
	{
		mapCamera.gameObject.SetActive(false);
	}

	private void Update()
	{
		MapZoom();
	}

	void GenerateMapNodes()
	{
		//use similar logic when placing down enemies to build pick where to place nodes instead of using unities grid layout group comp
		//spawn and initilize the nodes then activate the first node
	}

	//map drag and zoom
	void MapZoom()
	{
		mapCamera.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
		mapCamera.orthographicSize = Mathf.Clamp(mapCamera.orthographicSize, minCameraOrthoSize, maxCameraOrthoSize);
	}
	public void OnDrag(PointerEventData eventData)
	{
		MapDrag(eventData);
	}
	void MapDrag(PointerEventData eventData)
	{
		Vector3 movement = new Vector3(eventData.delta.x, eventData.delta.y, 0) * 2;
		Vector3 cameraMovePos = mapCamera.transform.position - movement;

		cameraMovePos.x = Mathf.Clamp(cameraMovePos.x, minCameraPosition.x, maxCameraPosition.x);
		cameraMovePos.y = Mathf.Clamp(cameraMovePos.y, minCameraPosition.y, maxCameraPosition.y);

		mapCamera.transform.position = cameraMovePos;
	}
}
