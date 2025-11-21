using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Woopsious.AttackData;

namespace Woopsious
{
	[CustomEditor(typeof(AttackData), true)]
	public class AttackDataEditor : Editor
	{
		AttackData data;

		void OnEnable()
		{
			data = (AttackData)target;
		}
		public override void OnInspectorGUI()
		{
			//DrawDefaultInspector(); // for other non-HideInInspector fields

			// update the current values into the serialized object and propreties
			serializedObject.Update();

			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.attackName)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.attackDescription)));

			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.isPlayerAttack)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.offensive)));

			if (data.isPlayerAttack)
			{
				data.attackUseChance = 0;

				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.cardRarity)));
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.cardDropChance)));
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.cardDrawChance)));

				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.energyCost)));
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.extraCardsToDraw)));
			}
			else
			{
				data.energyCost = 0;
				data.extraCardsToDraw = 0;

				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.attackUseChance)));
			}

			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.addDummyCardsForEffects)));

			if (data.addDummyCardsForEffects)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.effectDummyCards)));
			}
			else
			{
				data.effectDummyCards?.Clear();
			}

			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.DamageData)));

			if (data.cardRarity == CardRarity.Rare)
			{
				data.cardDropChance = 0.025f;
				data.cardDrawChance = 0.05f;
			}
			else if (data.cardRarity == CardRarity.Uncommon)
			{
				data.cardDropChance = 0.15f;
				data.cardDrawChance = 0.2f;
			}
			else if (data.cardRarity == CardRarity.Common)
			{
				data.cardDropChance = 0.75f;
				data.cardDrawChance = 0.75f;
			}
			else
			{
				data.cardDropChance = 0f;
				data.cardDrawChance = 0.75f;
			}

			// Write back changed values
			// This also handles all marking dirty, saving, undo/redo etc
			serializedObject.ApplyModifiedProperties();
		}
	}
}
