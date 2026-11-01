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

                MonoBehaviour current = element.objectReferenceValue as MonoBehaviour;

                EditorGUI.BeginChangeCheck();

                MonoBehaviour selected = EditorGUI.ObjectField(rect, current, typeof(MonoBehaviour), true) as MonoBehaviour;

                if (EditorGUI.EndChangeCheck())
                {
                    if (selected == null || selected is EventCollectionGroup || selected is EventCollectionElement)
                    {
                        element.objectReferenceValue = selected;
                    }
                    else
                    {
                        Debug.LogWarning($"EventCollectionGroup 또는 EventCollectionElement만 등록할 수 있습니다. Type={selected.GetType().Name}", selected);
                    }
                }
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
        private SerializedProperty _groupIDProperty;
        private SerializedProperty _displayNameProperty;
        private SerializedProperty _itemsProperty;

        private void OnEnable()
        {
            _groupIDProperty = serializedObject.FindProperty("groupID");
            _displayNameProperty = serializedObject.FindProperty("displayName");
            _itemsProperty = serializedObject.FindProperty("items");

            _items = new ReorderableList(serializedObject, _itemsProperty, true, true, true, true);

            _items.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Group Items");
            };

            _items.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                SerializedProperty element = _itemsProperty.GetArrayElementAtIndex(index);

                rect.y += 2f;
                rect.height = EditorGUIUtility.singleLineHeight;

                MonoBehaviour current = element.objectReferenceValue as MonoBehaviour;

                EditorGUI.BeginChangeCheck();

                MonoBehaviour selected = EditorGUI.ObjectField(rect, current, typeof(MonoBehaviour), true) as MonoBehaviour;

                if (EditorGUI.EndChangeCheck())
                {
                    if (selected == null || selected is EventCollectionGroup || selected is EventCollectionElement)
                    {
                        element.objectReferenceValue = selected;
                    }
                    else
                    {
                        Debug.LogWarning($"EventCollectionGroup 또는 EventCollectionElement만 등록할 수 있습니다. Type={selected.GetType().Name}", selected);
                    }
                }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(_groupIDProperty, new GUIContent("Group ID"));
            }

            EditorGUILayout.PropertyField(_displayNameProperty, new GUIContent("Display Name"));

            EditorGUILayout.Space();

            _items.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(EventCollectionElement), true)]
    public class EventCollectionElementEditor : UnityEditor.Editor
    {
        private SerializedProperty _eventIDProperty;
        private SerializedProperty _eventDisplayNameProperty;

        private void OnEnable()
        {
            _eventIDProperty = serializedObject.FindProperty("eventID");
            _eventDisplayNameProperty = serializedObject.FindProperty("eventDisplayName");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(_eventIDProperty, new GUIContent("Event ID"));
            }

            EditorGUILayout.PropertyField(_eventDisplayNameProperty, new GUIContent("Display Name"));

            EventCollectionElement element = (EventCollectionElement)target;

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.EnumPopup("Data Type", element.DataType);
            }

            EditorGUILayout.Space();

            DrawPropertiesExcluding(serializedObject, "m_Script", "eventID", "eventDisplayName");

            serializedObject.ApplyModifiedProperties();
        }
    }
}