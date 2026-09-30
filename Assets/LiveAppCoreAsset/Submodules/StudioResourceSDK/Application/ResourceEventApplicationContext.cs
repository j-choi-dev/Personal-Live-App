using StudioResourceSDK.Domain;
using System;
using UnityEngine;

namespace StudioResourceSDK.Application
{
    public class ResourceEventApplicationContext : IResourceEventApplicationContext
    {
        private readonly SceneGimmickList _sceneGimmickList;

        public ResourceEventApplicationContext(SceneGimmickList sceneGimmickList)
        {
            _sceneGimmickList = sceneGimmickList;
        }
        public bool TryGetGimmickDescriptor(string resourceID, out ResourceGimmickDescriptor descriptor)
        {
            descriptor = null;

            if (_sceneGimmickList.TryGet(resourceID, out SceneGimmickInfo gimmickInfo) == false)
            {
                return false;
            }

            EventCollection collection = gimmickInfo.EventCollection;

            if (collection == null || collection.EventList == null)
            {
                return false;
            }

            collection.RefreshEvents();

            descriptor = new ResourceGimmickDescriptor(
                resourceID,
                collection.CollectionID,
                collection.DisplayName
            );

            foreach (MonoBehaviour item in collection.EventList.Items)
            {
                GimmickEntryDescriptor entry = CreateEntryDescriptor(item);

                if (entry != null)
                {
                    descriptor.Add(entry);
                }
            }

            return true;
        }

        private static GimmickEntryDescriptor CreateEntryDescriptor(MonoBehaviour item)
        {
            if (item == null)
            {
                return null;
            }

            if (item is EventCollectionGroup group)
            {
                GimmickGroupDescriptor groupDescriptor = new GimmickGroupDescriptor(
                    group.GroupID,
                    group.DisplayName
                );

                foreach (MonoBehaviour child in group.Items)
                {
                    GimmickEntryDescriptor childDescriptor = CreateEntryDescriptor(child);

                    if (childDescriptor != null)
                    {
                        groupDescriptor.Add(childDescriptor);
                    }
                }

                return new GimmickEntryDescriptor(groupDescriptor);
            }

            if (item is EventCollectionElement element)
            {
                GimmickEventDescriptor eventDescriptor = new GimmickEventDescriptor(
                    element.EventID,
                    element.EventDisplayName,
                    ConvertValueType(element.DataType)
                );

                return new GimmickEntryDescriptor(eventDescriptor);
            }

            return null;
        }

        private static ResourceEventValueType ConvertValueType(EventDataType type)
        {
            switch (type)
            {
                case EventDataType.Trigger:
                    return ResourceEventValueType.Trigger;

                case EventDataType.Bool:
                    return ResourceEventValueType.Bool;

                case EventDataType.Int:
                    return ResourceEventValueType.Int;

                case EventDataType.Float:
                    return ResourceEventValueType.Float;

                case EventDataType.String:
                    return ResourceEventValueType.String;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(type),
                        type,
                        null
                    );
            }
        }

        public bool InvokeTrigger(string resourceID, int collectionID, int eventID)
        {
            if (TryGetEventList(resourceID, collectionID, out EventCollectionList eventList) == false)
            {
                return false;
            }

            return eventList.TryInvoke(eventID);
        }

        public bool InvokeBool(string resourceID, int collectionID, int eventID, bool value)
        {
            if (TryGetEventList(resourceID, collectionID, out EventCollectionList eventList) == false)
            {
                return false;
            }

            return eventList.TryInvoke(eventID, value);
        }

        public bool InvokeInt(string resourceID, int collectionID, int eventID, int value)
        {
            if (TryGetEventList(resourceID, collectionID, out EventCollectionList eventList) == false)
            {
                return false;
            }

            return eventList.TryInvoke(eventID, value);
        }

        public bool InvokeFloat(string resourceID, int collectionID, int eventID, float value)
        {
            if (TryGetEventList(resourceID, collectionID, out EventCollectionList eventList) == false)
            {
                return false;
            }

            return eventList.TryInvoke(eventID, value);
        }

        public bool InvokeString(string resourceID, int collectionID, int eventID, string value)
        {
            if (TryGetEventList(resourceID, collectionID, out EventCollectionList eventList) == false)
            {
                return false;
            }

            return eventList.TryInvoke(eventID, value);
        }

        private bool TryGetEventList(string resourceID, int collectionID, out EventCollectionList eventList)
        {
            eventList = null;

            if (_sceneGimmickList.TryGetEventCollection(resourceID, collectionID, out EventCollection collection) == false)
            {
                return false;
            }

            eventList = collection.EventList;

            return eventList != null;
        }
    }
}