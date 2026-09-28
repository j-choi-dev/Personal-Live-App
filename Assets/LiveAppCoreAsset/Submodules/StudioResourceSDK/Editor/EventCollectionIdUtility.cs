using StudioResourceSDK.Domain;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StudioResourceSDK.Editor
{
    public static class EventCollectionIdUtility
    {
        public static int GenerateUniqueID()
        {
            HashSet<int> usedIDs = CollectUsedIDs();
            return GenerateUniqueID(usedIDs);
        }

        public static int GenerateUniqueID(HashSet<int> usedIDs)
        {
            if (usedIDs == null)
            {
                usedIDs = new HashSet<int>();
            }

            while (true)
            {
                int id = GenerateID();

                if (id == 0 || usedIDs.Contains(id))
                {
                    continue;
                }

                usedIDs.Add(id);
                return id;
            }
        }

        private static void CollectIDs(GameObject rootObject, HashSet<int> usedIDs)
        {
            foreach (EventCollection collection in rootObject.GetComponentsInChildren<EventCollection>(true))
            {
                if (collection.CollectionID != 0)
                {
                    usedIDs.Add(collection.CollectionID);
                }
            }

            foreach (EventCollectionGroup group in rootObject.GetComponentsInChildren<EventCollectionGroup>(true))
            {
                if (group.GroupID != 0)
                {
                    usedIDs.Add(group.GroupID);
                }
            }

            foreach (EventCollectionElement element in rootObject.GetComponentsInChildren<EventCollectionElement>(true))
            {
                if (element.EventID != 0)
                {
                    usedIDs.Add(element.EventID);
                }
            }
        }

        private static void CollectSceneIDs(HashSet<int> usedIDs)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (scene.isLoaded == false)
                {
                    continue;
                }

                foreach (GameObject rootObject in scene.GetRootGameObjects())
                {
                    CollectIDs(rootObject, usedIDs);
                }
            }
        }

        private static void CollectIDs(GameObject rootObject, HashSet<int> usedIDs)
        {
            foreach (EventCollection collection in rootObject.GetComponentsInChildren<EventCollection>(true))
            {
                if (collection.CollectionID != 0)
                {
                    usedIDs.Add(collection.CollectionID);
                }
            }

            foreach (EventCollectionNode node in rootObject.GetComponentsInChildren<EventCollectionNode>(true))
            {
                if (node.ID != 0)
                {
                    usedIDs.Add(node.ID);
                }
            }
        }

        public static void RegenerateHierarchyIDs(GameObject rootObject)
        {
            if (rootObject == null)
            {
                return;
            }

            HashSet<int> usedIDs = CollectUsedIDs();

            EventCollection[] collections = rootObject.GetComponentsInChildren<EventCollection>(true);
            EventCollectionGroup[] groups = rootObject.GetComponentsInChildren<EventCollectionGroup>(true);
            EventCollectionElement[] elements = rootObject.GetComponentsInChildren<EventCollectionElement>(true);

            foreach (EventCollection collection in collections)
            {
                Undo.RecordObject(collection, "Regenerate EventCollection ID");
                collection.SetEditorIdentity(GenerateUniqueID(usedIDs), collection.DisplayName);
                EditorUtility.SetDirty(collection);
            }

            foreach (EventCollectionGroup group in groups)
            {
                Undo.RecordObject(group, "Regenerate EventCollection Group ID");
                group.SetEditorIdentity(GenerateUniqueID(usedIDs), group.DisplayName);
                EditorUtility.SetDirty(group);
            }

            foreach (EventCollectionElement element in elements)
            {
                Undo.RecordObject(element, "Regenerate EventCollection Event ID");
                element.SetEditorIdentity(GenerateUniqueID(usedIDs), element.EventDisplayName);
                EditorUtility.SetDirty(element);
            }
        }

        private static int GenerateID()
        {
            byte[] bytes = Guid.NewGuid().ToByteArray();

            int hash = BitConverter.ToInt32(bytes, 0);
            hash ^= BitConverter.ToInt32(bytes, 4);
            hash ^= BitConverter.ToInt32(bytes, 8);
            hash ^= BitConverter.ToInt32(bytes, 12);

            return hash & int.MaxValue;
        }

        private static void CollectPrefabIDs(HashSet<int> usedIDs)
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");

            foreach (string prefabGuid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab != null)
                {
                    CollectIDs(prefab, usedIDs);
                }
            }
        }
    }
}