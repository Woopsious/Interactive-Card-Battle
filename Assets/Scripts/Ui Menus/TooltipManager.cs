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
		private RectTransform closeTipButtonRect;

		public static Action<string, Vector2> OnMouseHover;
		public static Action OnMouseLoseFocus;

		void Awake()
		{
			instance = this;
			closeTipButtonRect = closeTipButton.GetComponent<RectTransform>();
			closeTipButton.onClick.AddListener(delegate { HideTip(); });
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

		void ShowTip(string tip, Vector2 mousePos)
		{
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
