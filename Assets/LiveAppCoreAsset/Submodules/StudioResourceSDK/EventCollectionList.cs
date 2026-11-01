using System.Collections.Generic;
using UnityEngine;

namespace StudioResourceSDK.Domain
{
    [DisallowMultipleComponent]
    public class EventCollectionList : MonoBehaviour
    {
        [SerializeField] private List<MonoBehaviour> items = new List<MonoBehaviour>();

        private readonly Dictionary<int, EventCollectionGroup> _groupTable = new Dictionary<int, EventCollectionGroup>();
        private readonly Dictionary<int, EventCollectionElement> _eventTable = new Dictionary<int, EventCollectionElement>();

        public IReadOnlyList<MonoBehaviour> Items => items;

        private void Awake()
        {
            Refresh();
        }

        public void Refresh()
        {
            _groupTable.Clear();
            _eventTable.Clear();

            var registeredGroups = new HashSet<EventCollectionGroup>();
            var registeredEvents = new HashSet<EventCollectionElement>();
            var registeredIDs = new HashSet<int>();

            foreach (MonoBehaviour item in items)
            {
                RegisterItem(item, registeredGroups, registeredEvents, registeredIDs);
            }
        }

        public bool TryGetGroup(int groupID, out EventCollectionGroup group)
        {
            return _groupTable.TryGetValue(groupID, out group);
        }

        public bool TryGetEvent(int eventID, out EventCollectionElement element)
        {
            return _eventTable.TryGetValue(eventID, out element);
        }

        public bool TryInvoke(int eventID)
        {
            if (TryGetEvent(eventID, out EventCollectionElement element) == false || element.DataType != EventDataType.Trigger)
            {
                return false;
            }

            return element.Invoke();
        }

        public bool TryInvoke(int eventID, bool value)
        {
            if (TryGetEvent(eventID, out EventCollectionElement element) == false || element.DataType != EventDataType.Bool)
            {
                return false;
            }

            return element.Invoke(value);
        }

        public bool TryInvoke(int eventID, int value)
        {
            if (TryGetEvent(eventID, out EventCollectionElement element) == false || element.DataType != EventDataType.Int)
            {
                return false;
            }

            return element.Invoke(value);
        }

        public bool TryInvoke(int eventID, float value)
        {
            if (TryGetEvent(eventID, out EventCollectionElement element) == false || element.DataType != EventDataType.Float)
            {
                return false;
            }

            return element.Invoke(value);
        }

        public bool TryInvoke(int eventID, string value)
        {
            if (TryGetEvent(eventID, out EventCollectionElement element) == false || element.DataType != EventDataType.String)
            {
                return false;
            }

            return element.Invoke(value);
        }

        private void RegisterItem(MonoBehaviour item, HashSet<EventCollectionGroup> registeredGroups, HashSet<EventCollectionElement> registeredEvents, HashSet<int> registeredIDs)
        {
            if (item == null)
            {
                return;
            }

            if (item is EventCollectionElement element)
            {
                RegisterEvent(element, registeredEvents, registeredIDs);
                return;
            }

            if (item is EventCollectionGroup group)
            {
                RegisterGroup(group, registeredGroups, registeredEvents, registeredIDs);
                return;
            }

            Debug.LogError($"지원하지 않는 EventCollection Item입니다. Object={item.name}, Type={item.GetType().Name}", item);
        }

        private void RegisterGroup(EventCollectionGroup group, HashSet<EventCollectionGroup> registeredGroups, HashSet<EventCollectionElement> registeredEvents, HashSet<int> registeredIDs)
        {
            if (registeredGroups.Add(group) == false)
            {
                return;
            }

            if (RegisterID(group.GroupID, group, registeredIDs) == false)
            {
                return;
            }

            _groupTable.Add(group.GroupID, group);

            foreach (MonoBehaviour child in group.Items)
            {
                RegisterItem(child, registeredGroups, registeredEvents, registeredIDs);
            }
        }

        private void RegisterEvent(EventCollectionElement element, HashSet<EventCollectionElement> registeredEvents, HashSet<int> registeredIDs)
        {
            if (registeredEvents.Add(element) == false)
            {
                return;
            }

            if (RegisterID(element.EventID, element, registeredIDs) == false)
            {
                return;
            }

            _eventTable.Add(element.EventID, element);
        }

        private static bool RegisterID(int id, UnityEngine.Object target, HashSet<int> registeredIDs)
        {
            if (id == 0)
            {
                Debug.LogError($"EventCollection ID가 할당되지 않았습니다. Object={target.name}", target);
                return false;
            }

            if (registeredIDs.Add(id) == false)
            {
                Debug.LogError($"EventCollection ID가 중복되었습니다. ID={id}, Object={target.name}", target);
                return false;
            }

            return true;
        }

#if UNITY_EDITOR
        public void AddEditorItem(MonoBehaviour item)
        {
            if (EventCollectionGroup.IsValidItem(item) == false || items.Contains(item))
            {
                return;
            }

            items.Add(item);
        }
#endif
    }
}