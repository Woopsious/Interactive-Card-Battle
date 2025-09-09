using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Woopsious
{
	public class TooltipManager : MonoBehaviour
	{
		public static TooltipManager instance;

		public RectTransform tooltipWindow;
		public TextMeshProUGUI tipText;
		public Button closeTipButton;
		TooltipUi tooltipUiCurrentlyDisplayed;

		public static Action<string, Vector2> OnMouseHover;
		public static Action OnMouseLoseFocus;

		void Awake()
		{
			instance = this;
			closeTipButton.onClick.AddListener(() => HideTip());
		}

		void OnEnable()
		{
			TooltipUi.OnTooltipTextLinkClicked += ShowTip;
			TooltipUi.OnTooltipStatusEffectIconClicked += ShowTip;
			TooltipUi.OnHideTooltip += HideTip;
		}
		void OnDisable()
		{
			TooltipUi.OnTooltipTextLinkClicked -= ShowTip;
			TooltipUi.OnTooltipStatusEffectIconClicked -= ShowTip;
			TooltipUi.OnHideTooltip -= HideTip;
		}

		void ShowTip(TooltipUi tooltipUi, string tip, Vector2 mousePos)
		{
			if (tooltipUi == tooltipUiCurrentlyDisplayed && tip == tipText.text) //hide tip if same ui element and text content
			{
				HideTip();
				tooltipUiCurrentlyDisplayed = null;
				return;
			}

			tooltipUiCurrentlyDisplayed = tooltipUi;
			tipText.text = tip;

			tooltipWindow.sizeDelta = new Vector2(tipText.preferredWidth > 300 ? 300 :
				tipText.preferredWidth * 1.25f, tipText.preferredHeight * 1.25f);

			tooltipWindow.transform.position = new Vector2(mousePos.x + 25 + tooltipWindow.sizeDelta.x / 2, mousePos.y + 10);
			tooltipWindow.gameObject.SetActive(true);
		}
		void HideTip()
		{
			tooltipWindow.gameObject.SetActive(false);
		}
	}
}
