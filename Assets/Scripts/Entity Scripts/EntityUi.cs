using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

namespace Woopsious
{
	public class EntityUi : MonoBehaviour
	{
		private Entity entity;

		[Header("Ui Elements")]
		public TMP_Text entityNameText;
		public GameObject energySymbol;
		public TMP_Text energyText;
		public TMP_Text entityHealthText;
		public TMP_Text entityBlockText;
		public GameObject turnIndicator;

		private RectTransform rectTransform;
		protected Image imageHighlight;

		private void Awake()
		{
			entity = GetComponent<Entity>();
			rectTransform = GetComponent<RectTransform>();
			imageHighlight = GetComponent<Image>();

			TurnOrderManager.OnStartTurn += UpdateTurnIndicator;
			entity.OnEntityInitialize += UpdateEntityDisplayName;
			PlayerEntity.OnEnergyChange += UpdateEnergyUi;
			entity.OnHealthChange += UpdateHealthUi;
			entity.OnBlockChange += UpdateBlockUi;
			entity.OnUpdateHighlightColour += UpdateHighlightUi;

			TogglePlayerOnlyUi();
		}

		private void OnDestroy()
		{
			TurnOrderManager.OnStartTurn -= UpdateTurnIndicator;
			entity.OnEntityInitialize -= UpdateEntityDisplayName;
			PlayerEntity.OnEnergyChange -= UpdateEnergyUi;
			entity.OnHealthChange -= UpdateHealthUi;
			entity.OnBlockChange -= UpdateBlockUi;
			entity.OnUpdateHighlightColour -= UpdateHighlightUi;
		}

		private void TogglePlayerOnlyUi()
		{
			if (entity is PlayerEntity)
			{
				energySymbol.SetActive(true);
			}
			else
			{
				energySymbol.SetActive(false);
			}
		}

		private void UpdateEntityDisplayName(string entityName)
		{
			gameObject.name = entityName;
			entityNameText.text = entityName;
		}

		private void UpdateEnergyUi(int energy)
		{
			energyText.text = $"{energy}";
		}
		private void UpdateHealthUi(int health, int maxHealth)
		{
			entityHealthText.text = "HP:" + health + "/" + maxHealth;
		}
		private void UpdateBlockUi(int block)
		{
			entityBlockText.text = "BLOCK: " + block;
		}

		private void UpdateHighlightUi(Color color)
		{
			imageHighlight.color = color;
		}

		private void UpdateTurnIndicator(Entity entity)
		{
			if (this.entity == entity)
			{
				turnIndicator.SetActive(true);

				if (this.entity is PlayerEntity)
					rectTransform.anchoredPosition = new Vector2(0, -190);
			}
			else
			{
				turnIndicator.SetActive(false);
				if (this.entity is PlayerEntity)
					rectTransform.anchoredPosition = new Vector2(0, -325);
			}
		}
	}
}
