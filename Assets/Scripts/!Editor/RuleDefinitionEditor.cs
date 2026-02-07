using UnityEditor;

namespace Woopsious
{
	[CustomEditor(typeof(RuleDefinition), true)]
	public class RuleDefinitionEditor : Editor
	{
		RuleDefinition data;

		void OnEnable()
		{
			data = (RuleDefinition)target;
		}
		public override void OnInspectorGUI()
		{
			//DrawDefaultInspector(); // for other non-HideInInspector fields

			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.Space();

			// update the current values into the serialized object and propreties
			serializedObject.Update();

			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.hasMultipleConditions)));
			EditorGUILayout.Space();

			if (data.hasMultipleConditions)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.allConditionsNeedToBeMet)));
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.ruleConditions)));
			}
			else
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.ruleCondition)));


			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.hasMultipleOutcomes)));
			EditorGUILayout.Space();

			if (data.hasMultipleOutcomes)
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.ruleOutcomes)));
			else
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.ruleOutcome)));

			// Write back changed values
			// This also handles all marking dirty, saving, undo/redo etc
			serializedObject.ApplyModifiedProperties();
		}
	}
}
