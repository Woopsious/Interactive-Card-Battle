using UnityEngine;
using UnityEngine.EventSystems;

public class InteractiveMapHandler : MonoBehaviour, IPointerDownHandler, IDragHandler
{
	public Canvas canvas;
	private RectTransform rectTransform;

	readonly float zoomSpeed = 0.5f;
	readonly float maxMapScale = 3f;
	readonly float minMapScale = 0.5f;

	private Vector2 offset;

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
	}

	private void Update()
	{
		MapScroll();
	}

	void MapScroll()
	{
		float scroll = Input.GetAxis("Mouse ScrollWheel");
		Vector3 localScale = rectTransform.localScale + new Vector3(scroll * zoomSpeed, scroll * zoomSpeed, 1);

		localScale.x = Mathf.Clamp(localScale.x, minMapScale, maxMapScale);
		localScale.y = Mathf.Clamp(localScale.y, minMapScale, maxMapScale);
		localScale.z = Mathf.Clamp01(1f);

		rectTransform.localScale = localScale;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		// Calculate offset from the pointer to the element’s pivot
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out offset);
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
			canvas.transform as RectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
		{
			rectTransform.localPosition = localPoint - offset;
		}
	}
}
