using UnityEditor;
using UnityEngine;

namespace StudioResourceSDK.Editor
{
    public class EventCollectionMenuItem : EditorWindow
    {
        private EventCollectionResourceType _resourceType = EventCollectionResourceType.Prop;
        private string _displayName = "New Resource";

        [MenuItem("LiveAppTool/Event/Create EventCollection Resource")]
        private static void Open()
        {
            EventCollectionMenuItem window = GetWindow<EventCollectionMenuItem>(true, "Create EventCollection");
            window.minSize = new Vector2(360f, 160f);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("EventCollection Resource", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            _resourceType = (EventCollectionResourceType)EditorGUILayout.EnumPopup("Resource Type", _resourceType);
            _displayName = EditorGUILayout.TextField("Display Name", _displayName);

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_displayName)))
            {
                if (GUILayout.Button("Create EventCollection Object", GUILayout.Height(32f)))
                {
                    EventCollectionEditorFactory.CreateResourceObject(_resourceType, _displayName, Selection.activeTransform);
                    Close();
                }
            }
        }
    }
}