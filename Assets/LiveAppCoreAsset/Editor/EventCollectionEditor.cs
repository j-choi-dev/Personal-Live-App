using StudioResourceSDK.Domain;
using UnityEditor;
using UnityEngine;

namespace StudioResourceSDK.Editor
{
    [CustomEditor(typeof(EventCollection), true)]
    public class EventCollectionEditor : UnityEditor.Editor
    {
        private SerializedProperty _idProperty;

        private void OnEnable()
        {
            _idProperty = serializedObject.FindProperty("id");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Event Collection");

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(_idProperty, new GUIContent("ID"));
            }

            if (GUILayout.Button("Regenerate ID"))
            {
                EventCollection collection = (EventCollection)target;

                Undo.RecordObject(collection, "Regenerate EventCollection ID");

                int id = EventCollectionEditorUtility.GenerateUniqueID();
                collection.SetEditorID(id);

                EditorUtility.SetDirty(collection);

                Debug.Log($"EventCollection ID Regenerated :: ID={id}, Object={collection.name}", collection);
            }


            DrawPropertiesExcluding(serializedObject, "m_Script", "id");

            serializedObject.ApplyModifiedProperties();
        }
    }
}