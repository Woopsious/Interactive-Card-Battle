using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Woopsious
{
	[CustomEditor(typeof(EntityData), true)]
	public class EntityDataEditor : Editor
	{
		EntityData data;

		Editor attackDataEditor;

		SerializedProperty attackData;

		bool showAttackData = false;

		void OnEnable()
		{
			data = (EntityData)target;
			attackData = serializedObject.FindProperty(nameof(data.attackData));
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
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(data.attackData)), true);

				if (attackData.objectReferenceValue != null)
				{
					if (attackDataEditor == null || attackDataEditor.target != attackData.objectReferenceValue)
						attackDataEditor = CreateEditor(attackData.objectReferenceValue);

					//grab attack name to set as fold out label
					AttackData data = attackData.objectReferenceValue as AttackData;
					string labelName = !string.IsNullOrEmpty(data.attackName)? data.attackName + " " : "";
					showAttackData = EditorGUILayout.Foldout(showAttackData, labelName + "Attack Data", true);

					//add indented box
					EditorGUILayout.BeginVertical("box");
					EditorGUI.indentLevel++;

					if (showAttackData)
					{
						EditorGUILayout.LabelField(labelName + "Attack Data", EditorStyles.boldLabel);
						attackDataEditor.OnInspectorGUI();
					}

					EditorGUI.indentLevel--;
					EditorGUILayout.EndVertical();
				}
			}

			// Write back changed values
			// This also handles all marking dirty, saving, undo/redo etc
			serializedObject.ApplyModifiedProperties();
		}
	}

	/*
	[CustomPropertyDrawer(typeof(AttackData), true)]
	public class AttackDataDrawerUIE : PropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			return BuildUi(property);
		}

		private VisualElement BuildUi(SerializedProperty property)
		{
			var container = new VisualElement();
			var DamageContainer = new VisualElement();
			var attackContainer = new VisualElement();

			// Create property fields.
			var nameField = new PropertyField(property.FindPropertyRelative("attackName"));
			var descriptionField = new PropertyField(property.FindPropertyRelative("attackDescription"));

			var offensiveField = new PropertyField(property.FindPropertyRelative("offensive"));
			var alsoHealsField = new PropertyField(property.FindPropertyRelative("alsoHeals"));

			var damageField = new PropertyField(property.FindPropertyRelative("damage"));
			var damageTypeField = new PropertyField(property.FindPropertyRelative("damageType"));

			var attackCooldownTurnsField = new PropertyField(property.FindPropertyRelative("attackCooldownTurns"));
			var attackUseChanceField = new PropertyField(property.FindPropertyRelative("attackUseChance"));

			// Add fields to the container.
			container.Add(nameField);
			container.Add(descriptionField);

			container.Add(offensiveField);
			container.Add(alsoHealsField);

			DamageContainer.Add(damageField);
			DamageContainer.Add(damageTypeField);

			attackContainer.Add(attackCooldownTurnsField);
			attackContainer.Add(attackUseChanceField);

			return container;
		}
	}
	*/
}
