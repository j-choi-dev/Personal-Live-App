using StudioResourceSDK.Domain;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace StudioResourceSDK.Editor
{
    [CustomEditor(typeof(EventCollectionList))]
    public class EventCollectionListEditor : UnityEditor.Editor
    {
        private ReorderableList _items;

        private void OnEnable()
        {
            SerializedProperty itemsProperty = serializedObject.FindProperty("items");

            _items = new ReorderableList(serializedObject, itemsProperty, true, true, true, true);

            _items.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Event Collection List");
            };

            _items.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                SerializedProperty element = itemsProperty.GetArrayElementAtIndex(index);

                rect.y += 2f;
                rect.height = EditorGUIUtility.singleLineHeight;

                EditorGUI.PropertyField(rect, element, GUIContent.none);
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox("Child의 EventCollectionGroup 또는 EventCollectionElement를 List에 Drag & Drop 해서 등록해.", MessageType.Info);

            _items.DoLayoutList();

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            EventCollectionList list = (EventCollectionList)target;

            if (GUILayout.Button("Refresh Registry"))
            {
                list.Refresh();
            }
        }
    }

    [CustomEditor(typeof(EventCollectionGroup))]
    public class EventCollectionGroupEditor : UnityEditor.Editor
    {
        private ReorderableList _items;

        private void OnEnable()
        {
            SerializedProperty itemsProperty = serializedObject.FindProperty("items");

            _items = new ReorderableList(serializedObject, itemsProperty, true, true, true, true);

            _items.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Group Items");
            };

            _items.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                SerializedProperty element = itemsProperty.GetArrayElementAtIndex(index);

                rect.y += 2f;
                rect.height = EditorGUIUtility.singleLineHeight;

                EditorGUI.PropertyField(rect, element, GUIContent.none);
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty idProperty = serializedObject.FindProperty("id");
            SerializedProperty displayNameProperty = serializedObject.FindProperty("displayName");

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(idProperty, new GUIContent("Group ID"));
            }

            EditorGUILayout.PropertyField(displayNameProperty, new GUIContent("Display Name"));

            EditorGUILayout.Space();

            _items.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(EventCollectionElement), true)]
    public class EventCollectionElementEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty idProperty = serializedObject.FindProperty("id");
            SerializedProperty displayNameProperty = serializedObject.FindProperty("displayName");

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(idProperty, new GUIContent("Event ID"));
            }

            EditorGUILayout.PropertyField(displayNameProperty, new GUIContent("Display Name"));

            EventCollectionElement element = (EventCollectionElement)target;

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.EnumPopup("Data Type", element.DataType);
            }

            DrawPropertiesExcluding(serializedObject, "m_Script", "id", "displayName");

            serializedObject.ApplyModifiedProperties();
        }
    }
}