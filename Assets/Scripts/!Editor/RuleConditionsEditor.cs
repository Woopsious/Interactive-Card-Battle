using UnityEditor;

namespace Woopsious
{
	[CustomEditor(typeof(EntityCondition), true)]
	public class EntityConditionEditor : Editor
	{
		EntityCondition data;

		void OnEnable()
		{
			data = (EntityCondition)target;
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

			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.useListVersion)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.sourceCheckOnly)));

			if (!data.useListVersion)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.sourceEntityType)));

				if (!data.sourceCheckOnly)
				{
					EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.targetEntityType)));
				}
			}
			else
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.sourceEntityTypes)));

				if (!data.sourceCheckOnly)
				{
					EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.targetEntityTypes)));
				}
			}

			// Write back changed values
			// This also handles all marking dirty, saving, undo/redo etc
			serializedObject.ApplyModifiedProperties();
		}
	}
}

