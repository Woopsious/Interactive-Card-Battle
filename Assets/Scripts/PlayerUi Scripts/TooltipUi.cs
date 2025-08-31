using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Woopsious
{
	public class TooltipUi : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerMoveHandler
	{
		public TMP_Text tmpTextBox;
		public static Action<string, Vector2> OnTooltipTextLinkClicked;
		public static Action OnHideTooltip;

		bool mouseHovering;
		Vector3 mousePosition;

		public void OnPointerEnter(PointerEventData eventData)
		{
			mouseHovering = true;
		}
		public void OnPointerMove(PointerEventData eventData)
		{
			if (!mouseHovering) return;
			mousePosition = new(eventData.position.x, eventData.position.y, 0);
		}
		public void OnPointerExit(PointerEventData eventData)
		{
			mouseHovering = false;
			mousePosition = Vector3.zero;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Right) return;

			if (tmpTextBox.textInfo.linkInfo.Length <= 0) //no links to grab
			{
				OnHideTooltip?.Invoke();
				return;
			}

			var linkTaggedText = TMP_TextUtilities.FindIntersectingLink(tmpTextBox, eventData.position, null);

			if (linkTaggedText != -1)
			{
				TMP_LinkInfo linkInfo = tmpTextBox.textInfo.linkInfo[linkTaggedText];
				OnTooltipTextLinkClicked?.Invoke(GetStatusEffectDescriptionToolTip(linkInfo.GetLinkText()), eventData.position);
			}
			else
			{
				Debug.LogError("no link found");
			}
		}

		string GetStatusEffectDescriptionToolTip(string effectName)
		{
			foreach (StatusEffectsData statusEffectsData in SpawnManager.instance.statusEffectsDataTypes)
			{
				if (effectName == statusEffectsData.effectName)
					return statusEffectsData.CreateTooltipDescription();
			}
			return "FAILED TO GET STATUS EFFECT DESCRIPTION";
		}
	}
}
