using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Woopsious
{
	public class TooltipUi : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerMoveHandler
	{
		public static Action<TooltipUi, string, Vector2> OnTooltipTextLinkClicked;
		public static Action<TooltipUi, string, Vector2> OnTooltipStatusEffectIconClicked;

		public static Action OnHideTooltip;

		bool mouseHovering;
		Vector3 mousePosition;

		public TooltipType tooltipType;
		public enum TooltipType
		{
			linkText, statusEffectIcon
		}

		public TMP_Text tmpTextBox;
		public StatusEffect statusEffect;

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

			switch (tooltipType)
			{
				case TooltipType.linkText:
					HandleLinkTextTooltips(eventData);
				break;

				case TooltipType.statusEffectIcon:
					HandleStatusEffectIconTooltips(eventData);
				break;
			}
		}

		void HandleLinkTextTooltips(PointerEventData eventData)
		{
			if (tmpTextBox.textInfo.linkInfo.Length <= 0) //no links to grab
			{
				OnHideTooltip?.Invoke();
				return;
			}

			var linkTaggedText = TMP_TextUtilities.FindIntersectingLink(tmpTextBox, eventData.position, null);

			if (linkTaggedText != -1)
			{
				TMP_LinkInfo linkInfo = tmpTextBox.textInfo.linkInfo[linkTaggedText];
				OnTooltipTextLinkClicked?.Invoke(this, GetStatusEffectDescriptionToolTip(linkInfo.GetLinkText()), eventData.position);
			}
			else
			{
				Debug.LogError("no link found");
			}
		}
		string GetStatusEffectDescriptionToolTip(string effectName)
		{
			foreach (StatusEffectsData statusEffectsData in GameManager.instance.statusEffectsDataTypes)
			{
				if (effectName == statusEffectsData.effectName)
					return statusEffectsData.CreateEffectDescription();
			}
			return "FAILED TO GET STATUS EFFECT DESCRIPTION";
		}

		void HandleStatusEffectIconTooltips(PointerEventData eventData)
		{
			OnTooltipStatusEffectIconClicked?.Invoke(this, statusEffect.CreateInGameDescription(), eventData.position);
		}
	}
}
