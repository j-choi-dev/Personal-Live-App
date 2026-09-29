using StudioResourceSDK.Domain;
using UnityEngine;

namespace StudioResourceSDK.Application
{
    public class SceneResourceLifecycleContext : ISceneResourceLifecycleContext
    {
        private readonly SceneResourceList _sceneResourceList;
        private readonly SceneGimmickList _sceneGimmickList;

        public SceneResourceLifecycleContext(
            SceneResourceList sceneResourceList,
            SceneGimmickList sceneGimmickList)
        {
            _sceneResourceList = sceneResourceList;
            _sceneGimmickList = sceneGimmickList;
        }

        public bool RegisterGimmick(string resourceID, GameObject resourceObject)
        {
            if (resourceObject == null)
            {
                return false;
            }

            EventCollection eventCollection = resourceObject.GetComponent<EventCollection>();

            if (eventCollection == null)
            {
                return false;
            }

            return _sceneGimmickList.Add(resourceID, eventCollection);
        }

        public void RemoveGimmick(string resourceID)
        {
            _sceneGimmickList.Remove(resourceID);
        }
    }
}