using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Woopsious
{
	[CustomEditor(typeof(EntityData), true)]
	public class EntityDataEditor : Editor
	{
		EntityData data;

		//move set attack data
		List<SerializedProperty> attackDataList = new();
		List<Editor> attackDataEditorList = new();
		List<bool> showAttackDataList = new();

		void OnEnable()
		{
			data = (EntityData)target;
			BuildLists();
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
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.enemyType)));
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.foundInLandTypes)));
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.entitySpawnChance)));
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.eliteEnemy)));

				GUILayout.Space(10);
				EditorGUILayout.LabelField("Create New Move For Entity", EditorStyles.boldLabel);
				if (GUILayout.Button("Create New Move AttackData"))
					CreateAttackDataScriptableObject();
				GUILayout.Space(10);

				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.moveSetOrder)), true);

				BuildAttackEditorUi();
			}

			// Write back changed values
			// This also handles all marking dirty, saving, undo/redo etc
			serializedObject.ApplyModifiedProperties();
		}

		void CreateAttackDataScriptableObject()
		{
			AttackData asset = CreateInstance<AttackData>();

			// Choose path to save
			string currentAssetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(data));
			string newPath = currentAssetPath + "/Moves";

			if (!AssetDatabase.IsValidFolder(newPath))
				AssetDatabase.CreateFolder(currentAssetPath, "Moves");

			newPath = AssetDatabase.GenerateUniqueAssetPath(newPath + "/NewAttackData.asset");

			// Save asset to the project
			AssetDatabase.CreateAsset(asset, newPath);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			// Select it in the Project window
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = asset;
		}
		void BuildLists()
		{
			attackDataList.Clear();
			attackDataEditorList.Clear();
			showAttackDataList.Clear();

			SerializedProperty moveSetOrderProp = serializedObject.FindProperty(nameof(data.moveSetOrder));

			for (int i = 0; i < data.moveSetOrder.Count; i++)
			{
				SerializedProperty moveSetDataProp = moveSetOrderProp.GetArrayElementAtIndex(i);
				SerializedProperty moveSetMovesProp = moveSetDataProp.FindPropertyRelative("moveSetMoves");

				for (int j = 0; j < moveSetMovesProp.arraySize; j++) //populate nested lists
				{
					Object newObjRef = moveSetMovesProp.GetArrayElementAtIndex(j).objectReferenceValue;
					if (attackDataList.Any(p => p.objectReferenceValue == newObjRef)) continue;

					attackDataList.Add(moveSetMovesProp.GetArrayElementAtIndex(j));
					attackDataEditorList.Add(null);
					showAttackDataList.Add(false);
				}
			}
		}
		void BuildAttackEditorUi()
		{
			for (int i = 0; i < attackDataList.Count; i++)
			{
				if (attackDataList[i].objectReferenceValue == null) return;

				if (attackDataEditorList[i] == null || attackDataEditorList[i].target != attackDataList[i].objectReferenceValue)
					attackDataEditorList[i] = CreateEditor(attackDataList[i].objectReferenceValue);

				//grab attack name to set as fold out label
				AttackData data = attackDataList[i].objectReferenceValue as AttackData;
				string labelName = !string.IsNullOrEmpty(data.attackName) ? data.attackName + " " : "";
				showAttackDataList[i] = EditorGUILayout.Foldout(showAttackDataList[i], labelName + "Attack Data", true);

				//add indented box
				EditorGUILayout.BeginVertical("box");
				EditorGUI.indentLevel++;

				if (showAttackDataList[i])
				{
					EditorGUILayout.LabelField(labelName + "Attack Data", EditorStyles.boldLabel);
					attackDataEditorList[i].OnInspectorGUI();
				}

				EditorGUI.indentLevel--;
				EditorGUILayout.EndVertical();
			}
		}
	}
}
