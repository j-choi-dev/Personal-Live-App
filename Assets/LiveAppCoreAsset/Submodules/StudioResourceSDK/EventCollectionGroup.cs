using System.Collections.Generic;
using UnityEngine;

namespace StudioResourceSDK.Domain
{
    public class EventCollectionGroup : MonoBehaviour
    {
        [SerializeField] private int groupID;
        [SerializeField] private string displayName;
        [SerializeField] private List<MonoBehaviour> items = new List<MonoBehaviour>();

        public int GroupID => groupID;
        public string DisplayName => displayName;
        public IReadOnlyList<MonoBehaviour> Items => items;

#if UNITY_EDITOR
        public void SetEditorIdentity(int id, string name)
        {
            groupID = id;
            displayName = name;
        }

        public void AddEditorItem(MonoBehaviour item)
        {
            if (IsValidItem(item) == false || items.Contains(item))
            {
                return;
            }

            items.Add(item);
        }
#endif

        public static bool IsValidItem(MonoBehaviour item)
        {
            return item is EventCollectionGroup || item is EventCollectionElement;
        }
    }
}