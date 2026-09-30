using StudioResourceSDK.Domain;
using System;
using UniRx;
using UnityEngine;

namespace StudioResourceSDK.Application
{
    public class SceneResourceLifecycleContext : ISceneResourceLifecycleContext, IDisposable
    {
        private readonly SceneResourceList _sceneResourceList;
        private readonly SceneGimmickList _sceneGimmickList;

        private readonly Subject<string> _onGimmickRegistered = new Subject<string>();
        public IObservable<string> OnGimmickRegistered => _onGimmickRegistered;

        private readonly Subject<string> _onGimmickRemoved = new Subject<string>();
        public IObservable<string> OnGimmickRemoved => _onGimmickRemoved;

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

            bool result = _sceneGimmickList.Add(resourceID, eventCollection);

            if (result)
            {
                _onGimmickRegistered.OnNext(resourceID);
            }

            return result;
        }

        public void RemoveGimmick(string resourceID)
        {
            bool result = _sceneGimmickList.Remove(resourceID);

            if (result)
            {
                _onGimmickRemoved.OnNext(resourceID);
            }
        }

        public void Dispose()
        {
            _onGimmickRegistered.Dispose();
            _onGimmickRemoved.Dispose();
        }
    }
}