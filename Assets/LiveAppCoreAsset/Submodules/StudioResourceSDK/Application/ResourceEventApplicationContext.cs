using StudioResourceSDK.Domain;

namespace StudioResourceSDK.Application
{
    public class ResourceEventApplicationContext
    {
        private readonly SceneGimmickList _sceneGimmickList;

        public ResourceEventApplicationContext(SceneGimmickList sceneGimmickList)
        {
            _sceneGimmickList = sceneGimmickList;
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