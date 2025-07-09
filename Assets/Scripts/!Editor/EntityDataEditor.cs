using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Woopsious
{
	[CustomEditor(typeof(EntityData), true)]
	public class EntityDataEditor : Editor
	{
		EntityData data;

		//move set attack data
		List<List<SerializedProperty>> attackDataList = new();
		List<List<Editor>> attackDataEditorList = new();
		List<List<bool>> showAttackDataList = new();

		void OnEnable()
		{
			data = (EntityData)target;
			BuildNestedLists();
		}

		public override void OnInspectorGUI()
		{
			//DrawDefaultInspector(); // for other non-HideInInspector fields

			// update the current values into the serialized object and propreties
			serializedObject.Update();

			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.entityName)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.entityDescription)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.isPlayer)));

			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.maxHealth)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.minHealPercentage)));

			if (data.isPlayer)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.maxCardsUsedPerTurn)));
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.maxDamageCardsUsedPerTurn)));
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.maxNonDamageCardsUsedPerTurn)));
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.maxReplaceableCardsPerTurn)));
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.cards)), true);
			}
            else
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.moveSetOrder)), true);

				BuildMoveSetEditorUi();
			}

			// Write back changed values
			// This also handles all marking dirty, saving, undo/redo etc
			serializedObject.ApplyModifiedProperties();
		}

		void BuildNestedLists()
		{
			attackDataList.Clear();
			attackDataEditorList.Clear();
			showAttackDataList.Clear();

			SerializedProperty moveSetOrderProp = serializedObject.FindProperty(nameof(data.moveSetOrder));

			for (int i = 0; i < moveSetOrderProp.arraySize; i++) //build nested lists
			{
				SerializedProperty moveSetDataProp = moveSetOrderProp.GetArrayElementAtIndex(i);
				SerializedProperty moveSetMovesProp = moveSetDataProp.FindPropertyRelative("moveSetMoves");

				List<SerializedProperty> nestedAttackDataList = new();
				List<Editor> nestedAttackDataEditorList = new();
				List<bool> nestedShowAttackDataList = new();

				for (int j = 0; j < moveSetMovesProp.arraySize; j++) //populate nested lists
				{
					nestedAttackDataList.Add(moveSetMovesProp.GetArrayElementAtIndex(j));
					nestedAttackDataEditorList.Add(null);
					nestedShowAttackDataList.Add(false);
				}

				attackDataList.Add(nestedAttackDataList);
				attackDataEditorList.Add(nestedAttackDataEditorList);
				showAttackDataList.Add(nestedShowAttackDataList);
			}
		}

		void BuildMoveSetEditorUi()
		{
			for (int i = 0; i < data.moveSetOrder.Count; i++) //loop through move sets
			{
				MoveSetData moveSetData = data.moveSetOrder[i];

				for (int j = 0; j < moveSetData.moveSetMoves.Count; j++) //loop through moves in move set to build ui
					BuildAttackDataUi(i, j);
			}
		}
		void BuildAttackDataUi(int i, int j)
		{
			if (attackDataList[i][j].objectReferenceValue == null) return;

			if (attackDataEditorList[i][j] == null || attackDataEditorList[i][j].target != attackDataList[i][j].objectReferenceValue)
				attackDataEditorList[i][j] = CreateEditor(attackDataList[i][j].objectReferenceValue);

			//grab attack name to set as fold out label
			AttackData data = attackDataList[i][j].objectReferenceValue as AttackData;
			string labelName = !string.IsNullOrEmpty(data.attackName) ? data.attackName + " " : "";
			showAttackDataList[i][j] = EditorGUILayout.Foldout(showAttackDataList[i][j], labelName + "Attack Data", true);

			//add indented box
			EditorGUILayout.BeginVertical("box");
			EditorGUI.indentLevel++;

			if (showAttackDataList[i][j])
			{
				EditorGUILayout.LabelField(labelName + "Attack Data", EditorStyles.boldLabel);
				attackDataEditorList[i][j].OnInspectorGUI();
			}

			EditorGUI.indentLevel--;
			EditorGUILayout.EndVertical();
		}
	}
}
