using System;
using UnityEditor;
using UnityEngine;

namespace Woopsious
{
	[CustomEditor(typeof(EntityData))]
	public class EntityDataEditor : Editor
	{
		public EntityData data;

		void OnEnable()
		{
			data = (EntityData)target;
		}

		public override void OnInspectorGUI()
		{
			//DrawDefaultInspector(); // for other non-HideInInspector fields

			// update the current values into the serialized object and propreties
			serializedObject.Update();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("entityName"), new GUIContent(data.entityName));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("entityDescription"), new GUIContent(data.entityDescription));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.isPlayer)));

			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.maxHealth)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.minHealPercentage)));

			if (data.isPlayer)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.maxCardsUsedPerTurn)));
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.maxDamageCardsUsedPerTurn)));
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.maxNonDamageCardsUsedPerTurn)));
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.maxReplaceableCardsPerTurn)));
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.cards)));
			}
            else
            {
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.moveSetOrder)));
			}

			// Write back changed values
			// This also handles all marking dirty, saving, undo/redo etc
			serializedObject.ApplyModifiedProperties();
		}
	}
}
