using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Woopsious;
using static UnityEngine.GraphicsBuffer;

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

				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.energyCost)));
			}
			else
			{
				data.energyCost = 0;

				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.attackUseChance)));

				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.addDummyCardsForEffects)));

				if (data.addDummyCardsForEffects)
				{
					EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.effectDummyCards)));
				}
				else
				{
					data.effectDummyCards.Clear();
				}
			}

			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.DamageData)));

			// Write back changed values
			// This also handles all marking dirty, saving, undo/redo etc
			serializedObject.ApplyModifiedProperties();
		}
	}
}
