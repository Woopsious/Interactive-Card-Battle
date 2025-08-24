using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using static Woopsious.DamageData;

namespace Woopsious
{
	public class CardUi : MonoBehaviour
	{
		[HideInInspector] public CardHandler cardHandler;

		public TMP_Text cardNametext;
		public TMP_Text cardDescriptiontext;
		public RectTransform replaceCardButton;

		public bool PlayerCard { get; private set; }
		public bool Offensive { get; private set; }
		public DamageData DamageData { get; private set; }

		public bool selectable;

		public static event Action<CardUi> OnCardReplace;

		void Awake()
		{
			cardHandler = GetComponent<CardHandler>();
		}

		public void SetupCard(Entity cardOwner, AttackData AttackData, bool playerCard)
		{
			if (AttackData == null)
			{
				Debug.LogError("Attack data null");
				return;
			}

			PlayerCard = playerCard;
			Offensive = AttackData.offensive;

			string cardName = AttackData.attackName;
			gameObject.name = cardName;
			cardNametext.text = cardName;
			cardDescriptiontext.text = AttackData.CreateDescription();

			DamageData = new(cardOwner, AttackData.DamageData);
			DamageData.DamageValue = (int)(DamageData.DamageValue * cardOwner.damageDealtModifier.Value); //apply damage dealt modifier

			replaceCardButton.gameObject.SetActive(false);
		}

		//button call
		public void ReplaceCard()
		{
			OnCardReplace?.Invoke(this);
		}
	}
}