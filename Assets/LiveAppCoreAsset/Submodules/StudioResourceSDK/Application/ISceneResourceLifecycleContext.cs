using System;
using UnityEngine;

namespace StudioResourceSDK.Application
{
    public interface ISceneResourceLifecycleContext
    {
        IObservable<string> OnGimmickRegistered { get; }
        IObservable<string> OnGimmickRemoved { get; }

        bool RegisterGimmick(string resourceID, GameObject resourceObject);
        void RemoveGimmick(string resourceID);
    }
}
