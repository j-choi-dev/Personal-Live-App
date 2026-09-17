using StudioResourceSDK.Domain;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StudioResourceSDK.Editor
{
    public static class EventCollectionEditorUtility
    {
        public static int GenerateUniqueID()
        {
            HashSet<int> usedIDs = CollectUsedIDs();

            while (true)
            {
                int id = Guid.NewGuid().GetHashCode() & int.MaxValue;

                if (id != 0 && usedIDs.Contains(id) == false)
                {
                    return id;
                }
            }
        }

        private static HashSet<int> CollectUsedIDs()
        {
            var usedIDs = new HashSet<int>();

            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");

            foreach (string prefabGuid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab == null)
                {
                    continue;
                }

                EventCollection[] collections = prefab.GetComponentsInChildren<EventCollection>(true);

                foreach (EventCollection collection in collections)
                {
                    if (collection.ID != 0)
                    {
                        usedIDs.Add(collection.ID);
                    }
                }
            }

            EventCollection[] loadedCollections = Resources.FindObjectsOfTypeAll<EventCollection>();

            foreach (EventCollection collection in loadedCollections)
            {
                if (EditorUtility.IsPersistent(collection))
                {
                    continue;
                }

                if (collection.ID != 0)
                {
                    usedIDs.Add(collection.ID);
                }
            }

            return usedIDs;
        }
    }
}