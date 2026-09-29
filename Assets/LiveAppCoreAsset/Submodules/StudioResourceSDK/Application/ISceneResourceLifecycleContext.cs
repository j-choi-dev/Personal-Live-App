using UnityEngine;

public interface ISceneResourceLifecycleContext
{
    bool RegisterGimmick(string resourceID, GameObject resourceObject);
    void RemoveGimmick(string resourceID);
}
