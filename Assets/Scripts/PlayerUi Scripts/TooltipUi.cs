using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Woopsious
{
	public class TooltipUi : MonoBehaviour, IPointerClickHandler
	{
		public TMP_Text tmpTextBox;
		public static Action<string, Vector2> OnTooltipTextLinkClicked;
		public static Action OnHideTooltip;

		public void OnPointerClick(PointerEventData eventData)
		{
			if (tmpTextBox.textInfo.linkInfo.Length <= 0) //no links to grab
			{
				OnHideTooltip?.Invoke();
				return;
			}

			Vector3 mousePos = new(eventData.position.x, eventData.position.y, 0);
			Debug.LogError("mouse pos: " + eventData.position);

			//var linkTaggedText = TMP_TextUtilities.FindIntersectingLink(tmpTextBox, eventData.position, Camera.main);

			var linkTaggedText = TMP_TextUtilities.FindNearestLink(tmpTextBox, mousePos, Camera.main);

			if (linkTaggedText != -1)
			{
				TMP_LinkInfo linkInfo = tmpTextBox.textInfo.linkInfo[linkTaggedText];

				// Get the character index for the link
				int characterIndex = linkInfo.linkTextfirstCharacterIndex;

				// Get the position of the first character of the link
				Vector3 linkCharPosition = tmpTextBox.textInfo.characterInfo[characterIndex].bottomLeft;

				// Convert the position from local space to world space
				Vector3 worldPosition = tmpTextBox.transform.TransformPoint(linkCharPosition);

				// Optionally, log the world position for debugging
				Debug.Log("Link Position (World): " + worldPosition);

				OnTooltipTextLinkClicked?.Invoke(linkInfo.GetLinkText(), mousePos);
			}
			else
			{
				Debug.LogError("no link found");
			}
		}
	}
}
