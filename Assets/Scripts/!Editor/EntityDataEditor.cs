using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Woopsious
{
	[CustomEditor(typeof(EntityData))]
	public class EntityDataEditor : Editor
	{
		public EntityData data;

		public List<MoveSetData> moveSetData;

		void OnEnable()
		{
			data = (EntityData)target;
			moveSetData = data.moveSetOrder;
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
