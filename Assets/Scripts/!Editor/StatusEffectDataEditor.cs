using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Woopsious
{
	[CustomEditor(typeof(StatusEffectsData), true)]
	public class StatusEffectDataEditor : Editor
	{
		StatusEffectsData data;

		//move set attack data
		List<SerializedProperty> attackDataList = new();
		List<Editor> attackDataEditorList = new();
		List<bool> showAttackDataList = new();

		void OnEnable()
		{
			data = (StatusEffectsData)target;
		}
		public override void OnInspectorGUI()
		{
			//DrawDefaultInspector(); // for other non-HideInInspector fields

			// update the current values into the serialized object and propreties
			serializedObject.Update();

			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.effectName)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.effectDescription)));

			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.hasStacks)));

			if (data.hasStacks)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.effectStacks)));
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.maxEffectStacks)));
			}

			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.hasLifetime)));

			if (data.hasLifetime)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.effectTurnLifetime)));
			}

			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.isDoT)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.isPercentage)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.effectValue)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.effectStatModifierType)));

			// Write back changed values
			// This also handles all marking dirty, saving, undo/redo etc
			serializedObject.ApplyModifiedProperties();
		}
	}
}
