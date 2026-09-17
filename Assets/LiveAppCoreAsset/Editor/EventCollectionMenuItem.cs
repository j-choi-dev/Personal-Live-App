using StudioResourceSDK.Domain;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace StudioResourceSDK.Editor
{
    public static class EventCollectionMenuItem
    {
        private const string MENU_NAME = "LiveAppTool/Event/Create EventObject";

        [MenuItem(MENU_NAME)]
        private static void CreateEventObject()
        {
            GameObject eventObject = new GameObject("EventObject");

            Undo.RegisterCreatedObjectUndo(eventObject, "Create EventObject");

            if (Selection.activeTransform != null)
            {
                eventObject.transform.SetParent(Selection.activeTransform, false);
            }

            EventCollection collection = Undo.AddComponent<EventCollection>(eventObject);

            int id = EventCollectionEditorUtility.GenerateUniqueID();
            collection.SetEditorID(id);

            EditorUtility.SetDirty(collection);

            if (eventObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(eventObject.scene);
            }

            Selection.activeGameObject = eventObject;
            EditorGUIUtility.PingObject(eventObject);

            Debug.Log($"EventObject Created :: ID={id}, Object={eventObject.name}", eventObject);
        }
    }
}