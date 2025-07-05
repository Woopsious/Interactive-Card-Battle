using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Woopsious
{
	public static class EditorList
	{
		public static void Show(SerializedProperty list, bool showListSize = true)
		{
			EditorGUILayout.PropertyField(list);
			EditorGUI.indentLevel += 1;

			if (list.isExpanded)
			{
				if (showListSize)
				{
					EditorGUILayout.PropertyField(list.FindPropertyRelative("Array.size"));
				}
				for (int i = 0; i < list.arraySize; i++)
				{
					EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i));
				}
			}

			EditorGUI.indentLevel -= 1;
		}
	}

	public static class EditorUtilities
	{
		public static void DrawInspectorExcept(this SerializedObject serializedObject, string[] fieldsToSkip)
		{
			serializedObject.Update();
			SerializedProperty prop = serializedObject.GetIterator();
			if (prop.NextVisible(true))
			{
				do
				{
					if (fieldsToSkip.Any(prop.name.Contains))
						continue;

					EditorGUILayout.PropertyField(serializedObject.FindProperty(prop.name), true);
				}
				while (prop.NextVisible(false));
			}
			serializedObject.ApplyModifiedProperties();
		}
	}
}
