using StudioResourceSDK.Domain;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace StudioResourceSDK.Editor
{
    public enum EventCollectionResourceType
    {
        Character,
        Background,
        Prop
    }

    public static class EventCollectionEditorFactory
    {
        public static EventCollection CreateResourceObject(EventCollectionResourceType resourceType, string displayName, Transform parent)
        {
            HashSet<int> usedIDs = EventCollectionIdUtility.CollectUsedIDs();

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = resourceType.ToString();
            }

            GameObject rootObject = CreateObject($"{displayName}_EventCollection", parent);

            EventCollection collection = Undo.AddComponent<EventCollection>(rootObject);
            collection.SetEditorIdentity(EventCollectionIdUtility.GenerateUniqueID(usedIDs), displayName);

            GameObject pivotObject = CreateObject("Transform Pivot", rootObject.transform);
            collection.SetEditorTransformPivot(pivotObject.transform);

            Undo.AddComponent<TransformEmitter>(pivotObject);
            BoolEmitter boolEmitter = Undo.AddComponent<BoolEmitter>(pivotObject);

            CreateObject(GetResourceObjectName(resourceType), pivotObject.transform);

            GameObject listObject = CreateObject("EventCollectionList", rootObject.transform);
            EventCollectionList eventList = Undo.AddComponent<EventCollectionList>(listObject);
            collection.SetEditorList(eventList);

            CreateDefaultEvents(eventList, listObject.transform, usedIDs);

            eventList.Refresh();

            EditorUtility.SetDirty(collection);
            EditorUtility.SetDirty(eventList);

            if (rootObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(rootObject.scene);
            }

            Selection.activeGameObject = rootObject;
            EditorGUIUtility.PingObject(rootObject);

            Debug.Log($"EventCollection Resource Created :: Type={resourceType}, CollectionID={collection.CollectionID}, DisplayName={displayName}", rootObject);

            return collection;
        }

        public static EventCollectionGroup CreateGroup(Transform parent, string displayName)
        {
            HashSet<int> usedIDs = EventCollectionIdUtility.CollectUsedIDs();

            GameObject groupObject = CreateObject(displayName, parent);
            EventCollectionGroup group = Undo.AddComponent<EventCollectionGroup>(groupObject);
            group.SetEditorIdentity(EventCollectionIdUtility.GenerateUniqueID(usedIDs), displayName);

            EditorUtility.SetDirty(group);

            return group;
        }

        public static EventCollectionElement CreateEventElement(Type eventType, Transform parent)
        {
            if (eventType == null || eventType.IsAbstract || typeof(EventCollectionElement).IsAssignableFrom(eventType) == false)
            {
                Debug.LogError($"EventCollectionElement Type이 올바르지 않습니다. Type={eventType}");
                return null;
            }

            HashSet<int> usedIDs = EventCollectionIdUtility.CollectUsedIDs();

            GameObject eventObject = CreateObject(eventType.Name, parent);
            EventCollectionElement element = Undo.AddComponent(eventObject, eventType) as EventCollectionElement;

            if (element == null)
            {
                Undo.DestroyObjectImmediate(eventObject);
                return null;
            }

            element.SetEditorIdentity(EventCollectionIdUtility.GenerateUniqueID(usedIDs), eventType.Name);
            EditorUtility.SetDirty(element);

            return element;
        }

        private static void CreateDefaultEvents(EventCollectionList eventList, Transform parent, HashSet<int> usedIDs)
        {
            GameObject defaultGroupObject = CreateObject("Default Event Group", parent);

            EventCollectionGroup defaultGroup = Undo.AddComponent<EventCollectionGroup>(defaultGroupObject);
            defaultGroup.SetEditorIdentity(EventCollectionIdUtility.GenerateUniqueID(usedIDs), "Default Event Group");

            GameObject toggleObject = CreateObject("Toggle Event", defaultGroupObject.transform);

            ToggleEvent toggleEvent = Undo.AddComponent<ToggleEvent>(toggleObject);
            toggleEvent.SetEditorIdentity(EventCollectionIdUtility.GenerateUniqueID(usedIDs), "Toggle");

            GameObject transformObject = CreateObject("Transform Event", defaultGroupObject.transform);

            EventCollectionGroup transformGroup = Undo.AddComponent<EventCollectionGroup>(transformObject);
            transformGroup.SetEditorIdentity(EventCollectionIdUtility.GenerateUniqueID(usedIDs), "Transform");

            EventCollectionGroup positionGroup = CreateTransformGroup(
                "Position",
                transformObject.transform,
                usedIDs,
                "Position X",
                "Position Y",
                "Position Z"
            );

            EventCollectionGroup rotationGroup = CreateTransformGroup(
                "Rotation",
                transformObject.transform,
                usedIDs,
                "Rotation X",
                "Rotation Y",
                "Rotation Z"
            );

            EventCollectionGroup scaleGroup = CreateTransformGroup(
                "Scale",
                transformObject.transform,
                usedIDs,
                "Scale X",
                "Scale Y",
                "Scale Z"
            );

            transformGroup.AddEditorItem(positionGroup);
            transformGroup.AddEditorItem(rotationGroup);
            transformGroup.AddEditorItem(scaleGroup);

            defaultGroup.AddEditorItem(toggleEvent);
            defaultGroup.AddEditorItem(transformGroup);

            eventList.AddEditorItem(defaultGroup);

            EditorUtility.SetDirty(toggleEvent);
            EditorUtility.SetDirty(positionGroup);
            EditorUtility.SetDirty(rotationGroup);
            EditorUtility.SetDirty(scaleGroup);
            EditorUtility.SetDirty(transformGroup);
            EditorUtility.SetDirty(defaultGroup);
            EditorUtility.SetDirty(eventList);
        }

        private static EventCollectionGroup CreateTransformGroup(string displayName, Transform parent, HashSet<int> usedIDs, params string[] eventDisplayNames)
        {
            GameObject groupObject = CreateObject(displayName, parent);

            EventCollectionGroup group = Undo.AddComponent<EventCollectionGroup>(groupObject);
            group.SetEditorIdentity(EventCollectionIdUtility.GenerateUniqueID(usedIDs), displayName);

            foreach (string eventDisplayName in eventDisplayNames)
            {
                TransformFloatEvent transformEvent = Undo.AddComponent<TransformFloatEvent>(groupObject);
                transformEvent.SetEditorIdentity(EventCollectionIdUtility.GenerateUniqueID(usedIDs), eventDisplayName);

                group.AddEditorItem(transformEvent);

                EditorUtility.SetDirty(transformEvent);
            }

            EditorUtility.SetDirty(group);

            return group;
        }

        private static GameObject CreateObject(string objectName, Transform parent)
        {
            GameObject gameObject = new GameObject(objectName);

            Undo.RegisterCreatedObjectUndo(gameObject, $"Create {objectName}");

            if (parent != null)
            {
                gameObject.transform.SetParent(parent, false);
            }

            return gameObject;
        }

        private static string GetResourceObjectName(EventCollectionResourceType resourceType)
        {
            switch (resourceType)
            {
                case EventCollectionResourceType.Character:
                    return "Character Resource";

                case EventCollectionResourceType.Background:
                    return "Background Resource";

                case EventCollectionResourceType.Prop:
                    return "Prop Resource";

                default:
                    return "Resource Object";
            }
        }
    }
}